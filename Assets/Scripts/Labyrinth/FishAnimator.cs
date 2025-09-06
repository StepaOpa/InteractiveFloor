using UnityEngine;

public class FishAnimator : MonoBehaviour
{
    [Header("Настройки поворота")]
    [Tooltip("Как быстро рыбка поворачивается в сторону движения.")]
    public float rotationSpeed = 5f;

    [Header("Настройки покачивания")]
    [Tooltip("Амплитуда покачивания по вертикали.")]
    public float bobAmplitude = 0.1f;
    [Tooltip("Скорость покачивания.")]
    public float bobSpeed = 2f;

    // Ссылка на Rigidbody родительского объекта (нашей группы)
    private Rigidbody parentRigidbody;
    // Начальная локальная позиция, чтобы покачивание было относительно неё
    private Vector3 initialLocalPosition;
    // Случайное смещение, чтобы рыбки качались не синхронно
    private float randomOffset;

    void Start()
    {
        // Находим компонент Rigidbody у родителя
        parentRigidbody = GetComponentInParent<Rigidbody>();

        // Запоминаем начальную позицию и случайное смещение
        initialLocalPosition = transform.localPosition;
        randomOffset = Random.Range(0f, 10f); // У каждой рыбки будет своё уникальное движение
    }

    void Update()
    {
        // --- Логика покачивания ---
        // Используем синусоиду для создания плавного движения вверх-вниз
        float yOffset = Mathf.Sin((Time.time * bobSpeed) + randomOffset) * bobAmplitude;

        // Применяем смещение к начальной позиции
        transform.localPosition = initialLocalPosition + new Vector3(0, yOffset, 0);
    }

    void LateUpdate()
    {
        // --- Логика поворота ---
        // Получаем вектор скорости движения группы
        Vector3 movementDirection = parentRigidbody.linearVelocity;

        // Поворачиваем рыбку только если есть движение (чтобы она не смотрела в пол при остановке)
        // sqrMagnitude быстрее, чем magnitude, для простой проверки на ноль
        if (movementDirection.sqrMagnitude > 0.1f)
        {
            // Создаем "целевой" поворот, который смотрит в сторону движения
            Quaternion targetRotation = Quaternion.LookRotation(movementDirection);

            // Плавно поворачиваем текущий объект в сторону целевого поворота
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }
}