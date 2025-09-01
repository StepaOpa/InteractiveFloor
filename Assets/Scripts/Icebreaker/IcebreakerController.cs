using UnityEngine;
using TMPro; // Не забываем эту строчку для работы с UI текстом

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
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private TextMeshProUGUI distanceText;

    private int maxHealth;
    private float distanceTraveled = 0f;

    private bool isMoving = false;
    private Quaternion originRotation;
    private float uiInput = 0f;

    void Start()
    {
        originRotation = transform.rotation;

        maxHealth = health;
        UpdateHealthUI();
        UpdateDistanceUI();
    }

    void Update()
    {
        Movement();

        if (distanceTraveled < 100f)
        {
            distanceTraveled += Time.deltaTime;
            UpdateDistanceUI();
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

    private bool IsShipMoving()
    {
        Vector3 currentRotation = transform.rotation.eulerAngles;
        return currentRotation.y != 0 || currentRotation.z != 0;
    }

    // ========== UI КНОПКИ ==========

    public void StartMoveLeft()
    {
        uiInput = -1f;
    }


    public void StartMoveRight()
    {
        uiInput = 1f;
    }


    public void StopMovement()
    {
        uiInput = 0f;
    }

    public void SetMovementInput(float input)
    {
        uiInput = Mathf.Clamp(input, -1f, 1f);
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        if (health < 0) health = 0;

        Debug.Log("Icebreaker health: " + health);
        UpdateHealthUI();

        if (health <= 0)
        {
            Die();
        }
    }

    // ========== Обновление UI ==========

    void UpdateHealthUI()
    {
        if (healthText != null)
        {
            // ЗАМЕНИТЕ "Прочность корпуса" НА ВАШ ВЫБОР
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

    private void Die()
    {
        Debug.Log("Icebreaker is dead");
    }
}