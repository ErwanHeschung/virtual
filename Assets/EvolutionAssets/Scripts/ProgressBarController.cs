using UnityEngine;
using UnityEngine.UI;

public class ProgressBarController : MonoBehaviour
{
    public Slider progressBar;

    void Start()
    {
        progressBar.maxValue = 30;
    }

    public void Update()
    {
        progressBar.value = FeedCreature.Instance.colectedData;
        Debug.Log("Progress Bar Value: " + progressBar.value);
    }
}