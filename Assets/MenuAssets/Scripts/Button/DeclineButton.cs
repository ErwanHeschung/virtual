using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeclineButton : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnButtonClick()
    {
        Debug.Log("Decline clicked!");
        MissionPanelBehavior missionPanelBehavior = GetComponentInParent<MissionPanelBehavior>();
        missionPanelBehavior.HidePanel();
    }
}
