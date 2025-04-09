using UnityEngine;

public class FitnessTracker : MonoBehaviour
{
    private Vector3 startPos;
    public float fitness;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        fitness = Vector3.Distance(startPos, transform.position);

    }

    public float GetFitness()
    {
        return fitness;
    }
}
