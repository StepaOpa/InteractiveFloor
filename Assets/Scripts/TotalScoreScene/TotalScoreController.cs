using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class TotalScoreController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI totalCoinsText;

    void Start()
    {
        // При запуске сцены получаем монеты из нашего хранилища и показываем их
        totalCoinsText.text = "Ваш общий счёт: " + CoinManager.GetCoins();
    }

    // Этот метод повесим на кнопку
    public void GoToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}