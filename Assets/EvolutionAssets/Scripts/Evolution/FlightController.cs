using UnityEngine;

public class FlightController : MonoBehaviour
{

    public float liftForce = 0f;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.drag = 100f;
        rb.angularDrag = 3f;
    }

    void FixedUpdate()
    {
    }
}