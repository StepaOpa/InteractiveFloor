using UnityEngine;
using UnityEngine.SceneManagement; // Обязательно подключите это пространство имен для работы со сценами


public class MainMenuController : MonoBehaviour
{
    // Этот метод будет загружать сцену по её имени
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // Этот метод можно будет использовать для кнопки выхода из игры
    public void QuitGame()
    {
        Debug.Log("Quitting game..."); // Это сообщение появится в консоли Unity
        Application.Quit();
    }
}