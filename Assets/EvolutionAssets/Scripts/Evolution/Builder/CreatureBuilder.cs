using System.Collections.Generic;
using UnityEngine;

public class CreatureBuilder : MonoBehaviour
{
    [Header("Prefabs and DNA")]
    public GameObject limbPrefab;
    public CreatureDNA dna;

    [Header("Build Mode Options")]
    public bool useSnakeBuilder = true;
    public bool useFlyingBuilder = false;

    [Header("Snake Builder Options")]
    public int segmentCount = 10;
    public float segmentSpacing = 0.5f;
    public Color snakeColor = new Color(0f, 1f, 0f);

    [Header("DNA Builder Options")]
    public Color dnaColor = new Color(0f, 1f, 1f);

    [Header("Flying Builder Options")]
    public int flyingSegmentCount = 8;
    public float flyingSegmentSpacing = 1.0f;
    public Color flyingColor = new Color(1f, 0.5f, 0f);

    public List<GameObject> builtParts = new List<GameObject>();

    public void Build()
    {
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        builtParts.Clear();

        if (useFlyingBuilder)
        {
            BuildFlyingCreature();
        }
        else if (useSnakeBuilder)
        {
            BuildSnake();
        }
        else
        {
            if (dna == null || dna.root == null)
            {
                Debug.LogWarning("CreatureDNA or its root gene is not assigned.");
                return;
            }
            BuildCreature(dna.root, transform, true);
        }
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
            GameObject limb = Instantiate(limbPrefab, pos, transform.rotation, transform);
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
                rb.mass = 2f;
                rb.drag = 0.1f;
                rb.angularDrag = 0.05f;
                rb.sleepThreshold = 0;
            }

            // Ensure the limb has a Collider.
            if (limb.GetComponent<Collider>() == null)
            {
                limb.AddComponent<BoxCollider>();
            }

            if (i > 0)
            {
                // Add and configure the HingeJoint.
                HingeJoint joint = limb.AddComponent<HingeJoint>();
                Rigidbody prevRb = builtParts[i - 1].GetComponent<Rigidbody>();
                joint.connectedBody = prevRb;
                joint.anchor = Vector3.zero;
                joint.axis = Vector3.up;
                joint.useMotor = true;

                JointMotor motor = joint.motor;
                motor.force = 300f;
                motor.targetVelocity = 0f;
                motor.freeSpin = false;
                joint.motor = motor;

                OscillatingJoint oscJoint = limb.AddComponent<OscillatingJoint>();
                BodyPartGene gene = new BodyPartGene
                {
                    frequency = 2f,
                    amplitude = 15f,
                    jointStrength = 10f,
                    phase = i * 0.3f
                };
                oscJoint.Init(gene);
            }
            else
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

    public void BuildFlyingCreature()
    {
        for (int i = 0; i < flyingSegmentCount; i++)
        {
            Vector3 pos = transform.position + transform.right * i * flyingSegmentSpacing;
            GameObject limb = Instantiate(limbPrefab, pos, transform.rotation, transform);
            limb.name = "FlyingLimb_" + i;
            limb.transform.localScale = new Vector3(1f, 0.5f, 2f);

            Renderer rend = limb.GetComponent<Renderer>();
            if (rend != null)
                rend.material.color = flyingColor;

            Rigidbody rb = limb.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = limb.AddComponent<Rigidbody>();
                rb.mass = 0.1f;
                rb.drag = 2f;
            }
            if (limb.GetComponent<Collider>() == null)
                limb.AddComponent<BoxCollider>();

            if (i > 0)
            {

                HingeJoint joint = limb.AddComponent<HingeJoint>();
                Rigidbody prevRb = builtParts[i - 1].GetComponent<Rigidbody>();
                joint.connectedBody = prevRb;
                joint.anchor = new Vector3(-flyingSegmentSpacing / 2, 0, 0);
                joint.axis = Vector3.forward;
                joint.useMotor = true;
                JointMotor motor = joint.motor;
                motor.force = 100f;
                motor.targetVelocity = 0f;
                motor.freeSpin = false;
                joint.motor = motor;

                OscillatingJoint oscJoint = limb.AddComponent<OscillatingJoint>();
                BodyPartGene gene = new BodyPartGene
                {
                    frequency = 2f,
                    amplitude = 15f,
                    jointStrength = 10f,
                    phase = i * 0.3f
                };
                oscJoint.Init(gene);
            }
            else
            {
                limb.tag = "Creature";
                FlightController flightController = limb.AddComponent<FlightController>();
                flightController.liftForce = 10f;
            }
            builtParts.Add(limb);
        }
    }

    void BuildCreature(BodyPartGene gene, Transform parent, bool isRoot)
    {

        Vector3 position = parent.position + parent.rotation * gene.offset;
        Quaternion rotation = parent.rotation * Quaternion.Euler(gene.rotation - new Vector3(90f, 0f, 0f));

        GameObject limb = Instantiate(limbPrefab, position, rotation, parent);
        limb.transform.localScale = gene.scale;
        limb.name = isRoot ? "RootLimb" : "Limb";


        builtParts.Add(limb);

        Renderer rend = limb.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = dnaColor;
        }

        Rigidbody rb = limb.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = limb.AddComponent<Rigidbody>();
        }
        rb.mass = 50f;


        if (limb.GetComponent<Collider>() == null)
        {
            limb.AddComponent<BoxCollider>();
        }

        if (!isRoot)
        {
            HingeJoint joint = limb.AddComponent<HingeJoint>();

            Rigidbody parentRb = parent.GetComponent<Rigidbody>();
            if (parentRb == null)
            {
                Debug.LogWarning("Parent Rigidbody is missing on " + parent.gameObject.name + ". Adding one automatically.");
                parentRb = parent.gameObject.AddComponent<Rigidbody>();
                parentRb.mass = 2f;
            }
            joint.connectedBody = parentRb;


            joint.anchor = Vector3.zero;
            joint.axis = Vector3.up;
            joint.useMotor = true;
            JointMotor motor = joint.motor;
            motor.force = gene.jointStrength;
            motor.targetVelocity = 20f;
            motor.freeSpin = false;
            joint.motor = motor;
        }
        else
        {
            // For the root limb, tag it and attach the FitnessTracker.
            limb.tag = "Creature";
            if (limb.GetComponent<FitnessTracker>() == null)
            {
                limb.AddComponent<FitnessTracker>();
            }
        }

        // Recursively build any child limbs defined in this gene.
        if (gene.children != null)
        {
            foreach (BodyPartGene childGene in gene.children)
            {
                BuildCreature(childGene, limb.transform, false);
            }
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