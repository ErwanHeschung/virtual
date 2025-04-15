using UnityEngine;

public class ProximityAnimation : MonoBehaviour
{
    public Transform player;
    public float activationDistance = 5f;

    private Animator animator;
    private bool isNear = false;

    private bool isVisited = false;
    private static int visitedCount = 0;

    void Start()
    {
        animator = GetComponent<Animator>();
        animator.enabled = false;
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= activationDistance && !isNear)
        {
            animator.enabled = true;
            if (!isVisited)
            {
                visitedCount++;
            }
            isVisited = true;
            if (visitedCount == 3)
            {
                DoSpecialAction();
            }
        }
        else if (distance > activationDistance && isNear)
        {
            animator.enabled = false;
        }
    }

    private void DoSpecialAction()
    {
        AchievementTracker.Instance.CompleteAchievement("Museum");
    }
}
