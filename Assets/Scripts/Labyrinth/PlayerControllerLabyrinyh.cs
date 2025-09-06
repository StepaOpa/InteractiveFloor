using UnityEngine;

public class PlayerControllerLabyrinth : MonoBehaviour
{
    // --- Переменные для движения ---
    public float moveSpeed = 5.0f;
    private Rigidbody rb;
    private Vector3 moveDirection = Vector3.zero;

    // --- Ссылка на главный управляющий скрипт ---
    private GameManagerLabyrinth gameManager;

    void Start()
    {
        // Получаем компонент Rigidbody для управления физикой
        rb = GetComponent<Rigidbody>();
        // Находим на сцене объект со скриптом GameManagerLabyrinth и "запоминаем" его
        gameManager = FindObjectOfType<GameManagerLabyrinth>();
    }

    void FixedUpdate()
    {
        // Считываем ввод с клавиатуры (стрелки или WASD)
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        Vector3 keyboardMovement = new Vector3(moveHorizontal, 0.0f, moveVertical);

        // Складываем движение от клавиатуры и от UI-кнопок
        Vector3 totalMovement = keyboardMovement + moveDirection;

        // Применяем силу к объекту, чтобы он двигался.
        // .normalized нужен, чтобы скорость по диагонали не была выше
        rb.AddForce(totalMovement.normalized * moveSpeed);
    }

    // --- НОВЫЙ МЕТОД ---
    // Этот метод автоматически вызывается Unity, когда наш объект входит в триггер
    private void OnTriggerEnter(Collider other)
    {
        // Проверяем тег объекта, в который мы вошли
        if (other.CompareTag("WinZone"))
        {
            // Если это зона победы, сообщаем GameManager'у
            gameManager.WinGame();
        }

        if (other.CompareTag("Trap"))
        {
            // Если это ловушка, сообщаем GameManager'у
            gameManager.LoseGame("Вы попались в сети!");
        }
    }

    // --- Методы для UI-кнопок (остаются без изменений) ---
    // Вызываются, когда кнопка НАЖАТА
    public void OnPointerDownForward() { moveDirection.z = 1; }
    public void OnPointerDownBack() { moveDirection.z = -1; }
    public void OnPointerDownLeft() { moveDirection.x = -1; }
    public void OnPointerDownRight() { moveDirection.x = 1; }

    // Вызываются, когда кнопка ОТПУЩЕНА
    public void OnPointerUpForward() { moveDirection.z = 0; }
    public void OnPointerUpBack() { moveDirection.z = 0; }
    public void OnPointerUpLeft() { moveDirection.x = 0; }
    public void OnPointerUpRight() { moveDirection.x = 0; }
}