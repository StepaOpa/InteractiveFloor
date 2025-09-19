using UnityEngine;

public class DirtPile : MonoBehaviour
{
    [Header("Настройки опускания")]
    [Tooltip("Сколько касаний мышкой нужно, чтобы полностью опустить кучку.")]
    [SerializeField] private int touchesToLower = 5;

    [Tooltip("На какую высоту кучка опускается за одно касание.")]
    [SerializeField] private float heightStep = 0.1f;

    // --- НОВОЕ: Настраиваемая скорость "копания" ---
    [Tooltip("Задержка в секундах между 'касаниями'. Чем меньше, тем быстрее копает.")]
    [SerializeField] private float digCooldown = 0.1f;

    private int currentTouches = 0;
    private Vector3 initialPosition;

    // --- НОВОЕ: Переменная для хранения времени с последнего удачного 'касания' ---
    private float timeSinceLastDig = 0f;

    void Start()
    {
        initialPosition = transform.position;
    }

    // --- НОВОЕ: Метод Update для работы с таймером ---
    void Update()
    {
        // Каждый кадр мы увеличиваем наш счетчик времени
        timeSinceLastDig += Time.deltaTime;
    }

    public void Dig()
    {
        // --- ГЛАВНОЕ ИЗМЕНЕНИЕ ---
        // Прежде чем что-то делать, проверяем, прошла ли задержка (cooldown)
        if (timeSinceLastDig < digCooldown)
        {
            return; // Если времени прошло мало, выходим из метода
        }

        if (currentTouches >= touchesToLower)
        {
            return;
        }

        // Если все проверки пройдены, сбрасываем таймер
        timeSinceLastDig = 0f;

        currentTouches++;

        float newY = initialPosition.y - (currentTouches * heightStep);
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}