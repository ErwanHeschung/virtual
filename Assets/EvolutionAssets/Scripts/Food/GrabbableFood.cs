using UnityEngine;

public class GrabbableFood : MonoBehaviour
{
    public GameObject indicator;
    public float grabRange = 3f;

    private Transform player;

    void Start()
    {
        if (indicator != null)
        {
            indicator.SetActive(false);
        }

        player = GameObject.FindGameObjectWithTag("Player")?.transform;

    }

    void Update()
    {

        float distance = Vector3.Distance(player.position, transform.position);

        if (distance <= grabRange)
        {
            Debug.Log("Distance: " + distance);
            if (indicator != null)
            {
                indicator.SetActive(true);
                Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
                indicator.transform.position = screenPos;
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                GrabFood();
            }
        }
        else
        {
            if (indicator != null)
            {
                indicator.SetActive(false);
            }
        }
    }

    void GrabFood()
    {
        PlayerInventory.Instance?.AddFood();
        Destroy(gameObject);
        if (indicator != null)
        {
            Destroy(indicator);
        }
    }
}