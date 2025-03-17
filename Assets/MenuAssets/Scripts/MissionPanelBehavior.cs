using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class MissionPanelBehavior : MonoBehaviour
{
    // Reference to the Panel GameObject
    private GameObject panel;
    private GameObject planet;
    public TMP_Text planetName;

    void Start()
    {
        panel = gameObject;

        HidePanel();
    }

    // Method to show the panel
    public void ShowPanel()
    {
        GameObject gameObject = MenuController.currentPlanet;
        planet = Instantiate(gameObject, new Vector3(0, 10, 40), Quaternion.identity);

        SortingGroup sortingGroup = planet.GetComponent<SortingGroup>();

        if (sortingGroup == null)
        {
            sortingGroup = planet.AddComponent<SortingGroup>();
        }
        sortingGroup.sortingLayerName = "modal";
        sortingGroup.sortingOrder = 2;

        if (panel != null)
        {
            panel.SetActive(true);  // Set the panel to active, making it visible
            planetName.text = MenuController.currentPlanetName;
        }
    }


    public void HidePanel()
    {
        if (panel != null)
        {
            panel.SetActive(false);  // Set the panel to inactive, making it invisible
        }
        if (planet != null)
        {
            Destroy(planet);
        }

    }
}
