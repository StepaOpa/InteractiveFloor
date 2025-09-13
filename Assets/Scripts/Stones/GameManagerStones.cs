using UnityEngine;
using UnityEngine.UI; // Обязательно добавляем для работы с компонентом Image
using TMPro;


public class GameManagerStones : MonoBehaviour
{
    // --- ПЕРЕМЕННЫЕ ИГРОВОЙ ЛОГИКИ ---
    [Header("Game Logic")]
    public int coins = 0;
    public int targetCoins = 10; // Цель по сбору монеток
    public float gameTime = 30f; // Начальное время игры

    // --- ССЫЛКИ НА UI ЭЛЕМЕНТЫ ---
    [Header("UI Elements")]
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI timerText;
    public Image timerRadialBar; // Новая ссылка на наш прогресс-бар

    private float totalGameTime; // Переменная для хранения общего времени
    private bool isGameActive = true;

    void Start()
    {
        totalGameTime = gameTime; // Сохраняем начальное время
        UpdateCoinText();
        UpdateTimeText();
    }

    void Update()
    {
        if (!isGameActive) return;

        // --- ЛОГИКА ТАЙМЕРА ---
        gameTime -= Time.deltaTime;
        UpdateTimeText();
        UpdateRadialBar(); // Обновляем наш прогресс-бар каждый кадр

        if (gameTime <= 0)
        {
            gameTime = 0;
            UpdateTimeText();
            UpdateRadialBar();
            LoseGame();
        }

        // --- ЛОГИКА КЛИКОВ ---
        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
    }

    void HandleClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("ValuableStone"))
            {
                coins++;
                UpdateCoinText();
                Destroy(hit.collider.gameObject);

                if (coins >= targetCoins)
                {
                    WinGame();
                }
            }
        }
    }

    // --- МЕТОДЫ ОБНОВЛЕНИЯ UI ---
    void UpdateCoinText()
    {
        // Обновляем формат текста для счетчика монеток
        coinText.text = $"собрано ценных камней: {coins}/{targetCoins}";
    }

    void UpdateTimeText()
    {
        timerText.text = Mathf.Ceil(gameTime).ToString();
    }

    // Новый метод для обновления радиального бара
    void UpdateRadialBar()
    {
        // fillAmount - это значение от 0 до 1. Делим текущее время на общее.
        timerRadialBar.fillAmount = gameTime / totalGameTime;
    }

    // --- МЕТОДЫ ПОБЕДЫ И ПОРАЖЕНИЯ ---
    void WinGame()
    {
        isGameActive = false;
        Debug.Log("ВЫ ПОБЕДИЛИ!");
    }

    void LoseGame()
    {
        isGameActive = false;
        Debug.Log("ВЫ ПРОИГРАЛИ! Время вышло.");
    }
}