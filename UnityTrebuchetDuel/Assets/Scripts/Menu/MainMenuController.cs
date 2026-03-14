using UnityEngine;
using UnityEngine.SceneManagement;

namespace TrebuchetDuel.Menu
{
    public class MainMenuController : MonoBehaviour
    {
        public void StartGame() => SceneManager.LoadScene("Gameplay");
        public void QuitGame() => Application.Quit();
    }
}
