using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameRule : MonoBehaviour
{
    public TMP_Text evolutionStageText;
    public TMP_Text targetStageText;
    public TMP_Text countdownText;
    public TMP_Text congratsText;

    private float timeRemaining = 300f;
    private int evolutionStage = 1;
    private int targetStage = 5;
    private bool reachedTarget = false;

    void Start()
    {
        targetStageText.text = "Target Stage: " + targetStage;
        UpdateEvolutionStage();
    }

    void Update()
    {
        if (reachedTarget) return;

        UpdateTimer();

        /*
        if (Input.GetKeyDown(KeyCode.Space) && evolutionStage < targetStage)
        {
            evolutionStage++;
            UpdateEvolutionStage();
        }*/

        if (evolutionStage >= targetStage)
        {
            reachedTarget = true;
            StartCoroutine(HandleSuccess());
        }
    }

    void UpdateTimer()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
            int minutes = Mathf.FloorToInt(timeRemaining / 60f);
            int seconds = Mathf.FloorToInt(timeRemaining % 60f);
            countdownText.text = $"{minutes:00}:{seconds:00}";
        }
        else
        {
            congratsText.text = "Time's Up!";
        }
    }

    void UpdateEvolutionStage()
    {
        evolutionStageText.text = "Evolution Stage: " + evolutionStage;
    }

    IEnumerator HandleSuccess()
    {
        congratsText.text = "Congrats! You reached the glorious evolution !";
        yield return new WaitForSeconds(3f);
        SceneManager.UnloadSceneAsync("Evolution");
        SceneManager.LoadScene("Menu");
    }
}
