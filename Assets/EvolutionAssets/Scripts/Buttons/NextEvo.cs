using TMPro;
using UnityEngine;

public class NextEvo : MonoBehaviour
{
    private Animator animator;
    public Transform player;
    public GameObject indicatorPrefab;
    public float interactionDistance = 3f;
    public bool isSimulationRunning = true;

    private Canvas canvas;
    private GameObject currentIndicator;

    void Start()
    {
        canvas = FindObjectOfType<Canvas>();
        animator = GetComponent<Animator>();

        currentIndicator = Instantiate(indicatorPrefab, canvas.transform);
        currentIndicator.SetActive(false);

        TextMeshProUGUI text = currentIndicator.GetComponentInChildren<TextMeshProUGUI>();
        text.text = "Press 'E' to evolve to the next generation";

        if (player == null)
        {
            player = Camera.main.transform;
        }
    }

    void Update()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= interactionDistance)
        {
            currentIndicator.SetActive(true);
            Vector3 screenPos = Camera.main.WorldToScreenPoint(transform.position);
            currentIndicator.transform.position = screenPos;

            if (Input.GetKeyDown(KeyCode.E))
            {

                animator.SetTrigger("PressButton");
                isSimulationRunning = false;
                EvolutionManager.Instance.nextGen();
            }
        }
        else
        {
            currentIndicator.SetActive(false);
        }
    }
}
