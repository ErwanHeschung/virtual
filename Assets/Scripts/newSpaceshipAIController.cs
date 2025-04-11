using UnityEngine;
using System.Collections.Generic;

public class SpaceshipAIController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 10f;               // Forward movement speed.
    public float rotationSpeed = 5f;            // Smoothing factor for rotation.
    public float checkpointTolerance = 1f;      // Horizontal distance threshold to consider a waypoint reached.

    [Header("Flight Altitude")]
    public float targetAltitude = 50f;          // Fixed altitude the ship maintains.

    [Header("Waypoint Settings")]
    // Manually assign your RCC checkpoint Transforms here.
    public List<Transform> waypoints;
    private int currentWaypointIndex = 0;

    [Header("Obstacle Avoidance Settings")]
    public float detectionDistance = 50f;       // Maximum distance for obstacle detection.
    public float detectionAngleThreshold = 10f;   // Only obstacles nearly directly ahead trigger avoidance.
    public float avoidanceDuration = 2f;        // Duration (in seconds) for which the block-style lateral move is applied.
    public float avoidanceMoveSpeed = 10f;      // Lateral move speed during avoidance.
    public GameObject[] obstacles;              // Assign your obstacle GameObjects here.

    // The fixed dodge direction (block-style): always to the right in this version,
    // or you can set it to -transform.right to dodge left.
    [HideInInspector]
    public Vector3 dodgeDirection = Vector3.right;

    // The current state of our FSM.
    private SpaceshipState currentState;

    #region FSM Nested Classes

    // Base state class.
    public abstract class SpaceshipState
    {
        protected SpaceshipAIController controller;
        public SpaceshipState(SpaceshipAIController controller)
        {
            this.controller = controller;
        }
        public virtual void Enter() { }
        public virtual void Update() { }
        public virtual void Exit() { }
    }

    // NavigationState: ship navigates toward waypoints normally.
    public class NavigationState : SpaceshipState
    {
        public NavigationState(SpaceshipAIController controller) : base(controller) { }
        public override void Enter()
        {
            Debug.Log("Entering Navigation State");
        }
        public override void Update()
        {
            Transform wp = controller.GetCurrentWaypoint();
            if (wp != null)
            {
                // Build target position using waypoint's X and Z; Y is forced to targetAltitude.
                Vector3 targetPos = new Vector3(wp.position.x, controller.targetAltitude, wp.position.z);
                // Calculate the desired direction (for rotation only).
                Vector3 desiredDir = (targetPos - controller.transform.position).normalized;
                Quaternion targetRot = Quaternion.LookRotation(desiredDir, Vector3.up);
                controller.transform.rotation = Quaternion.Slerp(controller.transform.rotation, targetRot,
                                                                  controller.rotationSpeed * Time.deltaTime);

                // Move forward using current forward vector.
                Vector3 movement = controller.transform.forward * controller.moveSpeed * Time.deltaTime;
                movement.y = 0;
                controller.transform.position += movement;
                controller.LockAltitude();

                // Check horizontal ("flat") distance to determine if waypoint is reached.
                Vector3 flatPos = new Vector3(controller.transform.position.x, 0, controller.transform.position.z);
                Vector3 flatTarget = new Vector3(targetPos.x, 0, targetPos.z);
                if (Vector3.Distance(flatPos, flatTarget) < controller.checkpointTolerance)
                {
                    controller.AdvanceWaypoint();
                }
            }
            else
            {
                // If no waypoint is assigned, simply move forward.
                Vector3 movement = controller.transform.forward * controller.moveSpeed * Time.deltaTime;
                movement.y = 0;
                controller.transform.position += movement;
                controller.LockAltitude();
            }
        }
    }

    // AvoidanceState: ship moves sideways (block style) for a fixed duration.
    public class AvoidanceState : SpaceshipState
    {
        private float timer;
        public AvoidanceState(SpaceshipAIController controller, float duration) : base(controller)
        {
            timer = duration;
        }
        public override void Enter()
        {
            Debug.Log("Entering Avoidance State");
        }
        public override void Update()
        {
            timer -= Time.deltaTime;
            // Move laterally in the fixed dodge direction (like a block translation).
            Vector3 movement = controller.dodgeDirection.normalized * controller.avoidanceMoveSpeed * Time.deltaTime;
            movement.y = 0;
            controller.transform.position += movement;
            controller.LockAltitude();

            if (timer <= 0f)
            {
                controller.ChangeState(new NavigationState(controller));
            }
        }
        public override void Exit()
        {
            Debug.Log("Exiting Avoidance State");
        }
    }

    #endregion

    #region MonoBehaviour Methods and Helpers

    void Start()
    {
        // Initialize the FSM in NavigationState.
        ChangeState(new NavigationState(this));
    }

    void Update()
    {
        // If not already in avoidance mode, check for obstacles.
        if (!(currentState is AvoidanceState))
        {
            foreach (GameObject obs in obstacles)
            {
                if (obs == null)
                    continue;
                Vector3 directionToObs = obs.transform.position - transform.position;
                float distanceToObs = directionToObs.magnitude;
                if (distanceToObs <= detectionDistance)
                {
                    float angleToObs = Vector3.Angle(transform.forward, directionToObs);
                    if (angleToObs < detectionAngleThreshold)
                    {
                        RaycastHit hit;
                        if (Physics.Raycast(transform.position, directionToObs.normalized, out hit, detectionDistance))
                        {
                            if (hit.collider.gameObject == obs)
                            {
                                Debug.Log("Obstacle detected: " + obs.name);
                                // Set dodge direction; here we always dodge to the right.
                                SetDodgeDirection();
                                ChangeState(new AvoidanceState(this, avoidanceDuration));
                                return; // Give priority to avoidance.
                            }
                        }
                    }
                }
            }
        }

        // Update the current FSM state.
        currentState.Update();
    }

    // Changes the current FSM state.
    public void ChangeState(SpaceshipState newState)
    {
        if (currentState != null)
        {
            currentState.Exit();
        }
        currentState = newState;
        currentState.Enter();
    }

    // Returns the current waypoint.
    public Transform GetCurrentWaypoint()
    {
        if (waypoints != null && waypoints.Count > 0)
        {
            return waypoints[currentWaypointIndex];
        }
        return null;
    }

    // Advances to the next waypoint.
    public void AdvanceWaypoint()
    {
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Count;
        Debug.Log("Advanced to waypoint index: " + currentWaypointIndex);
    }

    // Locks the ship's altitude to targetAltitude.
    public void LockAltitude()
    {
        Vector3 pos = transform.position;
        pos.y = targetAltitude;
        transform.position = pos;
    }

    // Sets the fixed dodge direction (always to the right in this version).
    private void SetDodgeDirection()
    {
        dodgeDirection = transform.right;
    }

    #endregion
}
