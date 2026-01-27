using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    public void LoadGameScene()
    {
        SceneManager.LoadScene(2);
    }

        public void LoadSetingsScene()
    {
        SceneManager.LoadScene(1);
    }
        public void LoadMenuScene()
    {
        SceneManager.LoadScene(0);
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
