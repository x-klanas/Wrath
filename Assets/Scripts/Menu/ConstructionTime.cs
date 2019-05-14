using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConstructionTime : MonoBehaviour
{
    // Start is called before the first frame update
    public float timer;
    public GameController.GameController gameControllerObj;
    Text text;

    private void Start()
    {
        gameControllerObj = GameObject.FindGameObjectWithTag("GameController")
            .GetComponent<GameController.GameController>();
        timer = gameControllerObj.constructionTimeSeconds;

    }

    void FixedUpdate()
    {
        if (gameControllerObj.constructionTimerTicking == false)
        {
            float x = timer - gameControllerObj.constructionTimeSeconds;
            string minutes = Mathf.Floor((x % 3600) / 60).ToString("00");
            string seconds = (x % 60).ToString("00");
            text.text = minutes + ':' + seconds;
            ;
        }
    }
}