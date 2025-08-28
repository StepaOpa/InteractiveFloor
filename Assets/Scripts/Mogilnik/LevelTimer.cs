using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelTimer : MonoBehaviour
{
    [Header("Настройки таймера")]
    [SerializeField] private float levelTime = 120f;

    [Header("UI Элементы")]
    [SerializeField] private Image radialProgressImage;
    [SerializeField] private TextMeshProUGUI timerText;

    private float timeRemaining;
    private bool isTimerRunning = false;
    private bool timeUpHandled = false; // <-- ВОТ ЭТОТ ФЛАГ РЕШИТ ПРОБЛЕМУ

    void Update()
    {
        if (isTimerRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                UpdateTimerUI();
            }
            else
            {
                timeRemaining = 0;
                isTimerRunning = false;
                UpdateTimerUI();
                TimeUp();
            }
        }
    }

    private void UpdateTimerUI()
    {
        if (radialProgressImage != null)
        {
            radialProgressImage.fillAmount = timeRemaining / levelTime;
        }

        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public void StartTimer()
    {
        timeRemaining = levelTime;
        isTimerRunning = true;
        timeUpHandled = false; // <-- СБРАСЫВАЕМ ФЛАГ ПРИ СТАРТЕ НОВОГО ТАЙМЕРА
        Debug.Log("[LevelTimer] Таймер запущен!");
    }

    public void StopTimer()
    {
        isTimerRunning = false;
        Debug.Log("[LevelTimer] Таймер остановлен.");
    }

    private void TimeUp()
    {
        if (timeUpHandled) return; // <-- ЕСЛИ МЕТОД УЖЕ ВЫЗЫВАЛСЯ, ВЫХОДИМ
        timeUpHandled = true;

        Debug.LogWarning("[LevelTimer] ВРЕМЯ ВЫШЛО!");

        // Принудительно прячем UI осмотра
        if (InspectionUI.Instance != null)
        {
            InspectionUI.Instance.ForceHide();
        }

        // Вызываем экран поражения через GameManager
        GameManager.Instance.ShowLoseScreen(
            UIController.Instance.GetCurrentScore(),
            UIController.Instance.GetCurrentLevel()
        );
    }
}