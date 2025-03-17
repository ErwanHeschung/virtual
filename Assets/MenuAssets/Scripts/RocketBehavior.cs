using UnityEngine;

public class RocketBehavior : MonoBehaviour
{
    private Vector3 targetCoordinates;
    private float moveSpeed = 4f;
    private float arcHeight = 10f;
    private float floatingHeight = 0.01f;
    private float floatingSpeed = 2f;

    private Vector3 startPosition;
    private bool isMoving = false;
    private float journeyProgress = 0f;
    private float floatingOffset = 0f;

    void Update()
    {
        if (isMoving)
        {
            MoveRocketToPlanet();
        }
        else
        {
            Floating();
        }
    }

    public void StartMoving(Vector3 startPosition, Vector3 targetCoordinates)
    {
        this.startPosition = startPosition;
        this.targetCoordinates = targetCoordinates;
        journeyProgress = 0f;
        isMoving = true;
    }

    public void StopMoving()
    {
        isMoving = false;
    }

    private void MoveRocketToPlanet()
    {
        journeyProgress += moveSpeed * Time.deltaTime;

        Vector3 straightPath = Vector3.Lerp(startPosition, targetCoordinates, journeyProgress);

        float arcOffset = Mathf.Sin(Mathf.Clamp01(journeyProgress) * Mathf.PI) * arcHeight;
        straightPath.y += arcOffset;

        transform.position = straightPath;

        if (journeyProgress >= 1f)
        {
            StopMoving();
        }
    }

    private void Floating()
    {
        floatingOffset = Mathf.Sin(Time.time * floatingSpeed) * floatingHeight;
        transform.position = new Vector3(transform.position.x, transform.position.y + floatingOffset, transform.position.z);
    }
}
