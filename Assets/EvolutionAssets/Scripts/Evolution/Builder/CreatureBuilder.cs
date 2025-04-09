using System.Collections.Generic;
using UnityEngine;

public class CreatureBuilder : MonoBehaviour
{
    public GameObject limbPrefab;
    public CreatureDNA dna;
    public List<GameObject> builtParts = new();
    public int segmentCount = 10;
    public float segmentSpacing = 1f;
    public float bestFitness = 0f;

    public Color snakeColor = new Color(0f, 1f, 0f);

    public void Build()
    {
        BuildSnake();
    }

    public void BuildSnake()
    {
        builtParts.Clear();

        GameObject prev = null;
        Rigidbody prevRb = null;
        Vector3 centerPos = transform.position;

        float adjustedSpacing = 0.5f;

        for (int i = 0; i < segmentCount; i++)
        {

            Vector3 pos = transform.position + transform.forward * i * adjustedSpacing;
            GameObject segment = Instantiate(limbPrefab, pos, Quaternion.identity, transform);
            segment.name = $"Segment_{i}";
            segment.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            Renderer segmentRenderer = segment.GetComponent<Renderer>();
            if (segmentRenderer != null)
            {
                segmentRenderer.material.color = snakeColor;
            }

            var rb = segment.GetComponent<Rigidbody>();
            builtParts.Add(segment);

            BodyPartGene gene = new BodyPartGene
            {
                frequency = Random.Range(1f, 3f),
                amplitude = Random.Range(10f, 30f),
                phase = i * 0.3f,
                jointStrength = 100f
            };

            if (prevRb != null)
            {
                var hinge = segment.AddComponent<HingeJoint>();
                hinge.connectedBody = prevRb;
                hinge.axis = Vector3.right;
                hinge.anchor = Vector3.zero;
                hinge.useMotor = true;

                JointMotor motor = new JointMotor
                {
                    force = 1000f,
                    targetVelocity = 80f,
                    freeSpin = false
                };
                hinge.motor = motor;
            }

            OscillatingJoint osc = segment.AddComponent<OscillatingJoint>();
            osc.Init(gene);

            prev = segment;
            prevRb = rb;

            if (i == 0)
            {
                rb.mass = 2f;
                segment.AddComponent<FitnessTracker>();

                segment.tag = "Creature";
            }
        }
    }

    public void UpdateBestFitness()
    {
        GameObject creatureChild = null;
        foreach (var part in builtParts)
        {
            if (part.CompareTag("Creature") && part.GetComponent<FitnessTracker>() != null)
            {
                creatureChild = part;
                break;
            }
        }

        if (creatureChild != null)
        {
            FitnessTracker tracker = creatureChild.GetComponent<FitnessTracker>();
            bestFitness = bestFitness > tracker.GetFitness() ? bestFitness : tracker.GetFitness();
        }
    }

    private void FixedUpdate()
    {
        if (builtParts.Count > 0)
        {
            UpdateBestFitness();
        }
    }

    private void BuildRecursive(BodyPartGene gene, Transform parent, Rigidbody parentRb)
    {
        // Get world position from local offset
        Vector3 worldPos = parent.TransformPoint(gene.offset);

        GameObject limb = Instantiate(limbPrefab, worldPos, Quaternion.identity, parent);
        limb.transform.localEulerAngles = gene.rotation;
        limb.transform.localScale = gene.scale;

        Rigidbody rb = limb.GetComponent<Rigidbody>();
        builtParts.Add(limb);

        if (parentRb != null)
            AddJoint(limb, parentRb, gene, parent);

        OscillatingJoint osc = limb.AddComponent<OscillatingJoint>();
        osc.Init(gene);

        foreach (var child in gene.children)
            BuildRecursive(child, limb.transform, rb);
    }


    private void AddJoint(GameObject limb, Rigidbody parentRb, BodyPartGene gene, Transform parent)
    {
        switch (gene.jointType)
        {
            case JointType.Hinge:
                var hinge = limb.AddComponent<HingeJoint>();
                hinge.connectedBody = parentRb;
                hinge.anchor = Vector3.zero;
                hinge.axis = gene.rotation.normalized;
                hinge.useLimits = true;
                hinge.limits = new JointLimits
                {
                    min = -45,
                    max = 45,
                    bounciness = 0,
                    bounceMinVelocity = 0
                };
                hinge.useMotor = true;
                hinge.motor = new JointMotor
                {
                    force = gene.jointStrength,
                    targetVelocity = gene.amplitude,
                    freeSpin = false
                };
                break;

            case JointType.Spring:
                var spring = limb.AddComponent<ConfigurableJoint>();
                spring.connectedBody = parentRb;
                spring.anchor = Vector3.zero;

                spring.xMotion = ConfigurableJointMotion.Limited;
                spring.yMotion = ConfigurableJointMotion.Limited;
                spring.zMotion = ConfigurableJointMotion.Limited;

                var linearLimit = new SoftJointLimit();
                linearLimit.limit = 0.1f;
                spring.linearLimit = linearLimit;

                spring.angularYZDrive = new JointDrive
                {
                    positionSpring = gene.jointStrength,
                    positionDamper = 1f,
                    maximumForce = 100f
                };
                break;

            case JointType.Fixed:
                var fixedJoint = limb.AddComponent<FixedJoint>();
                fixedJoint.connectedBody = parentRb;
                break;
        }
    }
}