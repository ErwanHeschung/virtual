using UnityEngine;

public class OscillatingJoint : MonoBehaviour
{
    private BodyPartGene gene;
    private HingeJoint hinge;
    private Rigidbody rb;
    public float maxTorque = 100f;


    public void Init(BodyPartGene g)
    {
        gene = g;
        hinge = GetComponent<HingeJoint>();
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
    }

    void FixedUpdate()
    {
        if (gene == null) return;


        float signal = Mathf.Sin(Time.time * gene.frequency + gene.phase) * gene.amplitude;
        signal = Mathf.Clamp(signal, -maxTorque, maxTorque);

        if (hinge)
        {
            JointMotor motor = hinge.motor;
            motor.force = gene.jointStrength * 2f;
            motor.targetVelocity = signal * 2f;
            hinge.motor = motor;
            hinge.useMotor = true;
        }
        else if (rb)
        {
            rb.AddTorque(transform.up * signal);
        }

        if (IsGrounded())
        {
            Vector3 rawDirection = transform.right;
            rawDirection.y = 0f; // Remove vertical component
            rawDirection.Normalize();

            float wave = Mathf.Sin(Time.time * gene.frequency + gene.phase) * gene.amplitude;
            Vector3 lateral = rawDirection * wave;

            rb.AddForce(lateral * 5f);
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 0.6f);
    }
}
