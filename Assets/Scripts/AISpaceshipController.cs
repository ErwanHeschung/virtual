using UnityEngine;
using UnityEngine.AI;

public class AISpaceshipController : MonoBehaviour
{
    public Transform[] waypoints;  // List of waypoints to follow (define these points on your race track)
    public float waypointTolerance = 2f;  // How close the AI must get to a waypoint before moving to the next one
    private int currentWaypointIndex = 0;
    private Rigidbody rb;  // Rigidbody reference for physics-based movement
    private bool isNavigating = false;

    public float moveSpeed = 10f;  // Adjust movement speed if needed
    public float rotationSpeed = 5f;  // Speed at which the ship rotates towards the movement direction

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Lock the rotation of the Rigidbody to prevent unwanted rotation due to physics collisions
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        SetNextWaypoint();
    }

    void FixedUpdate()
    {
        // Only move the Rigidbody based on the NavMeshAgent's destination
        if (isNavigating)
        {
            // Calculate direction to the current waypoint
            Vector3 direction = (waypoints[currentWaypointIndex].position - transform.position).normalized;

            // Move the ship using Rigidbody in FixedUpdate for smooth physics-based movement
            rb.MovePosition(transform.position + direction * moveSpeed * Time.fixedDeltaTime);

            // Manually rotate the ship to face the movement direction
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime));

            // Check if the AI has reached the waypoint
            if (Vector3.Distance(transform.position, waypoints[currentWaypointIndex].position) < waypointTolerance)
            {
                // Move to the next waypoint
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
                SetNextWaypoint();
            }
        }
    }

    void SetNextWaypoint()
    {
        isNavigating = true;
    }
}
