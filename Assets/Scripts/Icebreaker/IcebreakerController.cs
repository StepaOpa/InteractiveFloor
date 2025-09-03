using UnityEngine;
using TMPro;
using System.Collections;

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
    [SerializeField] private float winDistance = 100f;

    [Header("UI References")]
    [SerializeField] private GameObject endGameScreen;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI detailsText;
    [SerializeField] private CoinControllerIcebreaker coinRewardController;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI distanceText;

    [Header("Animation")]
    [Tooltip("Перетащи сюда Main Camera")]
    [SerializeField] private Animator cameraAnimator;
    [SerializeField] private float endScreenDelay = 1.5f;

    private int maxHealth;
    private float distanceTraveled = 0f;
    private bool isGameEnded = false;
    private Quaternion originRotation;
    private float uiInput = 0f;

    private void EndGame(bool isVictory)
    {
        if (isGameEnded) return;

        StartCoroutine(EndGameSequence(isVictory));
    }

    private IEnumerator EndGameSequence(bool isVictory)
    {
        // === ЧАСТЬ 1: Выполняется сразу ===
        isGameEnded = true;
        Time.timeScale = 0f;

        // ========== ИЗМЕНЕНИЕ 1: Жестко выключаем экран перед паузой ==========
        // Это гарантирует, что он не появится сам по себе.
        endGameScreen.SetActive(false);

        // Запускаем анимацию камеры
        if (cameraAnimator != null)
        {
            cameraAnimator.SetTrigger("StartEndAnimation");
        }

        // === ЧАСТЬ 2: Пауза ===
        yield return new WaitForSecondsRealtime(endScreenDelay);

        // === ЧАСТЬ 3: Выполняется после паузы ===

        // ========== ИЗМЕНЕНИЕ 2: Теперь включаем экран ==========
        // Только после паузы мы разрешаем ему появиться
        endGameScreen.SetActive(true);

        int coinsToAward = health * 2;

        if (isVictory) { titleText.text = "Победа!"; }
        else { titleText.text = "Корабль потоплен"; }

        int finalDistance = Mathf.FloorToInt(distanceTraveled);
        detailsText.text = "Пройденное расстояние: " + finalDistance + " м\n" +
                           "Заработано монеток: " + coinsToAward;

        coinRewardController.StartRewardSequence(coinsToAward);
    }

    #region Остальные методы (без изменений)
    void Start() { isGameEnded = false; Time.timeScale = 1f; endGameScreen.SetActive(false); originRotation = transform.rotation; maxHealth = health; UpdateHealthUI(); UpdateDistanceUI(); }
    void Update() { if (isGameEnded) return; Movement(); if (distanceTraveled < winDistance) { distanceTraveled += Time.deltaTime; UpdateDistanceUI(); } else { EndGame(true); } }
    public void TakeDamage(int damage) { if (isGameEnded) return; health -= damage; if (health < 0) health = 0; UpdateHealthUI(); if (health <= 0) { EndGame(false); } }
    void UpdateHealthUI() { if (healthText != null) { healthText.text = "Прочность корпуса: " + health + " / " + maxHealth; } }
    void UpdateDistanceUI() { if (distanceText != null) { distanceText.text = "Расстояние: " + Mathf.FloorToInt(distanceTraveled) + " м"; } }
    void Movement() { float keyboardInput = Input.GetAxis("Horizontal"); float horizontalInput = Mathf.Clamp(keyboardInput + uiInput, -1f, 1f); Vector3 newPosition = transform.position + Vector3.right * horizontalInput * moveSpeed * Time.deltaTime; newPosition.x = Mathf.Clamp(newPosition.x, minX, maxX); transform.position = newPosition; Tilt(horizontalInput); }
    void Tilt(float horizontalInput) { float tilt = Mathf.Lerp(0, maxTiltAngle, Mathf.Abs(horizontalInput)); float rotation = Mathf.Lerp(0, maxRotation, Mathf.Abs(horizontalInput)); originRotation = Quaternion.Euler(0, rotation * horizontalInput, tilt * horizontalInput); transform.rotation = Quaternion.Slerp(transform.rotation, originRotation, tiltSpeed * Time.deltaTime); }
    public void StartMoveLeft() { uiInput = -1f; }
    public void StartMoveRight() { uiInput = 1f; }
    public void StopMovement() { uiInput = 0f; }
    public void SetMovementInput(float input) { uiInput = Mathf.Clamp(input, -1f, 1f); }
    #endregion
}