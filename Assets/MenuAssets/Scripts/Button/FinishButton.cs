using UnityEngine;
using UnityEngine.UI;

public class FinishButton : MonoBehaviour
{
    public Button button;

    void Start()
    {
        if (button == null)
        {
            Debug.LogWarning("Button is not assigned!");
            return;
        }

        button.onClick.AddListener(OnFinishButtonClick);

        if (AchievementTracker.Instance != null
            && AchievementTracker.Instance.Museum
            && AchievementTracker.Instance.Race
            && AchievementTracker.Instance.Evolution)
        {
            button.gameObject.SetActive(true);
        }
        else
        {
            button.gameObject.SetActive(false);
        }
    }

    public void OnFinishButtonClick()
    {

    }
}
