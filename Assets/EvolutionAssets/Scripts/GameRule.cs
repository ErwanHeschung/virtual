using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameRule : MonoBehaviour
{
    public TMP_Text currentFitnessText;
    public TMP_Text congratsText;

    private float fitness = 0f;

    void Start()
    {
        currentFitnessText.text = "Current Fitness: " + fitness;
    }

    void Update()
    {
        UpdateFitness();
    }

    public void UpdateFitness()
    {
        fitness = EvolutionManager.Instance.bestFitness;
        currentFitnessText.text = "Current Fitness: " + fitness;
    }



}
