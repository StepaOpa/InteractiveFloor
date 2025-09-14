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
            coinsText.text = "Монеты: " + CoinManager.GetCoins();
        }
    }

    // Этот метод для большой кнопки "Начать игру"
    public void StartGame()
    {
        // Сначала сбрасываем счет
        CoinManager.ResetCoins();
        UpdateCoinsDisplay();
        // Потом запускаем самый первый уровень
        MainMenuLevelManager.StartFirstLevel();
    }

    // НОВЫЙ МЕТОД: для маленьких кнопок выбора уровня
    public void StartLevel(int levelIndex)
    {
        // Тоже сбрасываем счет
        CoinManager.ResetCoins();
        UpdateCoinsDisplay();
        // И запускаем игру с ВЫБРАННОГО уровня
        MainMenuLevelManager.StartSpecificLevel(levelIndex);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}