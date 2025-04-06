using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class CreatureAgent : Agent
{
    private Rigidbody rb;
    public float moveSpeed = 100f;
    private Vector3 lastPosition;
    public float stuckThreshold = 0.2f;
    private float stuckTimer = 0f;
    private GameObject nearestFood;
    private float previousFoodDistance;

    public override void Initialize()
    {
        rb = GetComponent<Rigidbody>();
        rb.drag = 1.5f;
        rb.mass = 0.1f;
        rb.angularDrag = 0.5f;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    public override void OnEpisodeBegin()
    {
        float spawnRange = 80f;
        transform.position = new Vector3(Random.Range(-spawnRange, spawnRange), 5, Random.Range(-spawnRange, spawnRange));
        rb.velocity = Vector3.zero;
        lastPosition = transform.position;
        stuckTimer = 0f;
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        Vector3 movementForce = new Vector3(moveX, 0, moveZ) * moveSpeed;

        rb.AddForce(movementForce, ForceMode.Force);

        // Exploration reward
        float moveReward = Vector3.Distance(transform.position, lastPosition) * 0.1f;
        AddReward(moveReward);

        if (nearestFood != null)
        {
            float currentDistance = Vector3.Distance(transform.position, nearestFood.transform.position);
            float distanceDelta = previousFoodDistance - currentDistance;
            AddReward(distanceDelta * 0.1f);
            previousFoodDistance = currentDistance;
        }


        CheckIfStuck();
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        actionsOut.ContinuousActions.Array[0] = Input.GetAxis("Horizontal");
        actionsOut.ContinuousActions.Array[1] = Input.GetAxis("Vertical");
    }

    private void CheckIfStuck()
    {
        float dist = Vector3.Distance(transform.position, lastPosition);

        if (dist < stuckThreshold)
            stuckTimer += Time.deltaTime;
        else
            stuckTimer = 0f;

        if (stuckTimer > 5f)
        {
            AddReward(-1f);  // Moderate penalty if stuck
            EndEpisode();
        }

        lastPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Food"))
        {
            AddReward(10f);  // Higher reward for food
            Destroy(other.gameObject);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Obstacle"))
        {
            AddReward(-0.5f);
            EndEpisode();
        }
    }
}
