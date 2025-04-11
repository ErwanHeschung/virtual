using UnityEngine;

public class NeuralController : MonoBehaviour
{
    public CreatureDNA creatureDNA;
    private NeuralNetwork brain;
    private Rigidbody rb;
    private GameObject firstLimb;

    void Start()
    {
        if (creatureDNA == null)
        {
            return;
        }

        if (creatureDNA.brain == null)
        {
            creatureDNA.brain = new NeuralNetwork();
            creatureDNA.brain.Initialize();
        }

        firstLimb = GetFirstLimb();
        if (firstLimb != null)
        {
            rb = firstLimb.GetComponent<Rigidbody>();
            if (rb == null)
            {
                Debug.LogWarning("No Rigidbody found on the first limb. NeuralController expects one for force-based movement.");
            }
        }
        else
        {
            Debug.LogWarning("No first limb found. Falling back to parent's Rigidbody.");
            rb = GetComponent<Rigidbody>();
        }

        // Clone and initialize the brain
        brain = creatureDNA.brain.Clone();
        brain.Initialize();
        if (brain == null)
        {
            Debug.LogWarning("Brain is null after initialization.");
        }
    }

    void Update()
    {
        if (brain == null)
        {
            Debug.LogWarning("Brain is null in Update!");
            return;
        }
        float[] inputs = GatherInputs();
        float[] outputs = brain.FeedForward(inputs);
        ApplyOutputs(outputs);
    }

    GameObject GetFirstLimb()
    {
        FitnessTracker tracker = GetComponentInChildren<FitnessTracker>();
        if (tracker != null)
            return tracker.gameObject;
        return null;
    }

    float[] GatherInputs()
    {
        if (firstLimb != null)
        {
            Rigidbody limbRb = firstLimb.GetComponent<Rigidbody>();
            float velocityMagnitude = (limbRb != null) ? limbRb.velocity.magnitude : 0f;
            Vector3 pos = firstLimb.transform.position;
            return new float[] { pos.x, pos.y, pos.z, velocityMagnitude };
        }
        else
        {
            float velocityMagnitude = (rb != null) ? rb.velocity.magnitude : 0f;
            Vector3 pos = transform.position;
            return new float[] { pos.x, pos.y, pos.z, velocityMagnitude };
        }
    }

    void ApplyOutputs(float[] outputs)
    {
        if (outputs.Length < 3)
        {
            Debug.LogWarning("Outputs array is too short. Expected at least 3 outputs.");
            return;
        }
        if (rb != null && outputs.Length >= 3)
        {

            float forceMultiplier = 200f;

            Vector3 force = new Vector3(outputs[0], outputs[1], outputs[2]) * forceMultiplier;
            rb.AddForce(force);
        }
    }
}