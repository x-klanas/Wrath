using System.Collections.Generic;
using UnityEngine;

namespace UtilityComponents {
    public class PathFollower : MonoBehaviour
    {
        public List<Transform> points = new List<Transform>();
        public float totalTime = 10f;

        public bool isMoving = true;
    
        private List<float> distances = new List<float>();
        private List<float> weights = new List<float>();
        private List<float> times = new List<float>();

        private int pointTracker = 0;
        private float currentTime = 0;

        private Transform startMarker;
        private Transform endMarker;

        void Start()
        {
            if (isMoving)
            {
                StartMoving();
            }
            else
            {
                CalculateValues();
                startMarker = points[0];
                endMarker = points[1];
            }
        }

        void Update()
        {
            if (isMoving)
            {
                currentTime += Time.deltaTime;

                if (CheckPosition())
                {
                    SetValues();
                }

                transform.position = Vector3.Lerp(startMarker.position, endMarker.position, CalculateInterpolant());

                if (!PointsExist())
                {
                    isMoving = false;
                }
            }
        }

        public void StartMoving()
        {
            CalculateValues();
            pointTracker = 0;
            currentTime = 0;
            startMarker = points[0];
            endMarker = points[1];
            isMoving = true;
        }

        public void StopMoving()
        {
            isMoving = false;
        }

        void CalculateValues()
        {
            float totalDistance = 0;
        
            for (int i = 0; i < points.Count - 1; i++)
            {
                distances.Add(CalculateDistance(points[i].position, points[i + 1].position));
                totalDistance += distances[i];
            }

            for (int i = 0; i < distances.Count; i++)
            {
                weights.Add(distances[i] / totalDistance);
            }

            for (int i = 0; i < weights.Count; i++)
            {
                times.Add(totalTime * weights[i]);
            }
        }

        float CalculateDistance(Vector3 a, Vector3 b)
        {
            return Vector3.Distance(a, b);
        }

        void SetValues()
        {
            if (pointTracker < points.Count - 2)
            {
                pointTracker++;
                startMarker = points[pointTracker];
                endMarker = points[pointTracker + 1];
            }
        }

        float CalculateInterpolant()
        {
            float result;
            result = currentTime / totalTime;
            for (int i = 0; i < pointTracker; i++)
            {
                result -= weights[i];
            }

            result /= weights[pointTracker];
            return result;
        }

        bool CheckPosition()
        {
            float result = 0;
            for (int i = 0; i < pointTracker + 1; i++)
            {
                result += times[i];
            }

            return currentTime >= result;
        }

        bool PointsExist()
        {
            return pointTracker < weights.Count;
        }
    }
}