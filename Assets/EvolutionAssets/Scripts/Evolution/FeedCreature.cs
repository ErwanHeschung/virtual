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

    void Start()
    {
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
            builder.segmentCount++;

            foreach (GameObject limb in builder.builtParts)
                Destroy(limb);
            builder.builtParts.Clear();
            builder.Build();
        }
    }

    void AddRandomLegToCreature(BodyPartGene root)
    {
        List<BodyPartGene> allParts = FlattenTree(root);
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
        List<BodyPartGene> result = new();
        Queue<BodyPartGene> queue = new();
        queue.Enqueue(root);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            result.Add(current);
            foreach (var child in current.children)
                queue.Enqueue(child);
        }

        return result;
    }
}
