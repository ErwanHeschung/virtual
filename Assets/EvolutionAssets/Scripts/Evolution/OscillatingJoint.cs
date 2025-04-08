using UnityEngine;

public class OscillatingJoint : MonoBehaviour
{
    private ConfigurableJoint joint;
    private LimbGene gene;

    public void Init(LimbGene geneData)
    {
        gene = geneData;
        joint = GetComponent<ConfigurableJoint>();
    }

    void FixedUpdate()
    {
        float torque = Mathf.Sin(Time.time * gene.frequency + gene.phase) * gene.amplitude;
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.AddTorque(transform.right * torque);
    }
}
