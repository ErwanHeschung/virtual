using UnityEngine;

public class FallRespawn : MonoBehaviour
{
    [Header("Respawn Settings")]
    public Vector3 respawnPosition = new Vector3(0, 5, 0); // Set your custom respawn point here
    public float fallThreshold = -20f; // If player falls below this Y value, they will respawn

    void Update()
    {
        if (transform.position.y < fallThreshold)
        {
            Respawn();
        }
    }

    private void Respawn()
    {
        transform.position = respawnPosition;

        // Optional: reset velocity if you use Rigidbody
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
