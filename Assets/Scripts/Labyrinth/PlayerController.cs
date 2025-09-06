using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    private Rigidbody rb;

    // Этот вектор будет хранить направление движения от кнопок.
    private Vector3 moveDirection = Vector3.zero;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // --- Движение от клавиатуры (оставляем его) ---
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        Vector3 keyboardMovement = new Vector3(moveHorizontal, 0.0f, moveVertical);

        // --- Движение от UI кнопок ---
        // Складываем движение от клавиатуры и от кнопок.
        // Если нажата и клавиша, и кнопка, движение будет быстрее.
        // Если нужно, чтобы работало что-то одно, нужно будет усложнить логику.
        Vector3 totalMovement = keyboardMovement + moveDirection;

        rb.AddForce(totalMovement.normalized * moveSpeed);
    }

    // --- Новые публичные методы для кнопок ---

    // Этот метод будет вызываться, когда кнопка "Вперед" НАЖАТА
    public void OnPointerDownForward()
    {
        moveDirection.z = 1;
    }

    // Этот метод будет вызываться, когда кнопка "Вперед" ОТПУЩЕНА
    public void OnPointerUpForward()
    {
        moveDirection.z = 0;
    }

    public void OnPointerDownBack()
    {
        moveDirection.z = -1;
    }

    public void OnPointerUpBack()
    {
        moveDirection.z = 0;
    }

    public void OnPointerDownLeft()
    {
        moveDirection.x = -1;
    }

    public void OnPointerUpLeft()
    {
        moveDirection.x = 0;
    }

    public void OnPointerDownRight()
    {
        moveDirection.x = 1;
    }

    public void OnPointerUpRight()
    {
        moveDirection.x = 0;
    }
}