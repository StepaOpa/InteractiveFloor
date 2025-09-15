using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameEndPanelPetroglyphs : MonoBehaviour
{
    [Header("UI Компоненты")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI detailsText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;

    // --- НОВАЯ СТРОКА ---
    [Header("Система наград")]
    [SerializeField] private CoinRewardController coinRewardController; // Ссылка на контроллер монеток

    void Start()
    {
        restartButton.onClick.AddListener(OnRestartButtonClick);
        mainMenuButton.onClick.AddListener(OnMainMenuButtonClick);
    }

    // --- МЕТОД ИЗМЕНЕН ---
    // Добавили параметр coinsAwarded для передачи количества монет
    public void ShowPanel(bool isWin, int foundCount, int totalCount, int coinsAwarded)
    {
        gameObject.SetActive(true);

        if (isWin)
        {
            titleText.text = "Победа!";
            detailsText.text = $"Вы нашли все рисунки!\nЗаработано монеток: {coinsAwarded}";

            // --- НОВАЯ СТРОКА ---
            // Запускаем анимацию падения монеток
            if (coinRewardController != null)
            {
                coinRewardController.StartRewardSequence(coinsAwarded);
            }
        }
        else
        {
            titleText.text = "Поражение";
            detailsText.text = $"Время вышло.\nНайдено рисунков: {foundCount} из {totalCount}";
        }
    }

    private void OnRestartButtonClick()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnMainMenuButtonClick()
    {
        Debug.Log("Переход в главное меню (не реализовано)");
    }

    private void OnDestroy()
    {
        restartButton.onClick.RemoveListener(OnRestartButtonClick);
        mainMenuButton.onClick.RemoveListener(OnMainMenuButtonClick);
    }
}