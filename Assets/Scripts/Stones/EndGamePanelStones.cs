using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // Необходимо для перезагрузки сцены

public class EndGamePanelStones : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI detailsText;

    // Этот метод будет вызываться из GameManagerStones
    public void ShowPanel(bool isVictory)
    {
        // Включаем саму панель
        gameObject.SetActive(true);

        if (isVictory)
        {
            titleText.text = "Победа!";
            detailsText.text = "Все ценные камни собраны";
        }
        else
        {
            titleText.text = "Поражение";
            detailsText.text = "Время вышло";
        }
    }

    // Этот метод мы привяжем к кнопке "Заново"
    public void OnRestartButtonClicked()
    {
        // Перезагружаем текущую активную сцену
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Этот метод мы привяжем к кнопке "Главное меню"
    public void OnMainMenuButtonClicked()
    {
        // Пока что просто выводим сообщение в консоль
        Debug.Log("Переход в главное меню...");
        // В будущем здесь будет SceneManager.LoadScene("MainMenuSceneName");
    }
}