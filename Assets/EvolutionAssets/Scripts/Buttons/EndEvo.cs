using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndEvo : MonoBehaviour
{
    private Animator animator;
    public Transform player;
    public GameObject indicatorPrefab;
    public float interactionDistance = 3f;
    public bool isSimulationRunning = true;
    public TMP_Text congratsText;
    //end text


    private string endText = "Simulation Ended";
    private Canvas canvas;
    private GameObject currentIndicator;

    void Start()
    {
        canvas = FindObjectOfType<Canvas>();
        animator = GetComponent<Animator>();

        currentIndicator = Instantiate(indicatorPrefab, canvas.transform);
        currentIndicator.SetActive(false);

        TextMeshProUGUI text = currentIndicator.GetComponentInChildren<TextMeshProUGUI>();
        text.text = "Press 'E' to stop the simulation";

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
                congratsText.text = endText;
                //wait for 3 seconds
                Invoke("EndSimulation", 3f);

            }
        }
        else
        {
            currentIndicator.SetActive(false);
        }
    }

    void EndSimulation()
    {
        try
        {
            AchievementTracker.Instance.CompleteAchievement("Evolution");
        }
        catch (System.Exception e)
        {
            Debug.LogError("AchievementTracker not found: " + e.Message);
        }

        SceneManager.LoadScene("Menu");
    }

}
