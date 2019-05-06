using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportingOnTrigger : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform theTarget;
    public GameObject thePlayer;
    public GameController.GameController gameControllerObj;

    private void Start()
    {
        gameControllerObj = GameObject.FindGameObjectWithTag("GameController")
            .GetComponent<GameController.GameController>();
    }
    public void OnCollisionEnter(Collision other)
    {

        if (other.gameObject.CompareTag("carObject") && gameControllerObj.constructionTimerTicking == false)
        {
            thePlayer.transform.position = theTarget.transform.position;
            gameControllerObj.testingTimerTicking = false;

        }
    }
}