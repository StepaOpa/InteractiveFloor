using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TotalScoreController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI totalCoinsText;

    void Start()
    {
        if (totalCoinsText != null)
        {
            totalCoinsText.text = "Ваш общий счёт: " + CoinManager.GetCoins();
        }
    }

    // Этот метод ТОЛЬКО загружает сцену. Он НЕ добавляет монеты.
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}