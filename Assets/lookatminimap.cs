using UnityEngine;

public class MinimapFollow : MonoBehaviour
{
    public Transform target; 
    public Vector3 offset = new Vector3(0, 20, 0); 
    public bool rotateWithPlayer = false;

    void LateUpdate()
    {
        if (target == null) return;

        // Set position
        transform.position = target.position + offset;

        // Set rotation
        if (rotateWithPlayer)
        {
            transform.rotation = Quaternion.Euler(90f, target.eulerAngles.y, 0f); 
        }
        else
        {
            transform.rotation = Quaternion.Euler(90f, 0f, 0f); 
        }
    }
}

