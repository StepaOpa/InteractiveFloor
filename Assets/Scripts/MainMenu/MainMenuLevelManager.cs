using UnityEngine;
using UnityEngine.SceneManagement;

public static class MainMenuLevelManager
{
    // НОВАЯ СТРОКА: "Флаг" для сброса счета.
    public static bool shouldResetScoreOnLoad = false;

    private static string[] levelScenes =
    {
        "Stones", "Labyrinth", "Petroglyphs_new", "Mogilnik_new",
        "Game4_SceneName", "Icebreaker_new", "Game7_SceneName"
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
}