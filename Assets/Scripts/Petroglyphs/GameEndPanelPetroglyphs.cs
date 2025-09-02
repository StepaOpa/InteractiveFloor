using UnityEngine;
using UnityEngine.UI;
using TMPro; // Важно! Нужно для работы с TextMeshPro
using UnityEngine.SceneManagement; // Нужно для перезагрузки сцены


public class GameEndPanelPetroglyphs : MonoBehaviour
{
    [Header("UI Компоненты")]
    [SerializeField] private TextMeshProUGUI titleText; // Текст заголовка (Победа/Поражение)
    [SerializeField] private TextMeshProUGUI detailsText; // Детальный текст (статистика)
    [SerializeField] private Button restartButton; // Кнопка "Начать заново"
    [SerializeField] private Button mainMenuButton; // Кнопка "Главное меню"

    void Start()
    {
        // Добавляем слушателей для кнопок. Это программный способ назначить действия.
        restartButton.onClick.AddListener(OnRestartButtonClick);
        mainMenuButton.onClick.AddListener(OnMainMenuButtonClick);
    }

    // Этот публичный метод будет вызываться из GameManager'а
    public void ShowPanel(bool isWin, int foundCount, int totalCount)
    {
        // Включаем саму панель, чтобы она стала видимой
        gameObject.SetActive(true);

        if (isWin)
        {
            // Настраиваем тексты для победы
            titleText.text = "Победа!";
            // Здесь можно будет добавить переменную для монеток, пока что просто число
            detailsText.text = "Вы нашли все рисунки!\nЗаработано монеток: 10";

            // Сюда позже добавим логику падения монеток
        }
        else
        {
            // Настраиваем тексты для поражения
            titleText.text = "Поражение";
            detailsText.text = $"Время вышло.\nНайдено рисунков: {foundCount} из {totalCount}";
        }
    }

    private void OnRestartButtonClick()
    {
        // Перезагружает текущую активную сцену
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnMainMenuButtonClick()
    {
        // Пока что просто выводим сообщение в консоль
        Debug.Log("Переход в главное меню (не реализовано)");
    }

    // Этот метод вызывается автоматически, когда объект уничтожается.
    // Хорошая практика - отписываться от событий, на которые подписались.
    private void OnDestroy()
    {
        restartButton.onClick.RemoveListener(OnRestartButtonClick);
        mainMenuButton.onClick.RemoveListener(OnMainMenuButtonClick);
    }
}