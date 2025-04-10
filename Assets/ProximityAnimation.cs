using UnityEngine;

public class ProximityAnimation : MonoBehaviour
{
    public Transform player;        
    public float activationDistance = 5f;

    private Animator animator;
    private bool isNear = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.enabled = false;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        Debug.Log("Distance " + distance);
        if (distance <= activationDistance && !isNear)
        {
            animator.enabled = true;
        }
        else if (distance > activationDistance && isNear)
        {
            animator.enabled = false;
        }
    }
}
