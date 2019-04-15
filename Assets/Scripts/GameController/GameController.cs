using System;
using UnityEngine;

namespace GameController
{
    public class GameController : MonoBehaviour
    {
        public long constructionTime = 60;
        private MyStopwatch timer = new MyStopwatch(DateTime.Now);

        // Start is called before the first frame update
        private void Start()
        {
            StartTimer();
        }

        // Update is called once per frame
        private void Update()
        {
            CheckTimer();
        }
    
        private void StartTimer()
        {
            timer.Reset();
            timer.Start();
        }

        private void PauseTimer()
        {
            timer.Stop();
        }

        private void ResumeTimer()
        {
            timer.Start();
        }

        private void CheckTimer()
        {
            if (timer.IsRunning && timer.ElapsedMilliseconds >= constructionTime * 1000)
            {
                timer.Stop();
                Debug.LogFormat("elapsed mm {0}", timer.ElapsedMilliseconds);
                Debug.Log("Time is up");
            }
        }
    
    
    }
}