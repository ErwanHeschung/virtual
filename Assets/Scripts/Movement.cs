using UnityEngine;

public class Movement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 5f;
    public Rigidbody rb;
    public bool isPlayerControlled = true; // Toggle for AI or player control

    public Transform[] waypoints; // Array of waypoints for AI to follow
    public float waypointTolerance = 1f; // How close AI needs to be to the waypoint before moving to the next one
    private int currentWaypointIndex = 0; // Index of the current waypoint AI is heading toward

    public int loopCount = 3; // Number of loops the AI should complete
    private int currentLoopCount = 0; // Current loop the AI is on

    private bool isAIActive = true; // Whether the AI should still be active

    float moveX;
    float moveZ;

    Quaternion targetRotation;

    Vector3 movement;

    private void Start()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        if (isAIActive)
        {
            if (isPlayerControlled)
            {
                GetPlayerInputs();
            }
            else
            {
                GetAIInputs();
            }
        }
    }

    private void FixedUpdate()
    {
        if (isAIActive)
        {
            CalculateMovement();
        }
    }

    void GetPlayerInputs()
    {
        moveX = Input.GetAxis("Horizontal");
        moveZ = Input.GetAxis("Vertical");
    }

    void GetAIInputs()
    {
        if (waypoints.Length == 0) return; // No waypoints to follow

        // Get the current target waypoint
        Transform targetWaypoint = waypoints[currentWaypointIndex];

        // Move towards the current waypoint
        Vector3 directionToTarget = (targetWaypoint.position - transform.position).normalized;
        moveX = directionToTarget.x;
        moveZ = directionToTarget.z;

        // Check if the AI is close enough to the current waypoint to switch to the next one
        if (Vector3.Distance(transform.position, targetWaypoint.position) < waypointTolerance)
        {
            // Move to the next waypoint, loop back to the first one if at the end
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;

            // If we've completed a loop through all waypoints, increment the loop count
            if (currentWaypointIndex == 0)
            {
                currentLoopCount++;

                // If the AI has completed the desired number of loops, stop the AI
                if (currentLoopCount >= loopCount)
                {
                    isAIActive = false;
                    StopAI();
                }
            }
        }
    }

    void CalculateMovement()
    {
        movement.Set(moveX, 0f, moveZ);

        if (movement.magnitude > 0)
        {
            Vector3 moveDirection = movement.normalized * moveSpeed * Time.deltaTime;
            rb.velocity = new Vector3(movement.x * moveSpeed, rb.velocity.y, movement.z * moveSpeed);

            targetRotation = Quaternion.LookRotation(movement.normalized);

            Quaternion currentRotation = transform.rotation;
            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
        else
        {
            rb.velocity = new Vector3(0, rb.velocity.y, 0);
            rb.angularVelocity = Vector3.zero;
        }
    }

    void StopAI()
    {
        // Stop the AI's movement by setting velocity to zero
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Optionally, disable other components or behaviors related to AI (if needed)
        // For example, you could disable AI pathfinding or AI scripts here as well
        Debug.Log("AI has completed the given number of loops and stopped.");
    }
}
