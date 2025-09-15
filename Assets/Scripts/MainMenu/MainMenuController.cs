using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    public TextMeshProUGUI coinsText;

    void Start()
    {
        if (MainMenuLevelManager.shouldResetScoreOnLoad)
        {
            CoinManager.ResetCoins();
            MainMenuLevelManager.shouldResetScoreOnLoad = false;
        }
        UpdateCoinsDisplay();
    }

    void UpdateCoinsDisplay()
    {
        if (coinsText != null)
        {
            coinsText.text = "������: " + CoinManager.GetCoins();
        }
    }

    // ���� ����� ��� ������� ������ "������ ����"
    public void StartGame()
    {
        CoinManager.ResetCoins();
        UpdateCoinsDisplay();
        MainMenuLevelManager.StartFirstLevel();
    }

    // ����� �����: ��� ��������� ������ ������ ������
    public void StartLevel(int levelIndex)
    {
        // ���� ���������� ����
        CoinManager.ResetCoins();
        UpdateCoinsDisplay();
        // � ��������� ���� � ���������� ������
        MainMenuLevelManager.StartSpecificLevel(levelIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}