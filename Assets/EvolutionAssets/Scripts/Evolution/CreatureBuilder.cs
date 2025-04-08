using System.Collections.Generic;
using UnityEngine;

public class CreatureBuilder : MonoBehaviour
{
    public GameObject limbPrefab;
    public CreatureDNA dna;

    public List<GameObject> builtLimbs = new List<GameObject>();


    private void Start()
    {
        gameObject.tag = "Creature";
    }

    public void Build()
    {
        GameObject baseLimb = Instantiate(limbPrefab, transform.position, Quaternion.identity, transform);
        builtLimbs.Add(baseLimb);
        Rigidbody baseRb = baseLimb.GetComponent<Rigidbody>();

        for (int i = 0; i < dna.limbCount; i++)
        {
            var gene = dna.limbs[i];

            Vector3 pos = baseLimb.transform.position + gene.positionOffset;
            GameObject limb = Instantiate(limbPrefab, pos, Quaternion.identity, transform);
            limb.transform.localScale = gene.scale;

            Rigidbody rb = limb.GetComponent<Rigidbody>();

            switch (gene.jointType)
            {
                case JointType.Hinge:
                    HingeJoint hingeJoint = limb.AddComponent<HingeJoint>();
                    hingeJoint.connectedBody = baseRb;
                    hingeJoint.axis = Vector3.right;
                    hingeJoint.useMotor = true;

                    JointMotor motor = hingeJoint.motor;
                    motor.force = gene.jointStrength;
                    motor.targetVelocity = gene.amplitude;
                    motor.freeSpin = false;
                    hingeJoint.motor = motor;
                    break;

                case JointType.Spring:
                    SpringJoint springJoint = limb.AddComponent<SpringJoint>();
                    springJoint.connectedBody = baseRb;
                    springJoint.spring = gene.jointStrength;
                    springJoint.damper = 1f;
                    break;

                case JointType.Fixed:
                    FixedJoint fixedJoint = limb.AddComponent<FixedJoint>();
                    fixedJoint.connectedBody = baseRb;
                    break;

                default:
                    Debug.LogWarning("Unknown joint type. Limb will not have a joint.");
                    break;
            }

            limb.AddComponent<OscillatingJoint>().Init(gene);

            builtLimbs.Add(limb);
        }
    }

}
