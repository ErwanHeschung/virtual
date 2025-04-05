using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CylinderController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveForce = 10f;      // Forward/backward force
    public float turnTorque = 5f;     // Left/right rotation torque
    public float jumpForce = 5f;      // Jump force (optional)

    private Rigidbody rb;
    private float verticalInput;
    private float horizontalInput;
    private bool isJumping;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // Freeze X/Z rotation to prevent tipping
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        // Get input axes
        verticalInput = Input.GetAxis("Vertical");
        horizontalInput = Input.GetAxis("Horizontal");

        // Jump input (optional)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            isJumping = true;
        }
    }

    void FixedUpdate()
    {
        // Apply forward/backward force
        rb.AddForce(transform.forward * verticalInput * moveForce, ForceMode.Force);

        // Apply left/right rotation torque
        rb.AddTorque(transform.up * horizontalInput * turnTorque, ForceMode.Force);

        // Jump (optional)
        if (isJumping)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isJumping = false;
        }
    }
}