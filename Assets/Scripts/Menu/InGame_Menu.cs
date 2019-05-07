using UnityEngine;
using UnityEngine.SceneManagement;

namespace Menu {
    public class InGame_Menu : MonoBehaviour
    {
        public Transform canvas;

        public Vector3 oldPosition; //position before teleport
        public GameObject thePlayer; //player
        public Transform theTarget; //teleportation target

        public string mainMenuSceneName;

        public void RestartButton()
        {
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }

        public void ResumeButton()
        {
            canvas.gameObject.SetActive(false);
            TeleportBackFromMenu();
            Time.timeScale = 1;
        }
    
        public void ExitButton()
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (canvas.gameObject.activeInHierarchy == false)
                {
                    canvas.gameObject.SetActive(true);
                    Time.timeScale = 0;
                    TeleportIntoMenu();
                }
                else
                {
                    canvas.gameObject.SetActive(false);
                    TeleportBackFromMenu();
                    Time.timeScale = 1;
                }
            }
        }

        private void TeleportIntoMenu()
        {
            oldPosition = thePlayer.gameObject.transform.position;
            thePlayer.gameObject.transform.position = theTarget.transform.position;
        }

        private void TeleportBackFromMenu()
        {
            thePlayer.gameObject.transform.position = oldPosition;
        }
    
    }
}
