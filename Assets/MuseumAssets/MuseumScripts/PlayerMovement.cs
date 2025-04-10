using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;

    public float groundDrag;
    public Transform orientation;
    public float jumpForce;
    public float airMultiplier;
    public float jumpCoolDown;
    [SerializeField] float playerGravityScale = 2f; // 2x normal gravity

    bool readyToJump;

    [Header("Keybinding")]
    public KeyCode jumpCode = KeyCode.Space;

    [Header("Ground Check")]
    public float playerHeight;
    LayerMask ground;

    public bool isGrounded;

    float horizontalInput;
    float verticalInput;

    Vector3 direction;

    Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        readyToJump = true;
    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        if (Input.GetKey(jumpCode) && readyToJump && isGrounded)
        {
            readyToJump = false;
            jump();
            Invoke(nameof(resetJump), jumpCoolDown);
        }

    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, 0.5f * playerHeight + 0.2f);
<<<<<<< Updated upstream:Assets/MuseumAssets/MuseumScripts/PlayerMovement.cs
        MyInput();
=======
        MyInput();   
>>>>>>> Stashed changes:Assets/MuseumAssets/MuseumScripts/Movement.cs
        if (isGrounded)
        {
            rb.drag = groundDrag;
        }
        else
        {
            rb.drag = 3;
        }
    }

    private void FixedUpdate()
    {
        rb.AddForce(playerGravityScale * Physics.gravity.y * Vector3.up, ForceMode.Acceleration);
        movePlayer();

    }

    private void movePlayer()
    {
        direction = orientation.forward * verticalInput + orientation.right * horizontalInput;
        if (isGrounded)
            rb.AddForce(direction * moveSpeed * 10f, ForceMode.Force);
        else if (!isGrounded)
            rb.AddForce(direction * moveSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    private void jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void resetJump()
    {
        readyToJump = true;
    }
}
