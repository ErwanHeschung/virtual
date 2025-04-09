using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EvolutionManager : MonoBehaviour
{
    public static EvolutionManager Instance { get; private set; }  // Singleton instance to get it from buttons
    public GameObject limbPrefab;
    public int populationSize = 10;
    public List<CreatureDNA> currentGeneration = new List<CreatureDNA>();
    private List<GameObject> population = new List<GameObject>();
    public float bestFitness = 0f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        RunEvolution();
    }

    void RunEvolution()
    {
        currentGeneration.Clear();

        for (int i = 0; i < populationSize; i++)
            currentGeneration.Add(RandomDNA());


        foreach (CreatureDNA dna in currentGeneration)
        {
            GameObject creature = new GameObject("Creature");
            creature.transform.position = new Vector3(Random.Range(-80, 80), 5, Random.Range(-80, 80));

            var builder = creature.AddComponent<CreatureBuilder>();
            builder.limbPrefab = limbPrefab;
            builder.dna = dna;
            builder.Build();

            population.Add(creature);
        }

    }

    public void nextGen()
    {
        var fitnessResults = population
            .Select(c => new
            {
                obj = c,
                dna = c.GetComponent<CreatureBuilder>().dna,
                fitness = c.GetComponent<CreatureBuilder>().bestFitness
            })
            .OrderByDescending(x => x.fitness)
            .ToList();

        Debug.Log($"Best fitness: {fitnessResults[0].fitness:F2}");
        bestFitness = fitnessResults[0].fitness;
        currentGeneration.Clear();
        for (int i = 0; i < 5; i++)
        {
            currentGeneration.Add(fitnessResults[i].dna);
        }

        while (currentGeneration.Count < populationSize)
        {
            var parent = fitnessResults[Random.Range(0, 3)].dna;
            currentGeneration.Add(MutateDNA(parent));
        }

        foreach (var c in population)
            Destroy(c);
        population.Clear();

        RunEvolution();
    }

    CreatureDNA RandomDNA()
    {
        CreatureDNA dna = ScriptableObject.CreateInstance<CreatureDNA>();

        var root = new BodyPartGene
        {
            offset = Vector3.zero,
            rotation = Vector3.zero,
            scale = new Vector3(1.5f, 0.5f, 1.5f),
            jointType = JointType.Fixed,
            jointStrength = 0,
            frequency = 0,
            amplitude = 0,
            phase = 0,
            children = new List<BodyPartGene>()
        };

        for (int i = 0; i < 4; i++)
        {
            float x = 1.0f;
            float z = -1.5f + i * 1.0f;

            root.children.Add(RandomLeg(new Vector3(-x, 0, z), 45, i));
            root.children.Add(RandomLeg(new Vector3(x, 0, z), -45, i));
        }

        dna.root = root;
        return dna;
    }

    BodyPartGene RandomLeg(Vector3 offset, float angle, int index)
    {
        return new BodyPartGene
        {
            offset = offset,
            rotation = new Vector3(0, 0, angle),
            scale = new Vector3(0.2f, 1.5f, 0.2f),
            jointType = JointType.Hinge,
            jointStrength = Random.Range(30f, 100f),
            frequency = Random.Range(1f, 3f),
            amplitude = Random.Range(15f, 40f),
            phase = index * Mathf.PI / 2f,
            children = new List<BodyPartGene> {
            new BodyPartGene { // Second leg segment
                offset = new Vector3(0, -1.2f, 0),
                rotation = new Vector3(0, 0, 20),
                scale = new Vector3(0.15f, 1.2f, 0.15f),
                jointType = JointType.Hinge,
                jointStrength = Random.Range(20f, 80f),
                frequency = Random.Range(1f, 3f),
                amplitude = Random.Range(10f, 30f),
                phase = index * Mathf.PI / 2f + 0.5f
            }
        }
        };
    }

    JointType RandomJointType()
    {
        return (JointType)Random.Range(0, System.Enum.GetValues(typeof(JointType)).Length);
    }

    CreatureDNA MutateDNA(CreatureDNA parent)
    {
        CreatureDNA child = ScriptableObject.CreateInstance<CreatureDNA>();
        child.root = CloneAndMutatePart(parent.root);
        return child;
    }

    BodyPartGene CloneAndMutatePart(BodyPartGene original)
    {
        var mutated = new BodyPartGene
        {
            offset = original.offset + new Vector3(Random.Range(-0.2f, 0.2f), 0, Random.Range(-0.2f, 0.2f)),
            rotation = original.rotation,
            scale = original.scale,
            jointType = Random.value < 0.1f ? RandomJointType() : original.jointType,
            jointStrength = Mathf.Clamp(original.jointStrength + Random.Range(-10f, 10f), 10f, 100f),
            frequency = Mathf.Clamp(original.frequency + Random.Range(-0.3f, 0.3f), 0.1f, 3f),
            amplitude = Mathf.Clamp(original.amplitude + Random.Range(-5f, 5f), 5f, 50f),
            phase = original.phase + Random.Range(-0.5f, 0.5f),
            children = new List<BodyPartGene>()
        };

        foreach (var child in original.children)
        {
            mutated.children.Add(CloneAndMutatePart(child));
        }

        return mutated;
    }
}
