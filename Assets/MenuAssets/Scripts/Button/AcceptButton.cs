using UnityEngine;

public class AcceptButton : MonoBehaviour
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
        Debug.Log("Accepted clicked!");
        UnityEngine.SceneManagement.SceneManager.LoadScene(MenuController.currentScene);
    }
}
