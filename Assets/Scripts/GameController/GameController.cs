using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameController
{
    public class GameController : MonoBehaviour
    {
        public float constructionTimeSeconds;
        public float testingTimeSeconds;
        public MyStopwatch constructionTimer = new MyStopwatch(DateTime.Now);
        public MyStopwatch testingTimer = new MyStopwatch(DateTime.Now);
        public bool constructionTimerTicking = true;
        public bool testingTimerTicking = true;
        public GameObject thePlayer;
        public Transform theTarget;
        public Text timerText;

        // Start is called before the first frame update
        private void Start()
        {
            StartConstructionTimer();
        }
        // Update is called once per frame
        private void Update()
        {
            if (constructionTimerTicking)
            {
                constructionTimeSeconds -= Time.deltaTime;
                /*string minutes = Mathf.Floor((constructionTimeSeconds % 3600) / 60).ToString("00");
                string seconds = (constructionTimeSeconds % 60).ToString("00");
                timerText.text = minutes + ':' + seconds;*/
                if (constructionTimeSeconds <= 0)
                {
                    
                    CheckConstructionTimer();
                    
                }
            }
            else
            {
                if (testingTimerTicking)
                {
                    {
                        testingTimeSeconds -= Time.deltaTime;
                        /*string minutes = Mathf.Floor((testingTimeSeconds % 3600) / 60).ToString("00");
                        string seconds = (testingTimeSeconds % 60).ToString("00");
                        timerText.text = minutes + ':' + seconds;*/
                        if (testingTimeSeconds <= 0)
                        {
                            CheckTestingTimer();
                        }
                    }
                }
            }
        }



        private void StartConstructionTimer()
        {
            constructionTimer.Reset();
            constructionTimer.Start();
        }
        private void PauseTimer()
        {
            constructionTimer.Stop();
        }
        private void ResumeTimer()
        {
            constructionTimer.Start();
        }
        private void CheckConstructionTimer()
        {          
                constructionTimer.Stop();
                Debug.LogFormat("Constr elapsed mm {0}", constructionTimer.ElapsedMilliseconds);
                Debug.Log("Time is up");
                StartTestingTimer();
                constructionTimerTicking = false;
            
        }

        private void StartTestingTimer()
        {
            testingTimer.Reset();
            testingTimer.Start();
        }

        private void CheckTestingTimer()
        {
                testingTimer.Stop();
                
                Debug.LogFormat("Testing elapsed mm {0}", testingTimer.ElapsedMilliseconds);
                Debug.Log("Time is up");
                thePlayer.gameObject.transform.position = theTarget.transform.position; // teleportation  
                testingTimerTicking = false;
        }

        private void OnTriggerEnter(Collider other)
        {
                
            {
               Debug.Log("Entered"); 
                testingTimerTicking = false;
                constructionTimerTicking = false;
            }
        }
    }
}