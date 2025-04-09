using System.Collections;
using TMPro;
using UnityEngine;

public class RaceStartController : MonoBehaviour
{
    public SpaceshipMovement playerController;
    public RCC_AICarController[] AIShips;
    public TextMeshProUGUI timerText;
    public Camera mainCamera; // The main camera of the car
    public Camera initialCamera; // The initial camera
    public GameObject startObject; // The object player needs to approach
    public GameObject character1; // The first character to disappear
    public GameObject character2; // The second character to disappear
    public float interactionRange = 3.0f;

    private int timer = 2;
    private bool countdownStarted = false;

    private void Start()
    {
        // Ensure the player and AI controllers are disabled at the start
        playerController.enabled = false;
        foreach (var ai in AIShips)
        {
            ai.enabled = false;
            ai.maximumSpeed = 1000;
            // Set maximum speed to 0 to prevent movement
        }

        timerText.text = timer.ToString();

        // Log the start of the race controller
        Debug.Log("RaceStartController initialized.");
    }

    private void Update()
    {
        if (!countdownStarted && Vector3.Distance(playerController.transform.position, startObject.transform.position) <= interactionRange)
        {
            Debug.Log("Player is within interaction range of the start object.");
            if (Input.GetKeyDown(KeyCode.R)) // Use 'R' key to start the countdown and make characters disappear
            {
                Debug.Log("'R' key pressed. Starting the countdown and making characters disappear.");
                StartGame();
            }
        }
    }

    public void StartGame()
    {
        countdownStarted = true;
        Debug.Log("Countdown started.");
        StartCoroutine(TimerText());
    }

    IEnumerator TimerText()
    {
        while (timer > 0)
        {
            yield return new WaitForSeconds(1f);
            timer--;
            timerText.text = timer.ToString();
            Debug.Log("Countdown: " + timer);
        }

        // After countdown, enable player control and AI
        playerController.enabled = true;
        foreach (var ai in AIShips)
        {
            ai.enabled = true;
        }

        // Switch cameras
        initialCamera.enabled = false; // Disable the initial camera
        mainCamera.enabled = true; // Enable the main camera of the car

        // Make characters disappear after a 3-second delay
        StartCoroutine(MakeCharactersDisappear());

        Debug.Log("Countdown finished. Race started.");
    }

    private IEnumerator MakeCharactersDisappear()
    {
        // Wait for 3 seconds
        yield return new WaitForSeconds(0f);

        // Disable character 1
        if (character1 != null)
        {
            character1.SetActive(false);
            Debug.Log("Character 1 disappeared.");
        }
        else
        {
            Debug.LogError("Character 1 is not assigned.");
        }

        // Disable character 2
        if (character2 != null)
        {
            character2.SetActive(false);
            Debug.Log("Character 2 disappeared.");
        }
        else
        {
            Debug.LogError("Character 2 is not assigned.");
        }
    }
}
