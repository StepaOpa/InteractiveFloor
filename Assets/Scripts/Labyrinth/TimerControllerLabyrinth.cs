using UnityEngine;
using UnityEngine.UI;
using TMPro; // <<< НОВОЕ: Добавили эту строку для работы с TextMeshPro

public class TimerControllerLabyrinth : MonoBehaviour
{
    // --- Старые переменные ---
    public Image timerImage;
    public float timeLimit = 120f;
    private float timeRemaining;
    public bool timeIsUp = false;

    // --- НОВАЯ ПЕРЕМЕННАЯ ---
    // Сюда мы перетащим наш текстовый UI-элемент
    public TextMeshProUGUI timerText;

    void Start()
    {
        timeRemaining = timeLimit;
        timeIsUp = false;
    }

    void Update()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;

            // Обновляем и радиальный бар...
            timerImage.fillAmount = timeRemaining / timeLimit;
            // ...и текст
            DisplayTime(timeRemaining); // <<< НОВОЕ: Вызываем метод для обновления текста
        }
        else
        {
            // Время вышло
            timeIsUp = true;
            timeRemaining = 0;

            // Убедимся, что все обнулилось
            timerImage.fillAmount = 0;
            timerText.text = "00:00"; // <<< НОВОЕ: Показываем нули в тексте
        }
    }

    // --- НОВЫЙ МЕТОД ---
    // Этот метод красиво форматирует время в формат ММ:СС и выводит на экран
    void DisplayTime(float timeToDisplay)
    {
        // Добавляем 1 секунду, чтобы таймер не показывал 00:00, когда осталась еще почти секунда
        if (timeToDisplay < 0)
        {
            timeToDisplay = 0;
        }

        // Вычисляем минуты и секунды
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        // Обновляем текст, используя форматирование строки. "00" означает, что будет ведущий ноль (01, 02 и т.д.)
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}