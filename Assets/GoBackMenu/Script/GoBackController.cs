using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GoBackController : MonoBehaviour
{
    [SerializeField] private string menuSceneName = "Menu";
    [SerializeField] private GameObject confirmationPanel;
    [SerializeField] private Button QuitButton;
    [SerializeField] private Button ResumeButton;

    private bool isMenuOpen = false;

    void Start()
    {
        confirmationPanel.SetActive(false);
        SetCursorState(false);

        QuitButton.onClick.AddListener(ReturnToMenu);
        ResumeButton.onClick.AddListener(CloseMenu);

        QuitButton.interactable = true;
        ResumeButton.interactable = true;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    void ToggleMenu()
    {
        isMenuOpen = !isMenuOpen;
        confirmationPanel.SetActive(isMenuOpen);

        SetCursorState(isMenuOpen);
        Time.timeScale = isMenuOpen ? 0f : 1f;
    }

    void SetCursorState(bool menuActive)
    {
        Cursor.visible = menuActive;
        Cursor.lockState = menuActive ? CursorLockMode.None : CursorLockMode.Locked;
    }

    void CloseMenu()
    {
        isMenuOpen = false;
        confirmationPanel.SetActive(false);
        SetCursorState(false);
        Time.timeScale = 1f;
    }

    void ReturnToMenu()
    {
        Time.timeScale = 1f;
        SetCursorState(true);
        SceneManager.LoadScene(menuSceneName);
    }
}