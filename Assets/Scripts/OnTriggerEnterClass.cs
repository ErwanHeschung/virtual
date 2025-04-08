using UnityEngine;

public class OnTriggerEnterClass : MonoBehaviour
{
    public PointUnlocker unlocker;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            unlocker.UpdatePoint();
        }
    }
}
