using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndGameArea_Menu : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform canvas;  // end game area canvas
    public string mainMenuSceneName;// main menu scene name
    
    void Start()
    {
        
    }

    public void ExitButton()
    {   
        canvas.gameObject.SetActive(false);
        SceneManager.LoadScene(mainMenuSceneName);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
