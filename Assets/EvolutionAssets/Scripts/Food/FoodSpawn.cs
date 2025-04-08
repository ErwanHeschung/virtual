using System.Collections;
using UnityEngine;

public class FoodSpawn : MonoBehaviour
{
    public GameObject foodPrefab;
    private GameObject[] trees;
    private int initialFoodCount = 20;
    private int maxFoodCount = 50;
    private float spawnInterval = 1f;
    public GameObject indicatorPrefab;

    void Start()
    {
        trees = GameObject.FindGameObjectsWithTag("Tree");

        for (int i = 0; i < initialFoodCount; i++)
        {
            if (trees.Length == 0) break;
            GameObject tree = trees[Random.Range(0, trees.Length)];
            SpawnFoodOnTree(tree);
        }

        StartCoroutine(SpawnFoodOverTime());
    }

    IEnumerator SpawnFoodOverTime()
    {
        while (true)
        {
            if (trees.Length == 0 || GameObject.FindGameObjectsWithTag("Food").Length >= maxFoodCount)
                yield break;

            GameObject tree = trees[Random.Range(0, trees.Length)];
            SpawnFoodOnTree(tree);
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    void SpawnFoodOnTree(GameObject tree)
    {
        Vector3 randomOffset = new Vector3(Random.Range(-1.5f, 1.5f), 0f, Random.Range(-1.5f, 1.5f));
        Vector3 spawnPos = tree.transform.position + randomOffset;

        spawnPos.y = 7f;
        Quaternion rotation = Quaternion.Euler(-90f, 0f, 0f);
        GameObject food = Instantiate(foodPrefab, spawnPos, rotation);

        Canvas canvas = FindObjectOfType<Canvas>();
        GameObject indicator = Instantiate(indicatorPrefab, canvas.transform);
        indicator.SetActive(false);

        GrabbableFood grabbableFoodScript = food.AddComponent<GrabbableFood>();
        grabbableFoodScript.indicator = indicator;
    }
}
