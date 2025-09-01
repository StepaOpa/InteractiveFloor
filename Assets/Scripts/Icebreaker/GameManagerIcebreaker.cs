using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerIcebreaker : MonoBehaviour // <-- Имя класса должно совпадать с именем файла
{
    // Функция ДОЛЖНА БЫТЬ public, чтобы Unity ее увидел
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}