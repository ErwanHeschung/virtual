using UnityEngine;
using TMPro; // Import the TextMeshPro namespace

public class DialogueSystem : MonoBehaviour
{
    public GameObject player;
    public GameObject guard;
    public float interactionRange = 3.0f;
    public string[] dialogueLines;
    public TextMeshProUGUI dialogueText; // Reference to the TextMeshPro UI Text element
    public GameObject dialogueBox; // Reference to the UI Panel
    private int dialogueIndex = 0;
    private bool inConversation = false;

    void Start()
    {
        // Ensure the dialogue box is inactive at the start
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }
        else
        {
            Debug.LogError("DialogueBox is not assigned.");
        }
    }

    void Update()
    {
        if (!inConversation && Vector3.Distance(player.transform.position, guard.transform.position) <= interactionRange)
        {
            Debug.Log("In range to interact with guard.");
            if (Input.GetKeyDown(KeyCode.E)) // Use 'E' key to interact
            {
                Debug.Log("E key pressed. Starting conversation.");
                StartConversation();
            }
        }

        if (inConversation && Input.GetKeyDown(KeyCode.E)) // Use 'E' key to advance dialogue
        {
            Debug.Log("E key pressed. Displaying next line.");
            DisplayNextLine();
        }
    }

    void StartConversation()
    {
        Debug.Log("Starting conversation.");
        inConversation = true;
        dialogueIndex = 0;
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(true); // Show the dialogue box
        }
        else
        {
            Debug.LogError("DialogueBox is not assigned.");
        }
        DisplayNextLine();
    }

    void DisplayNextLine()
    {
        if (dialogueIndex < dialogueLines.Length)
        {
            Debug.Log("Displaying dialogue line: " + dialogueLines[dialogueIndex]);
            if (dialogueText != null)
            {
                dialogueText.text = dialogueLines[dialogueIndex]; // Update the TextMeshPro UI Text element
            }
            else
            {
                Debug.LogError("DialogueText is not assigned.");
            }
            dialogueIndex++;
        }
        else
        {
            EndConversation();
        }
    }

    void EndConversation()
    {
        inConversation = false;
        if (dialogueText != null)
        {
            dialogueText.text = ""; // Clear the dialogue text
        }
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false); // Hide the dialogue box
        }
    }
}
