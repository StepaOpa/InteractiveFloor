using UnityEngine;
using UnityEngine.SceneManagement;

public static class MainMenuLevelManager
{
    // ���� ��� ������ �����, ������� �� ������� �����
    public static bool shouldResetScoreOnLoad = false;

    private static string[] levelScenes =
    {
        "Seids",             // ������ 0
        "Labyrinth",          // ������ 1
        "Petroglyphs_new",    // ������ 2
        "Mogilnik_new",       // ������ 3
        "Game4_SceneName",    // ������ 4
        "Icebreaker_new",     // ������ 5
        "Game7_SceneName"     // ������ 6
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

    // ����� �����: ��� ������� ���� � ������������� ������
    public static void StartSpecificLevel(int levelIndex)
    {
        // ���������, ��� ��� �� �������� �������� ����� ������
        if (levelIndex >= 0 && levelIndex < levelScenes.Length)
        {
            // ������������� ������� ������, ����� ���� �����, ������ ����������
            currentLevelIndex = levelIndex;
            // ��������� ����� �� ����� �������
            SceneManager.LoadScene(levelScenes[currentLevelIndex]);
        }
        else
        {
            Debug.LogError("������� ��������� �������������� ������� � ��������: " + levelIndex);
        }
    }
}