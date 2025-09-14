using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro; // ќЅя«ј“≈Ћ№Ќќ добавьте эту строку дл€ работы с TextMeshPro


public class MainMenuController : MonoBehaviour
{
    // —оздаем публичное поле, куда мы перетащим наш текстовый объект
    public TextMeshProUGUI coinsText;

    // Ётот метод вызываетс€ один раз при запуске сцены
    void Start()
    {
        UpdateCoinsDisplay();
    }

    // ћетод дл€ обновлени€ текста с монетами
    void UpdateCoinsDisplay()
    {
        if (coinsText != null)
        {
            // ќбращаемс€ к нашему CoinManager, получаем монеты и выводим на экран
            coinsText.text = "ћонеты: " + CoinManager.GetCoins();
        }
    }

    // ¬аши методы дл€ загрузки сцен и выхода из игры остаютс€ без изменений
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }
}