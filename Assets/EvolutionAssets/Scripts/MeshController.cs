
using UnityEngine;

public class MeshController : MonoBehaviour
{

    private void FixedUpdate()
    {
        //SpawnFood();
    }

    public void SpawnFood()
    {
        GameObject[] existingFood = GameObject.FindGameObjectsWithTag("Food");

        int diff = 200 - existingFood.Length;
        for (int i = 0; i < diff; i++)
        {
            float spawnRange = 80f;
            float spawnX = Random.Range(-spawnRange, spawnRange);
            float spawnZ = Random.Range(-spawnRange, spawnRange);
            Vector3 spawnPosition = new Vector3(spawnX, 3.3f, spawnZ);

            GameObject foodPrefab = Resources.Load<GameObject>("apple");
            Instantiate(foodPrefab, spawnPosition, Quaternion.identity);
        }

    }
}
