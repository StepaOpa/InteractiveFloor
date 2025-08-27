using UnityEngine;

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

    [SerializeField] private int health = 3;
    private bool isMoving = false;
    private Quaternion originRotation;
    private float uiInput = 0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        originRotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
    }




    void Movement()
    {
        // Объединяем ввод от клавиатуры и UI кнопок
        float keyboardInput = Input.GetAxis("Horizontal");
        float horizontalInput = Mathf.Clamp(keyboardInput + uiInput, -1f, 1f);

        transform.Translate(Vector3.right * horizontalInput * moveSpeed * Time.deltaTime, Space.World);
        Tilt(horizontalInput);
        // SmallRotation(horizontalInput);
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

    /// <summary>
    /// Установить конкретное значение движения (для слайдеров или джойстика)
    /// </summary>
    /// <param name="input">Значение от -1 до 1</param>
    public void SetMovementInput(float input)
    {
        uiInput = Mathf.Clamp(input, -1f, 1f);
    }

    public void TakeDamage(int damage)
    {
        health -= damage;
        Debug.Log("Icebreaker health: " + health);
        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log("Icebreaker is dead");
    }

}
