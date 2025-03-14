using UnityEngine;

public class MenuPlanetController : MonoBehaviour
{

    public string sceneToLoad;
    public GameObject rocket;
    public Vector3 targetCoordinates;
    public float moveSpeed = 2f;
    public float arcHeight = 5f;

    private Vector3 startPosition;
    private float journeyLength;
    private float startTime;


    void Start()
    {
        startPosition = rocket.transform.position;
    }

    void Update()
    {
        if (rocket != null && Vector3.Distance(rocket.transform.position, targetCoordinates) > 0.1f)
        {
            MoveRocketToPlanet();
        }
    }

    private void OnMouseDown()
    {
        Debug.Log("Planet clicked: " + gameObject.name);
        // Reset and start the parabolic movement
        startPosition = rocket.transform.position;
        startTime = Time.time;
        journeyLength = Vector3.Distance(startPosition, targetCoordinates);
    }

    private void MoveRocketToPlanet()
    {

    }
}
