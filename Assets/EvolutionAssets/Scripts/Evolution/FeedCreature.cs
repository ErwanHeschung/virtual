using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FeedCreature : MonoBehaviour
{
    public float feedRange = 3f;
    public GameObject indicatorPrefab;
    private GameObject currentIndicator;
    private GameObject nearestCreature;
    private Canvas canvas;
    public int colectedData = 0;

    public static FeedCreature Instance { get; private set; }

    void Start()
    {

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        canvas = FindObjectOfType<Canvas>();
        if (indicatorPrefab != null && canvas != null)
        {
            currentIndicator = Instantiate(indicatorPrefab, canvas.transform);
            currentIndicator.SetActive(false);
            TextMeshProUGUI text = currentIndicator.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = "Press 'E' to Feed";
            }
            else
            {
                Debug.LogWarning("TextMeshProUGUI component not found in the indicator prefab.");
            }
        }
    }

    void Update()
    {
        nearestCreature = FindNearestFeedableCreature();
        if (currentIndicator != null)
        {
            bool shouldShow = nearestCreature != null &&
                              PlayerInventory.Instance != null &&
                              PlayerInventory.Instance.foodCount > 0;
            currentIndicator.SetActive(shouldShow);
            if (shouldShow)
            {
                Vector3 screenPos = Camera.main.WorldToScreenPoint(nearestCreature.transform.position);
                currentIndicator.transform.position = screenPos;
            }
        }

        if (Input.GetKeyDown(KeyCode.E) && nearestCreature != null && PlayerInventory.Instance.foodCount > 0)
        {
            TryFeedNearestCreature();
        }
    }

    GameObject FindNearestFeedableCreature()
    {
        float nearestDistance = feedRange;
        GameObject[] creatures = GameObject.FindGameObjectsWithTag("Creature");
        GameObject nearest = null;
        foreach (GameObject creature in creatures)
        {
            float distance = Vector3.Distance(creature.transform.position, transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = creature;
            }
        }
        return nearest;
    }

    void TryFeedNearestCreature()
    {
        CreatureBuilder builder = nearestCreature.GetComponentInParent<CreatureBuilder>();
        if (builder == null)
        {
            Debug.LogWarning("CreatureBuilder component not found on the nearest creature.");
            return;
        }

        if (PlayerInventory.Instance.RemoveFood(1))
        {
            colectedData++;
            if (builder.useSnakeBuilder)
            {
                if (builder.dna != null)
                {
                    builder.dna.feedBoost++;
                    Debug.Log("Snake feed boost increased! It will translate to additional segments next generation.");
                }
            }
            else if (builder.useFlyingBuilder)
            {
                FlightController fc = builder.GetComponentInChildren<FlightController>();
                if (fc != null)
                {
                    fc.liftForce += 5f;
                    Debug.Log("Flying creature fed: lift force increased!");
                }
                else
                {
                    Debug.LogWarning("FlightController component missing in flying creature.");
                }
            }
            else
            {
                if (builder.dna != null && builder.dna.root != null)
                {
                    AddRandomLegToCreature(builder.dna.root);
                    Debug.Log("DNA creature fed: a new leg has been added to its DNA for next generation.");
                }
                else
                {
                    Debug.LogWarning("CreatureDNA or its root is missing in the CreatureBuilder.");
                }
            }
        }
    }

    void AddRandomLegToCreature(BodyPartGene root)
    {
        List<BodyPartGene> allParts = FlattenTree(root);
        if (allParts.Count == 0)
        {
            Debug.LogWarning("No body parts found in the DNA tree.");
            return;
        }

        BodyPartGene parent = allParts[Random.Range(0, allParts.Count)];
        BodyPartGene newLeg = new BodyPartGene
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
        parent.children.Add(newLeg);
    }

    List<BodyPartGene> FlattenTree(BodyPartGene root)
    {
        List<BodyPartGene> result = new List<BodyPartGene>();
        Queue<BodyPartGene> queue = new Queue<BodyPartGene>();
        queue.Enqueue(root);
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            result.Add(current);
            foreach (var child in current.children)
            {
                queue.Enqueue(child);
            }
        }
        return result;
    }
}