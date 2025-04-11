using TMPro;
using UnityEngine;

public class IntroDialogue : MonoBehaviour
{
    public string[] dialogueLines;
    public TextMeshProUGUI dialogueText;
    public GameObject dialogueBox;
    private int dialogueIndex = 0;

    void Start()
    {
        // Set the dialogue flag so that other interactions are disabled.
        GlobalDialogueManager.IsDialogueActive = true;

        // Activate the dialogue box and show the first line.
        if (dialogueBox != null)
            dialogueBox.SetActive(true);
        if (dialogueText != null && dialogueLines.Length > 0)
            dialogueText.text = dialogueLines[dialogueIndex];
    }

    void Update()
    {
        // Use the E key to advance the intro dialogue.
        if (Input.GetKeyDown(KeyCode.E))
        {
            DisplayNextLine();
        }
    }

    void DisplayNextLine()
    {
        dialogueIndex++;

        if (dialogueIndex < dialogueLines.Length)
        {
            if (dialogueText != null)
                dialogueText.text = dialogueLines[dialogueIndex];
        }
        else
        {
            EndDialogue();
        }
    }

    void EndDialogue()
    {
        dialogueIndex = 0;
        if (dialogueText != null)
            dialogueText.text = "";
        if (dialogueBox != null)
            dialogueBox.SetActive(false);

        // Clear the global dialogue flag.
        GlobalDialogueManager.IsDialogueActive = false;

        // Disable this script so it stops processing input.
        this.enabled = false;
    }

}
