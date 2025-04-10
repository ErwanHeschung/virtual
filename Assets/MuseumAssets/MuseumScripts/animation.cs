using UnityEngine;

public class animation : MonoBehaviour
{
    // Start is called before the first frame update
    Animator animator;

    public float playerHeight;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        float verticalInput = Input.GetAxisRaw("Vertical");

        // Check if the player is moving
        bool isMoving = horizontalInput != 0 || verticalInput != 0;
        BoxCollider collider = GetComponentInParent<BoxCollider>();
        PlayerMovement movement = GetComponentInParent<PlayerMovement>();
        if (movement.isGrounded)
        {
            animator.SetBool("isRunning", isMoving);
        }
        animator.SetBool("isGrounded", movement.isGrounded);

    }
}
