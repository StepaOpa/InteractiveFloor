using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;


public class MainMenuController : MonoBehaviour
{
    public TextMeshProUGUI coinsText;

    // Метод Start() теперь главный по сбросу счета
    void Start()
    {
        // ПРОВЕРЯЕМ ФЛАГ: Если нам дали сигнал сбросить счет...
        if (MainMenuLevelManager.shouldResetScoreOnLoad)
        {
            CoinManager.ResetCoins(); // ...сбрасываем монеты...
            MainMenuLevelManager.shouldResetScoreOnLoad = false; // ...и опускаем флаг, чтобы не сбросить счет еще раз.
        }

        // А уже после всех проверок обновляем текст на экране
        UpdateCoinsDisplay();
    }

    void UpdateCoinsDisplay()
    {
        if (coinsText != null)
        {
            coinsText.text = "Монеты: " + CoinManager.GetCoins();
        }
    }

    // Кнопка "Начать игру" теперь НЕ сбрасывает счет. Она просто запускает игру.
    public void StartGame()
    {
        MainMenuLevelManager.StartFirstLevel();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}