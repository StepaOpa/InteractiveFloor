using UnityEngine;
using TMPro;

public class IcebreakerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float maxTiltAngle = 15f;
    [SerializeField] private float tiltSpeed = 3f;
    [SerializeField] private float maxRotation = 15f;
    [SerializeField] private float rotationSpeed = 3f;
    [SerializeField] private float minX = -8f;
    [SerializeField] private float maxX = 8f;

    [Header("Stats & UI")]
    [SerializeField] private int health = 5;
    [SerializeField] private float winDistance = 100f; // <-- ПОБЕДНОЕ РАССТОЯНИЕ

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI distanceText;
    [SerializeField] private GameObject endGameScreen;      // <-- Ссылка на экран конца игры
    [SerializeField] private TextMeshProUGUI titleText;        // <-- Ссылка на ЗАГОЛОВОК (Победа/Поражение)
    [SerializeField] private TextMeshProUGUI detailsText;      // <-- Ссылка на текст с деталями
    [SerializeField] private CoinRewardController coinRewardController; // <-- НОВОЕ: Ссылка на скрипт монеток

    private int maxHealth;
    private float distanceTraveled = 0f;
    private bool isGameEnded = false; // <-- Переименовали для ясности

    private Quaternion originRotation;
    private float uiInput = 0f;

    void Start()
    {
        isGameEnded = false;
        Time.timeScale = 1f;
        endGameScreen.SetActive(false);
        originRotation = transform.rotation;

        maxHealth = health;
        UpdateHealthUI();
        UpdateDistanceUI();
    }

    void Update()
    {
        if (isGameEnded) return;

        Movement();

        // Проверяем условие победы
        if (distanceTraveled < winDistance)
        {
            distanceTraveled += Time.deltaTime;
            UpdateDistanceUI();
        }
        else
        {
            // Мы проплыли 100 метров!
            EndGame(true); // Вызываем конец игры с флагом "победа"
        }
    }

    public void TakeDamage(int damage)
    {
        if (isGameEnded) return;

        health -= damage;
        if (health < 0) health = 0;

        UpdateHealthUI();

        if (health <= 0)
        {
            EndGame(false); // Вызываем конец игры с флагом "поражение"
        }
    }

    // ========== НОВЫЙ ЦЕНТРАЛЬНЫЙ МЕТОД КОНЦА ИГРЫ ==========
    private void EndGame(bool isVictory)
    {
        if (isGameEnded) return; // Защита от повторного вызова

        isGameEnded = true;
        Time.timeScale = 0f; // Ставим игру на паузу (важно для анимации монеток)

        // 1. Рассчитываем награду
        int coinsToAward = health * 2;

        // 2. Настраиваем текст на финальном экране
        if (isVictory)
        {
            titleText.text = "Победа!";
        }
        else
        {
            titleText.text = "Поражение";
        }

        int finalDistance = Mathf.FloorToInt(distanceTraveled);
        detailsText.text = "Пройденное расстояние: " + finalDistance + " м\n" +
                           "Заработано монеток: " + coinsToAward;

        // 3. Показываем экран и запускаем анимацию монеток
        endGameScreen.SetActive(true);
        coinRewardController.StartRewardSequence(coinsToAward);
    }

    #region Старые методы (движение, UI, и т.д.)
    void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = "Прочность корпуса: " + health + " / " + maxHealth;
        }
    }

    void UpdateDistanceUI()
    {
        if (distanceText != null)
        {
            distanceText.text = "Расстояние: " + Mathf.FloorToInt(distanceTraveled) + " м";
        }
    }

    void Movement()
    {
        float keyboardInput = Input.GetAxis("Horizontal");
        float horizontalInput = Mathf.Clamp(keyboardInput + uiInput, -1f, 1f);
        Vector3 newPosition = transform.position + Vector3.right * horizontalInput * moveSpeed * Time.deltaTime;
        newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX);
        transform.position = newPosition;
        Tilt(horizontalInput);
    }

    void Tilt(float horizontalInput)
    {
        float tilt = Mathf.Lerp(0, maxTiltAngle, Mathf.Abs(horizontalInput));
        float rotation = Mathf.Lerp(0, maxRotation, Mathf.Abs(horizontalInput));
        originRotation = Quaternion.Euler(0, rotation * horizontalInput, tilt * horizontalInput);
        transform.rotation = Quaternion.Slerp(transform.rotation, originRotation, tiltSpeed * Time.deltaTime);
    }

    public void StartMoveLeft() { uiInput = -1f; }
    public void StartMoveRight() { uiInput = 1f; }
    public void StopMovement() { uiInput = 0f; }
    public void SetMovementInput(float input) { uiInput = Mathf.Clamp(input, -1f, 1f); }
    #endregion
}