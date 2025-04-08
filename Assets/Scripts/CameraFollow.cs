using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform spaceship;
    public Vector3 offset = new Vector3(0f, 5f, 0f);

    void LateUpdate()
    {
        transform.position = spaceship.position + offset;

        transform.LookAt(spaceship.position);
    }
}