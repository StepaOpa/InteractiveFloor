using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EndGamePanelUI : MonoBehaviour
{
    [Header("Компоненты панели")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI detailsText;
    public Button restartButton;
    public Button mainMenuButton;

    void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RegisterEndGamePanel(this);
        }
        else
        {
            Debug.LogError("[EndGamePanelUI] Не удалось найти GameManager для регистрации!");
        }
    }
    
    public void ShowWin(int finalScore)
    {
        gameObject.SetActive(true);
        titleText.text = "Победа!";
        detailsText.text = $"Все уровни пройдены!\nНабрано очков: {finalScore}";
    }
    
    public void ShowLose(int finalScore, int levelsCompleted)
    {
        gameObject.SetActive(true);
        titleText.text = "Упс, время вышло.";

        // <-- ИЗМЕНЕНИЕ 2: Форматируем текст по вашему желанию
        detailsText.text = $"Пройдено уровней: {levelsCompleted - 1}\nНабрано очков: {finalScore}";
    }
}