using TMPro;
using UnityEngine;

public class GameRule : MonoBehaviour
{
    public TMP_Text currentFitnessText;
    public TMP_Text congratsText;
    public TMP_Text evolutionStageText;

    private float fitness = 0f;

    void Start()
    {
        currentFitnessText.text = "Current Fitness: " + fitness;
        evolutionStageText.text = "Evolution Stage: " + EvolutionManager.Instance.evolutionStage;
    }

    void Update()
    {
        UpdateFitness();
    }

    public void UpdateFitness()
    {
        fitness = EvolutionManager.Instance.bestFitness;
        evolutionStageText.text = "Evolution Stage: " + EvolutionManager.Instance.evolutionStage;
        currentFitnessText.text = "Current Fitness: " + fitness;
    }



}
