// FishAnimator.cs

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

    // Ссылка на главный контроллер, чтобы знать, куда он движется
    private PlayerControllerLabyrinth playerController;

    // Переменные для уникальности анимации
    private Vector3 initialLocalPosition;
    private float randomOffset;

    void Start()
    {
        // Находим главный контроллер в родительских объектах
        playerController = GetComponentInParent<PlayerControllerLabyrinth>();

        // Запоминаем начальную позицию и случайное смещение
        initialLocalPosition = transform.localPosition;
        randomOffset = Random.Range(0f, 10f);
    }

    void Update()
    {
        // Эта часть отвечает за анимацию покачивания рыбки вверх-вниз
        float yOffset = Mathf.Sin((Time.time * bobSpeed) + randomOffset) * bobAmplitude;
        transform.localPosition = initialLocalPosition + new Vector3(0, yOffset, 0);
    }

    // LateUpdate используется для поворотов, чтобы они происходили после всех расчетов движения
    void LateUpdate()
    {
        if (playerController != null)
        {
            // Берем реальное направление движения из главного контроллера
            Vector3 movementDirection = playerController.CurrentMovementDirection;

            // Если есть направление (вектор не нулевой)
            if (movementDirection.sqrMagnitude > 0.01f)
            {
                // Создаем целевой поворот, который "смотрит" в сторону движения
                Quaternion targetRotation = Quaternion.LookRotation(movementDirection);
                // Плавно интерполируем (поворачиваем) рыбку к этому целевому повороту
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
        }
    }
}