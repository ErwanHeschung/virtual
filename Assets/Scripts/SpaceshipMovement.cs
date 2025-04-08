using UnityEngine;

public class SpaceshipMovement : MonoBehaviour
{
    public float speed = 10f;               // Movement speed of the spaceship
    public float maxSpeed = 10f;            // Max speed the spaceship can reach (for forward movement)
    public float reverseSpeed = 5f;         // Speed for reverse movement (slower than forward)
    public float acceleration = 5f;         // Acceleration speed for forward movement
    public float deceleration = 5f;         // Deceleration speed when stopping
    public float brakeForce = 10f;          // Braking force when moving forward and pressing reverse
    public float height = 5f;               // The fixed height at which the spaceship should stay
    public float tiltAmount = 30f;          // The maximum tilt angle when moving left or right
    public float tiltSpeed = 5f;            // The speed of the tilt animation
    public float rotationSpeed = 100f;      // Speed of rotation when adjusting to track curve
    public float liftForce = 10f;           // Force to maintain the height (Y-axis)
    public float collisionPushForce = 5f;   // Force to push the spaceship out of a collision

    private Rigidbody rb;
    private float currentTilt = 0f;         // Current tilt value to smoothly control tilt over time
    private bool isColliding = false;       // Flag to track if the spaceship is colliding

    // New variables to store the current speed and target speed for gradual changes
    public float currentSpeed = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
    }

    void FixedUpdate()
    {
        PlayerControl();
    }

    void PlayerControl()
    {
        // Get input for movement (W, S, A, D or arrow keys)
        float moveForward = Input.GetAxis("Vertical");  // Forward and backward (W/S or Up/Down arrow)
        float moveSide = Input.GetAxis("Horizontal");   // Left and right (A/D or Left/Right arrow)

        // Determine whether the ship should accelerate or decelerate
        if (moveForward > 0)  // Moving forward
        {
            // Accelerate the ship if moving forward
            currentSpeed = Mathf.MoveTowards(currentSpeed, maxSpeed, acceleration * Time.fixedDeltaTime);
        }
        else if (moveForward < 0)  // Moving backward
        {
            // If moving backward while moving forward, first slow down (brake) before reversing
            if (currentSpeed > 0)
            {
                // Braking behavior
                currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, brakeForce * Time.fixedDeltaTime);
            }
            else
            {
                // Accelerate in reverse when fully stopped
                currentSpeed = Mathf.MoveTowards(currentSpeed, -reverseSpeed, acceleration * Time.fixedDeltaTime);
            }
        }
        else  // No vertical input (stop moving)
        {
            // Decelerate the ship when not moving
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.fixedDeltaTime);
        }

        // Apply side movement scaling based on forward speed
        Vector3 sideMovement = transform.right * moveSide * Mathf.Abs(currentSpeed); // Side movement proportional to forward speed

        // Combine the forward and side movement (we only want side movement when moving forward)
        Vector3 velocity = transform.forward * currentSpeed;

        if (moveForward != 0 && Mathf.Abs(moveSide) > 0)
        {
            // If moving forward and pressing left or right, add side movement
            velocity += sideMovement;
        }

        // Apply the calculated velocity to the Rigidbody (only in X and Z directions)
        rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);

        // Keep the spaceship at the fixed height
        Vector3 position = transform.position;
        position.y = height;
        transform.position = position;

        // Tilt the spaceship based on horizontal input (sideways movement)
        if (moveSide != 0)
        {
            // Smoothly interpolate the tilt value (applies even when moving sideways only)
            currentTilt = Mathf.Lerp(currentTilt, -tiltAmount * moveSide, Time.fixedDeltaTime * tiltSpeed);

            // Apply the tilt to the spaceship, keeping Y-axis rotation intact
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, currentTilt);
        }
        else
        {
            // If no sideways input, smoothly return the tilt back to 0
            currentTilt = Mathf.Lerp(currentTilt, 0f, Time.fixedDeltaTime * tiltSpeed);
            transform.rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, currentTilt);
        }

        // Adjust the spaceship's rotation to match the track's curve when moving forward
        AdjustRotationToTrack(moveForward, moveSide);

        // Optionally, apply a force to keep the spaceship at a constant height (prevent drifting)
        Vector3 lift = new Vector3(0, liftForce, 0);
        rb.AddForce(lift, ForceMode.Acceleration);

        // Handle collision-related logic (prevent stuck situations)
        if (isColliding)
        {
            // Push the ship away from the wall slightly
            rb.AddForce(-transform.forward * collisionPushForce, ForceMode.VelocityChange);
        }
    }

    // Adjust the spaceship's rotation to match the track's curve when moving forward
    private void AdjustRotationToTrack(float moveForward, float moveSide)
    {
        if (moveForward != 0 && Mathf.Abs(moveSide) > 0)
        {
            // Rotate the spaceship around the Y-axis to follow the track's path
            float trackRotation = rotationSpeed * Time.fixedDeltaTime * moveSide;
            transform.Rotate(0, trackRotation, 0); // Adjust the spaceship rotation based on side movement
        }
        else if (Mathf.Abs(moveSide) > 0)
        {
            // Rotate in the direction of the side input (left or right) when no forward movement
            float trackRotation = rotationSpeed * Time.fixedDeltaTime * moveSide;
            transform.Rotate(0, trackRotation, 0);
        }
    }

    // Detect collisions and stop rotation if collision occurs
    private void OnCollisionStay(Collision collision)
    {
        // When a collision is detected, prevent further rotation
        isColliding = true;

        // Optionally, freeze angular velocity completely while colliding
        rb.angularVelocity = Vector3.zero;  // Ensure no further rotation
    }

    private void OnCollisionExit(Collision collision)
    {
        // Reset the collision status when no longer colliding
        isColliding = false;
    }
}
