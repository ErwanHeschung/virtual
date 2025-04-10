using UnityEngine;

public class FitnessTracker : MonoBehaviour
{
    private Vector3 startCenter;
    private CreatureBuilder builder;
    public float fitness;

    void Start()
    {
        builder = transform.parent?.GetComponent<CreatureBuilder>();
        if (builder != null)
        {
            startCenter = builder.GetCenterOfMass();
        }
        else
        {
            startCenter = transform.position;
            Debug.LogWarning("CreatureBuilder not found in parent!");
        }
    }

    void Update()
    {
        if (builder != null)
        {
            Vector3 currentCenter = builder.GetCenterOfMass();
            fitness = Vector3.Distance(startCenter, currentCenter);
        }
    }

    public float GetFitness()
    {
        return fitness;
    }
}