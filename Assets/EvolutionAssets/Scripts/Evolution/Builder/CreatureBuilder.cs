using System.Collections.Generic;
using UnityEngine;

public class CreatureBuilder : MonoBehaviour
{
    public GameObject limbPrefab;
    public CreatureDNA dna;
    public List<GameObject> builtParts = new List<GameObject>();
    public int segmentCount = 10;
    public float segmentSpacing = 0.5f;
    public Color snakeColor = new Color(0f, 1f, 0f);

    // Call this to build the creature
    public void Build()
    {
        BuildSnake();
    }

    public void BuildSnake()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        builtParts.Clear();

        for (int i = 0; i < segmentCount; i++)
        {
            Vector3 pos = transform.position + transform.forward * i * segmentSpacing;
            GameObject limb = Instantiate(limbPrefab, pos, Quaternion.identity, transform);
            limb.name = "Limb_" + i;
            limb.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            Renderer rend = limb.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.color = snakeColor;
            }

            Rigidbody rb = limb.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = limb.AddComponent<Rigidbody>();
            }
            rb.mass = 2f;

            if (limb.GetComponent<Collider>() == null)
            {
                limb.AddComponent<BoxCollider>();
            }

            if (i > 0)
            {
                HingeJoint joint = limb.AddComponent<HingeJoint>();
                Rigidbody prevRb = builtParts[i - 1].GetComponent<Rigidbody>();
                joint.connectedBody = prevRb;
                joint.anchor = Vector3.zero;
                joint.axis = Vector3.up;
                joint.useMotor = true;

                JointMotor motor = joint.motor;
                motor.force = 100f;
                motor.targetVelocity = 80f;
                motor.freeSpin = false;
                joint.motor = motor;
            }

            if (i == 0)
            {
                limb.tag = "Creature";
                if (limb.GetComponent<FitnessTracker>() == null)
                {
                    limb.AddComponent<FitnessTracker>();
                }
            }

            builtParts.Add(limb);
        }
    }

    public Vector3 GetCenterOfMass()
    {
        if (builtParts.Count == 0)
            return transform.position;

        Vector3 center = Vector3.zero;
        foreach (GameObject limb in builtParts)
        {
            center += limb.transform.position;
        }
        center /= builtParts.Count;
        return center;
    }
}