using TMPro; // Import the TextMeshPro namespace
using UnityEngine;

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

    private GameObject currentIndicator;
    private Canvas canvas;
    public GameObject indicatorPrefab;

    void Start()
    {
        canvas = FindObjectOfType<Canvas>();
        if (indicatorPrefab)
        {
            currentIndicator = Instantiate(indicatorPrefab, canvas.transform);
            currentIndicator.SetActive(false);
            TextMeshProUGUI text = currentIndicator.GetComponentInChildren<TextMeshProUGUI>();
            text.text = "Press 'E' to Talk";
        }
        if (dialogueBox != null)
            dialogueBox.SetActive(false);
    }

    void Update()
    {
        if (!inConversation && Vector3.Distance(player.transform.position, guard.transform.position) <= interactionRange)
        {
            if (currentIndicator)
            {
                currentIndicator.SetActive(true);
                Vector3 screenPos = Camera.main.WorldToScreenPoint(guard.transform.position);
                currentIndicator.transform.position = screenPos;
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                Debug.Log("E key pressed. Starting conversation.");
                StartConversation();
            }
        }
        else if (inConversation && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Space)))
        {
            DisplayNextLine();
        }
        else if (currentIndicator)
        {
            currentIndicator.SetActive(false);
        }
    }

    void StartConversation()
    {
        Debug.Log("Starting conversation.");
        inConversation = true;
        dialogueIndex = 0;
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(true);
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
                dialogueText.text = dialogueLines[dialogueIndex];
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
            dialogueText.text = "";
        }
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }
    }
}
