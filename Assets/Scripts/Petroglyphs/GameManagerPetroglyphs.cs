using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro; // Важно! Добавь эту строку для работы с TextMeshPro


public class GameManagerPetroglyphs : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image petroglyphToFindImage;
    [SerializeField] private Image timerImage; // Радиальный прогресс-бар
    [SerializeField] private TextMeshProUGUI timerText; // Текстовый таймер (цифры)
    [SerializeField] private GameEndPanelPetroglyphs endPanel; // Ссылка на скрипт нашей новой панели

    [Header("Game Settings")]
    [SerializeField] private List<Sprite> allPetroglyphSprites;
    [SerializeField] private float timePerPetroglyph = 15f;

    private List<Sprite> availablePetroglyphs;
    private Sprite currentPetroglyph;
    private float currentTime;
    private bool isGameActive = true;

    // --- Новые переменные для статистики ---
    private int foundPetroglyphsCount = 0;
    private int totalPetroglyphsCount = 0;

    void Start()
    {
        // Скрываем панель окончания игры в самом начале
        endPanel.gameObject.SetActive(false);

        availablePetroglyphs = new List<Sprite>(allPetroglyphSprites);
        totalPetroglyphsCount = allPetroglyphSprites.Count; // Запоминаем, сколько всего было рисунков
        foundPetroglyphsCount = 0; // Сбрасываем счетчик найденных

        SelectNextPetroglyph();
    }

    void Update()
    {
        if (!isGameActive) return;

        currentTime -= Time.deltaTime;

        // --- Обновление UI таймера (ИЗМЕНЕНО) ---
        // Обновляем радиальный прогресс-бар
        timerImage.fillAmount = currentTime / timePerPetroglyph;
        // Обновляем текст. Mathf.CeilToInt округляет до ближайшего большего целого (14.1 -> 15)
        timerText.text = Mathf.CeilToInt(currentTime).ToString();

        if (currentTime <= 0)
        {
            // Исправляем, чтобы таймер не показывал отрицательные числа
            timerText.text = "0";
            LoseGame();
        }
    }

    public void SelectNextPetroglyph()
    {
        if (availablePetroglyphs.Count > 0)
        {
            ResetTimer();
            int randomIndex = Random.Range(0, availablePetroglyphs.Count);
            currentPetroglyph = availablePetroglyphs[randomIndex];
            petroglyphToFindImage.sprite = currentPetroglyph;
            availablePetroglyphs.RemoveAt(randomIndex);
            Debug.Log("Нужно найти: " + currentPetroglyph.name);
        }
        else
        {
            WinGame();
        }
    }

    public void CheckFoundPetroglyph(PetroglyphLocation foundLocation)
    {
        if (!isGameActive) return;

        if (foundLocation.petroglyphSprite == currentPetroglyph)
        {
            Debug.Log("ПРАВИЛЬНО! Найден петroglyph: " + currentPetroglyph.name);
            foundPetroglyphsCount++; // Увеличиваем счетчик найденных
            SelectNextPetroglyph();
        }
        else
        {
            Debug.Log("Неверно. Это другой рисунок.");
        }
    }

    private void ResetTimer()
    {
        currentTime = timePerPetroglyph;
    }

    // --- Логика концовок (ИЗМЕНЕНО) ---
    private void WinGame()
    {
        isGameActive = false;
        Debug.Log("Поздравляю! Вы нашли все петроглифы!");
        // Вызываем метод на панели, передаем ему, что это победа, и статистику
        endPanel.ShowPanel(true, foundPetroglyphsCount, totalPetroglyphsCount);
    }

    private void LoseGame()
    {
        isGameActive = false;
        Debug.Log("Время вышло! Вы проиграли.");
        // Вызываем метод на панели, передаем ему, что это поражение, и статистику
        endPanel.ShowPanel(false, foundPetroglyphsCount, totalPetroglyphsCount);
    }
}