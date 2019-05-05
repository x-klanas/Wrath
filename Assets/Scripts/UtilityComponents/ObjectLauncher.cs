using UnityEngine;
using Valve.VR.InteractionSystem;

namespace UtilityComponents {
    public class ObjectLauncher : MonoBehaviour {
        public GameObject[] objects;

        public Transform[] spawnPoints;

        public float minSpawnRate = 0.1f;
        public float maxSpawnRate = 1f;

        public float minSpeed = 10f;
        public float maxSpeed = 50f;

        public float minAngularSpeed = 0f;
        public float maxAngularSpeed = 10f;

        public bool useTarget;
        public Transform target;
        public float targetError = 10f;

        private float waitForSpawn = 0f;

        private readonly System.Random random = new System.Random();

        private void Update() {
            if (waitForSpawn <= 0) {
                GameObject obj = objects[random.Next(objects.Length)];

                GameObject newObj = Instantiate(
                    obj,
                    spawnPoints[random.Next(spawnPoints.Length)].position,
                    Random.rotation
                );

                Rigidbody rigid = newObj.GetComponent<Rigidbody>();

                if (rigid) {
                    float speed = Random.Range(minSpeed, maxSpeed);
                    Vector3 direction;

                    if (useTarget) {
                        Vector3 targetPosition = target.position + Random.insideUnitSphere * targetError;
                        Vector3 position = newObj.transform.position;

                        Vector3 displacement = targetPosition - position;
                        Vector3 horizontalDisplacement = new Vector3(
                            displacement.x, 0f, displacement.z
                        );

                        direction = horizontalDisplacement.normalized;
                        speed = Mathf.Clamp(
                            horizontalDisplacement.magnitude / Mathf.Sqrt(2 * Mathf.Abs(displacement.y) / Physics.gravity.magnitude),
                            minSpeed,
                            maxSpeed
                        );
                    } else {
                        Vector2 direction2D = Random.insideUnitCircle;
                        direction = new Vector3(direction2D.x, 0f, direction2D.y);
                    }

                    rigid.velocity = direction * speed;
                    rigid.angularVelocity = Random.insideUnitSphere * Random.Range(minAngularSpeed, maxAngularSpeed);
                }

                waitForSpawn = Random.Range(minSpawnRate, maxSpawnRate);
            }

            waitForSpawn -= Time.deltaTime;
        }
    }
}