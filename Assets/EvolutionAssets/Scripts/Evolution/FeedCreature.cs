using UnityEngine;
using TMPro;

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
            //change the text to "Feed"
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

        if (Input.GetKeyDown(KeyCode.E) && nearestCreature != null)
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
        print("Nearest creature: " + nearest?.name + " at distance: " + nearestDistance);
        return nearest;
    }

    void TryFeedNearestCreature()
    {
        CreatureBuilder creatureBuilder = nearestCreature.GetComponent<CreatureBuilder>();
        if (creatureBuilder == null)
        {
            Debug.LogWarning("CreatureBuilder component not found on the nearest creature.");
            return;
        }
        if (PlayerInventory.Instance.RemoveFood(1))
        {
            CreatureDNA dna = creatureBuilder.dna;

            dna.limbCount++;

            dna.limbs.Add(new LimbGene
            {
                positionOffset = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f)),
                scale = Vector3.one,
                amplitude = Random.Range(10f, 40f),
                frequency = Random.Range(0.5f, 2f),
                phase = Random.Range(0f, Mathf.PI * 2f)
            });


            foreach (GameObject limb in creatureBuilder.builtLimbs)
            {
                Destroy(limb);
            }
            creatureBuilder.builtLimbs.Clear();

            creatureBuilder.Build();
            Debug.Log(creatureBuilder.dna.limbCount);
        }
    }
}