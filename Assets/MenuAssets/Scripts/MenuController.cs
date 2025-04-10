using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class MenuController : MonoBehaviour
{
    public GameObject rocket;
    public GameObject UI;
    public string targetScene;
    public string planetName;
    public string previousAchievementName;
    public GameObject planet;

    public static string currentScene;
    public static string currentPlanetName;
    public static GameObject currentPlanet;


    private RocketBehavior rocketBehavior;
    private MissionPanelBehavior missionPanelBehavior;
    private float topOffset = 0.5f;

    void Start()
    {
        rocketBehavior = rocket.GetComponent<RocketBehavior>();
        missionPanelBehavior = UI.GetComponent<MissionPanelBehavior>();
    }

    private void Update()
    {

        if (!isValidAchievement())
        {
            return;
        }

        if (!AchievementTracker.Instance.GetType().GetField(previousAchievementName).GetValue(AchievementTracker.Instance).Equals(true))
        {
            foreach (Renderer r in planet.GetComponentsInChildren<Renderer>())
            {
                Material[] mats = r.materials;
                for (int i = 0; i < mats.Length; i++)
                {
                    mats[i].color = Color.black;
                }
                r.materials = mats;
            }
        }

    }

    private Boolean isValidAchievement()
    {
        return !(AchievementTracker.Instance == null || planet == null || string.IsNullOrEmpty(previousAchievementName));

    }

    private Vector3 GetTopOfPlanet()
    {
        Vector3 topOfPlanet = transform.position;
        topOfPlanet.y += topOffset;

        return topOfPlanet;
    }

    private void OnMouseDown()
    {
        if (!string.IsNullOrEmpty(previousAchievementName))
        {
            Debug.Log("Mouse Down on: " + AchievementTracker.Instance.GetType().GetField(previousAchievementName).GetValue(AchievementTracker.Instance).Equals(true));
            if (IsPointerOverUIElement() ||
            !AchievementTracker.Instance.GetType().GetField(previousAchievementName).GetValue(AchievementTracker.Instance).Equals(true))
                return;
        }


        currentPlanet = gameObject;
        currentScene = targetScene;
        currentPlanetName = planetName;

        Vector3 topOfPlanet = GetTopOfPlanet();

        if (rocketBehavior != null)
        {
            rocketBehavior.StartMoving(rocket.transform.position, topOfPlanet);
            StartCoroutine(ShowPanelWithDelay());
            Debug.Log("Planet clicked: " + gameObject.name);
        }
    }

    private bool IsPointerOverUIElement()
    {
        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, results);
        return results.Count > 0;
    }

    private IEnumerator ShowPanelWithDelay()
    {
        yield return new WaitForSeconds(0.35f);
        missionPanelBehavior.ShowPanel();
    }

}
