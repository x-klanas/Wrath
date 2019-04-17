using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Valve.VR;

public class InGame_Menu : MonoBehaviour
{
    public Transform canvas;

    public Vector3 old_position;//position before teleport
    public Vector3 center;
    public GameObject Player;//player
    public GameObject Target; //teleportation

    // Start is called before the first frame update
    public void RestartGame()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }

    public void Star()
    {
        old_position = new Vector3(Player.gameObject.transform.position.x, Player.gameObject.transform.position.y, Player.gameObject.transform.position.z);
    }


    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (canvas.gameObject.activeInHierarchy == false)
            {
                canvas.gameObject.SetActive(true);
                Time.timeScale = 0;
                Player.gameObject.transform.position = Target.gameObject.transform.position;
            }
            
            else
            {
                canvas.gameObject.SetActive(false);
                Player.gameObject.transform.position = center;
                //Player.gameObject.transform.position = old_position;
                Time.timeScale = 1;
            }
        }
    }
    
    
}
