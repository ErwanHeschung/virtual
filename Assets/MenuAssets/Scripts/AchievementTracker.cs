using UnityEngine;
using UnityEngine.UI;

public class AchievementTracker : MonoBehaviour
{
    public static AchievementTracker Instance;

    public bool Museum = false;
    public bool Race = false;
    public bool Evolution = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CompleteAchievement(string achievementName)
    {
        switch (achievementName)
        {
            case "Museum":
                Museum = true;
                break;
            case "Race":
                Race = true;
                break;
            case "Evolution":
                Evolution = true;
                Debug.Log("Evolution achievement completed!");
                break;
        }
    }

}