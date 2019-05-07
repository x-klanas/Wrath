using UnityEngine;
using UnityEngine.SceneManagement;

namespace Menu {
    public class MainMenu : MonoBehaviour {
        public string playSceneName = "PlayScene";

        public void PlayGame() {
            SceneManager.LoadScene(playSceneName);
        }

        public void QuitGame() {
            Debug.Log("quit");
            Application.Quit();
        }
    }
}