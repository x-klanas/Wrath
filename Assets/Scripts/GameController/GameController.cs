using System;
using UnityEngine;

namespace GameController
{
    public class GameController : MonoBehaviour
    {
        public long constructionTime = 60;
        public long testingTime = 60;
        private MyStopwatch constructionTimer = new MyStopwatch(DateTime.Now);
        private MyStopwatch testingTimer = new MyStopwatch(DateTime.Now);
        private bool constructionTimerTicking = true;

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
                CheckConstructionTimer();
            }
            else
            {
                CheckTestingTimer();
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
            if (constructionTimer.IsRunning && constructionTimer.ElapsedMilliseconds >= constructionTime * 1000)
            {
                constructionTimer.Stop();
                Debug.LogFormat("Constr elapsed mm {0}", constructionTimer.ElapsedMilliseconds);
                Debug.Log("Time is up");
                StartTestingTimer();
                constructionTimerTicking = false;
              
            }
        }

        private void StartTestingTimer()
        {
            testingTimer.Reset();
            testingTimer.Start();
        }

        private void CheckTestingTimer()
        {
            if (testingTimer.IsRunning && testingTimer.ElapsedMilliseconds >= testingTime * 1000)
            {
                testingTimer.Stop();
                Debug.LogFormat("Testing elapsed mm {0}", testingTimer.ElapsedMilliseconds);
                Debug.Log("Time is up");
                //TeleportToEndGameArea();
            }
        }

    }
}