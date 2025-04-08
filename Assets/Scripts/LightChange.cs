using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LightChange : MonoBehaviour
{
    public SpaceshipMovement playerController;
    public RCC_AICarController[] AIShips;
    public GameObject startPanel;
    public TextMeshProUGUI timerText;
    int timer = 3;

    private void Start()
    {
        timerText.text = timer.ToString();
    }
    public void StartGame()
    {
        StartCoroutine(TimerText());
    }

    IEnumerator TimerText()
    {
        while (timer > 0)
        {
            yield return new WaitForSeconds(1f);
            timer--;
            timerText.text = timer.ToString();
        }
        startPanel.SetActive(false);
        playerController.enabled = true;
        foreach (var ai in AIShips)
            ai.enabled = true;

        GetComponent<MeshRenderer>().material.color = Color.green;
    }

    public void Restart()
    {
        SceneManager.LoadScene(0);
    }
}
