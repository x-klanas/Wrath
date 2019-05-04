using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Valve.VR;

public class InGame_Menu : MonoBehaviour
{
    public Transform canvas;

    public Vector3 old_position;//position before teleport
    public GameObject thePlayer;//player
    public Transform theTarget; //teleportation target

    // Start is called before the first frame update
    public void RestartButton()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    public void ResumeButton()
    {
        canvas.gameObject.SetActive(false);
        teleportBackFromMenu();
        Time.timeScale = 1;
        
    }

    
    public void ExitButton()
    {
        Scene mainMenuScene = SceneManager.GetSceneByBuildIndex(1);
        SceneManager.LoadScene(mainMenuScene.name);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (canvas.gameObject.activeInHierarchy == false)
            {
                canvas.gameObject.SetActive(true);
                Time.timeScale = 0;
                teleportIntoMenu();
            }
            
            else
            {
                canvas.gameObject.SetActive(false);
                teleportBackFromMenu();
                Time.timeScale = 1;
            }
        }
    }


    void teleportIntoMenu()
    {
        old_position = thePlayer.gameObject.transform.position;
        thePlayer.gameObject.transform.position = theTarget.transform.position;

    }
    
    void teleportBackFromMenu()
    {
        thePlayer.gameObject.transform.position = old_position;
    }
    
    
}
