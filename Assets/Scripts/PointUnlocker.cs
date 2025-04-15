using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PointUnlocker : MonoBehaviour
{
    public GameObject[] nextPointObject;
    int currentPoint = 0;
    public int totalLaps = 1;
    int currentLap = 0;

    public GameObject[] objectsToDisable;
    public GameObject[] objectsToEnable;

    [Header("UI")]
    public TextMeshProUGUI currentLapText;
    private void Start()
    {
        currentLapText.text = "0/2";
    }
    public void UpdatePoint()
    {
        nextPointObject[currentPoint].SetActive(false);
        currentPoint++;
        if (currentPoint >= nextPointObject.Length)
        {
            currentPoint = 0;
            currentLap++;
            currentLapText.text = currentLap + "/2";
            if (currentLap == 2)
                endRace();
        }
        nextPointObject[currentPoint].SetActive(true);


        if (currentLap >= totalLaps)
        {
            foreach (GameObject obj in objectsToEnable)
                obj.SetActive(true);

            foreach (GameObject obj in objectsToDisable)
                obj.SetActive(false);
        }
    }

    private void endRace()
    {
        Invoke("LoadMenu", 2f);
    }

    private void LoadMenu()
    {
        AchievementTracker.Instance.CompleteAchievement("Race");
        SceneManager.LoadScene("Menu");

    }
}
