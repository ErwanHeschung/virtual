using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EvolutionManager : MonoBehaviour
{
    public static EvolutionManager Instance { get; private set; }
    public GameObject limbPrefab;
    public int populationSize = 10;
    public List<CreatureDNA> currentGeneration = new List<CreatureDNA>();
    private List<GameObject> population = new List<GameObject>();
    public float bestFitness = 0f;
    public int evolutionStage = 0;

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
        // Divide population equally among three creature types.
        int baseCount = populationSize / 3;
        int remainder = populationSize % 3;
        int snakeCount = baseCount;
        int dnaCount = baseCount;
        int flyingCount = baseCount;
        // Distribute any remainder items (if populationSize is not a multiple of 3)
        if (remainder > 0) { snakeCount++; remainder--; }
        if (remainder > 0) { dnaCount++; remainder--; }
        if (remainder > 0) { flyingCount++; remainder--; }

        // Generate required DNA for DNA creatures.
        currentGeneration.Clear();
        for (int i = 0; i < dnaCount; i++)
        {
            currentGeneration.Add(RandomDNA(false)); // normal (non-flying) DNA
        }
        population.Clear();

        // --- Create Snake Creatures ---
        for (int i = 0; i < snakeCount; i++)
        {
            GameObject creature = new GameObject("Creature_Snake_" + i);
            creature.AddComponent<Rigidbody>().mass = 1f;
            creature.transform.position = new Vector3(Random.Range(-80, 80), 5, Random.Range(-80, 80));
            CreatureBuilder builder = creature.AddComponent<CreatureBuilder>();
            builder.limbPrefab = limbPrefab;
            builder.useSnakeBuilder = true;
            builder.useFlyingBuilder = false;
            builder.dna = null;
            builder.Build();
            creature.AddComponent<NeuralController>();
            population.Add(creature);
        }

        // --- Create DNA Creatures ---
        for (int i = 0; i < dnaCount; i++)
        {
            GameObject creature = new GameObject("Creature_DNA_" + i);
            creature.AddComponent<Rigidbody>().mass = 1f;
            creature.transform.position = new Vector3(Random.Range(-80, 80), 5, Random.Range(-80, 80));
            CreatureBuilder builder = creature.AddComponent<CreatureBuilder>();
            builder.limbPrefab = limbPrefab;
            builder.useSnakeBuilder = false;
            builder.useFlyingBuilder = false;
            builder.dna = currentGeneration[i];
            builder.Build();
            creature.AddComponent<NeuralController>();
            population.Add(creature);
        }

        // --- Create Flying Creatures ---
        for (int i = 0; i < flyingCount; i++)
        {
            GameObject creature = new GameObject("Creature_Flying_" + i);
            creature.AddComponent<Rigidbody>().mass = 1f;
            creature.transform.position = new Vector3(Random.Range(-80, 80), 5, Random.Range(-80, 80));
            CreatureBuilder builder = creature.AddComponent<CreatureBuilder>();
            builder.limbPrefab = limbPrefab;
            builder.useSnakeBuilder = false;
            builder.useFlyingBuilder = true;
            builder.dna = RandomDNA(true);
            builder.dna.isFlying = true; // Ensure the DNA is marked as flying
            builder.Build();
            creature.AddComponent<NeuralController>();
            population.Add(creature);
        }
    }

    public void nextGen()
    {
        evolutionStage++;
        var fitnessResults = population
            .Select(c =>
            {
                CreatureBuilder builder = c.GetComponent<CreatureBuilder>();
                CreatureDNA dna = builder.dna;
                // For flying creatures, if they have no DNA, assign new flying DNA.
                if (builder.useFlyingBuilder && dna == null)
                {
                    dna = RandomDNA(true);
                    builder.dna = dna;
                }
                FitnessTracker tracker = c.GetComponentInChildren<FitnessTracker>();
                float fit = (tracker != null) ? tracker.fitness : 0f;
                return new { obj = c, dna = dna, fitness = fit };
            })
            .Where(x => x.dna != null)  // Only include creatures with valid DNA.
            .OrderByDescending(x => x.fitness)
            .ToList();

        if (fitnessResults.Count == 0)
        {
            Debug.LogWarning("No creatures with valid DNA were found!");
            return;
        }

        Debug.Log($"Best fitness: {fitnessResults[0].fitness:F2}");
        bestFitness = fitnessResults[0].fitness;
        currentGeneration.Clear();
        // Use the top 5 creatures’ DNA to seed the next generation.
        for (int i = 0; i < Mathf.Min(5, fitnessResults.Count); i++)
        {
            currentGeneration.Add(fitnessResults[i].dna);
        }

        while (currentGeneration.Count < populationSize)
        {
            int parentPoolSize = Mathf.Max(3, fitnessResults.Count / 2);
            var parentDNA = fitnessResults[Random.Range(0, parentPoolSize)].dna;
            if (parentDNA.isFlying)
            {
                currentGeneration.Add(RandomDNA(true));
            }
            else
            {
                currentGeneration.Add(MutateDNA(parentDNA));
            }
        }
        foreach (var c in population)
        {
            Destroy(c);
        }
        population.Clear();
        RunEvolution();
    }

    CreatureDNA RandomDNA(bool isFlying)
    {
        CreatureDNA dna = ScriptableObject.CreateInstance<CreatureDNA>();
        dna.isFlying = isFlying;

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
        if (!isFlying)
        {
            for (int i = 0; i < 4; i++)
            {
                float x = 1.0f;
                float z = -1.5f + i * 1.0f;
                root.children.Add(RandomLeg(new Vector3(-x, 0, z), 45, i));
                root.children.Add(RandomLeg(new Vector3(x, 0, z), -45, i));
            }
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
                new BodyPartGene
                { // Second leg segment
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
        if (parent.isFlying)
        {
            return RandomDNA(true);
        }

        if (parent.brain == null)
        {
            parent.brain = new NeuralNetwork();
            parent.brain.Initialize();
        }

        CreatureDNA child = ScriptableObject.CreateInstance<CreatureDNA>();
        child.root = MutateOrAddNew(parent.root);
        child.brain = parent.brain.CloneAndMutate();

        for (int i = 0; i < parent.feedBoost; i++)
        {
            child.root.children.Add(CreateRandomLimb());
        }
        child.feedBoost = 0;
        child.isFlying = false;
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

    BodyPartGene MutateOrAddNew(BodyPartGene original)
    {
        BodyPartGene mutated = new BodyPartGene
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
            mutated.children.Add(MutateOrAddNew(child));
        }
        if (Random.value < 0.5f)
        {
            Debug.Log("Adding new limb");
            mutated.children.Add(CreateRandomLimb());
        }
        return mutated;
    }

    BodyPartGene CreateRandomLimb()
    {
        return new BodyPartGene
        {
            offset = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)),
            rotation = new Vector3(0, 0, Random.Range(-90f, 90f)),
            scale = new Vector3(0.2f, 1.2f, 0.2f),
            jointType = JointType.Hinge,
            jointStrength = Random.Range(20f, 80f),
            frequency = Random.Range(0.5f, 2f),
            amplitude = Random.Range(10f, 40f),
            phase = Random.Range(0f, Mathf.PI * 2f),
            children = new List<BodyPartGene>()
        };
    }
}