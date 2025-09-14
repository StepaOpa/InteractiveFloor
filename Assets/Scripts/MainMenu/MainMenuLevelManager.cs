using UnityEngine;
using UnityEngine.SceneManagement;

public static class MainMenuLevelManager
{
    // Флаг для сброса счета, который мы сделали ранее
    public static bool shouldResetScoreOnLoad = false;

    private static string[] levelScenes =
    {
        "Stones",             // Индекс 0
        "Labyrinth",          // Индекс 1
        "Petroglyphs_new",    // Индекс 2
        "Mogilnik_new",       // Индекс 3
        "Game4_SceneName",    // Индекс 4
        "Icebreaker_new",     // Индекс 5
        "Game7_SceneName"     // Индекс 6
    };

    public static int currentLevelIndex = 0;

    public static void LoadNextLevel()
    {
        currentLevelIndex++;
        if (currentLevelIndex < levelScenes.Length)
        { SceneManager.LoadScene(levelScenes[currentLevelIndex]); }
        else
        { SceneManager.LoadScene("TotalScoreScene"); }
    }

    public static void RestartCurrentLevel()
    {
        SceneManager.LoadScene(levelScenes[currentLevelIndex]);
    }

    public static void StartFirstLevel()
    {
        currentLevelIndex = 0;
        SceneManager.LoadScene(levelScenes[currentLevelIndex]);
    }

    // НОВЫЙ МЕТОД: для запуска игры с определенного уровня
    public static void StartSpecificLevel(int levelIndex)
    {
        // Проверяем, что нам не передали неверный номер уровня
        if (levelIndex >= 0 && levelIndex < levelScenes.Length)
        {
            // Устанавливаем текущий индекс, чтобы игра знала, откуда продолжать
            currentLevelIndex = levelIndex;
            // Загружаем сцену по этому индексу
            SceneManager.LoadScene(levelScenes[currentLevelIndex]);
        }
        else
        {
            Debug.LogError("Попытка запустить несуществующий уровень с индексом: " + levelIndex);
        }
    }
}