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
    private bool timeUpHandled = false;
    private bool isTickingSoundStarted = false;

    void Update()
    {
        if (isTimerRunning)
        {
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;

                if (timeRemaining <= 5.0f && !isTickingSoundStarted)
                {
                    isTickingSoundStarted = true;
                    if (SoundManager.Instance != null)
                    {
                        SoundManager.Instance.StartTickingSound();
                    }
                }

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
        timeUpHandled = false;
        isTickingSoundStarted = false; // Сбрасываем флаг при старте
        Debug.Log("[LevelTimer] Таймер запущен!");
    }

    public void StopTimer()
    {
        isTimerRunning = false;
        
        if (isTickingSoundStarted && SoundManager.Instance != null)
        {
            SoundManager.Instance.StopTickingSound();
            isTickingSoundStarted = false; // Сбрасываем флаг
        }

        Debug.Log("[LevelTimer] Таймер остановлен.");
    }

    private void TimeUp()
    {
        if (timeUpHandled) return;
        timeUpHandled = true;

        Debug.LogWarning("[LevelTimer] ВРЕМЯ ВЫШЛО!");

        // GameManager теперь сам остановит звук, но для надежности можно оставить
        if (isTickingSoundStarted && SoundManager.Instance != null)
        {
            SoundManager.Instance.StopTickingSound();
            isTickingSoundStarted = false;
        }

        if (InspectionUI.Instance != null)
        {
            InspectionUI.Instance.ForceHide();
        }

        if (GameManager.Instance != null) // Добавим проверку на всякий случай
        {
            GameManager.Instance.ShowLoseScreen(
                UIController.Instance.GetCurrentScore(),
                UIController.Instance.GetCurrentLevel()
            );
        }
    }

    // --- ДОБАВЛЕНО ДЛЯ НАДЕЖНОСТИ ---
    // Этот метод автоматически вызывается Unity перед уничтожением объекта
    // (например, при перезагрузке сцены). Это наша главная страховка.
    private void OnDestroy()
    {
        // Если звук тикал, и мы покидаем сцену, его нужно принудительно остановить.
        if (isTickingSoundStarted && SoundManager.Instance != null)
        {
            SoundManager.Instance.StopTickingSound();
        }
    }
}