using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EvolutionManager : MonoBehaviour
{
    public GameObject limbPrefab;
    public int populationSize = 10;
    public float generationDuration = 15f;

    private List<GameObject> population = new List<GameObject>();

    void Start()
    {
        StartCoroutine(RunEvolution());
    }

    IEnumerator RunEvolution()
    {
        List<CreatureDNA> currentGeneration = new List<CreatureDNA>();

        for (int i = 0; i < populationSize; i++)
            currentGeneration.Add(RandomDNA());

        while (true)
        {
            // 1. Spawn generation
            foreach (CreatureDNA dna in currentGeneration)
            {
                GameObject creature = new GameObject("Creature");
                creature.transform.position = new Vector3(Random.Range(-80, 80), 5, Random.Range(-80, 80));

                var builder = creature.AddComponent<CreatureBuilder>();
                builder.limbPrefab = limbPrefab;
                builder.dna = dna;
                builder.Build();

                creature.AddComponent<FitnessTracker>();
                population.Add(creature);
            }

            // 2. Wait for simulation
            yield return new WaitForSeconds(generationDuration);

            // 3. Evaluate fitness
            var fitnessResults = population
                .Select(c => new { dna = c.GetComponent<CreatureBuilder>().dna, fitness = c.GetComponent<FitnessTracker>().GetFitness() })
                .OrderByDescending(x => x.fitness)
                .ToList();

            Debug.Log($"Best fitness: {fitnessResults[0].fitness:F2}");

            // 4. Keep top 3 and mutate
            currentGeneration.Clear();
            for (int i = 0; i < 3; i++)
                currentGeneration.Add(fitnessResults[i].dna);

            while (currentGeneration.Count < populationSize)
            {
                var parent = fitnessResults[Random.Range(0, 3)].dna;
                currentGeneration.Add(MutateDNA(parent));
            }

            // 5. Clean up
            foreach (var c in population)
                Destroy(c);
            population.Clear();
        }
    }

    CreatureDNA RandomDNA()
    {
        CreatureDNA dna = new CreatureDNA();
        dna.limbCount = 4;

        for (int i = 0; i < dna.limbCount; i++)
        {
            dna.limbs.Add(new LimbGene
            {
                positionOffset = new Vector3(1.5f * i, 0, 0),
                scale = Vector3.one,
                amplitude = Random.Range(10f, 40f),
                frequency = Random.Range(0.5f, 2f),
                phase = Random.Range(0f, Mathf.PI * 2f),
                jointType = RandomJointType(),
                jointStrength = Random.Range(10f, 100f)
            });
        }
        return dna;
    }

    JointType RandomJointType()
    {
        return (JointType)Random.Range(0, System.Enum.GetValues(typeof(JointType)).Length);
    }

    CreatureDNA MutateDNA(CreatureDNA parent)
    {
        CreatureDNA child = new CreatureDNA();
        child.limbCount = parent.limbCount;

        foreach (var limb in parent.limbs)
        {
            var mutated = new LimbGene
            {
                positionOffset = limb.positionOffset + new Vector3(Random.Range(-0.3f, 0.3f), 0, 0),
                scale = limb.scale,
                amplitude = Mathf.Clamp(limb.amplitude + Random.Range(-5f, 5f), 5f, 50f),
                frequency = Mathf.Clamp(limb.frequency + Random.Range(-0.2f, 0.2f), 0.1f, 3f),
                phase = limb.phase + Random.Range(-0.5f, 0.5f),
                jointType = Random.value < 0.1f ? RandomJointType() : limb.jointType, // 10% chance to mutate
                jointStrength = Mathf.Clamp(limb.jointStrength + Random.Range(-10f, 10f), 10f, 100f)
            };
            child.limbs.Add(mutated);
        }

        return child;
    }
}
