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

    // <<< ИЗМЕНЕНО: Ссылка на PlayerController вместо Rigidbody >>>
    private PlayerControllerLabyrinth playerController;
    private Vector3 initialLocalPosition;
    private float randomOffset;

    void Start()
    {
        // <<< ИЗМЕНЕНО: Ищем PlayerController в родительских объектах >>>
        // Это сработает, так как рыбка (или ее pivot) находится внутри GroupFish, на которой висит контроллер
        playerController = GetComponentInParent<PlayerControllerLabyrinth>();

        // Запоминаем начальную позицию и случайное смещение для уникальности анимации
        initialLocalPosition = transform.localPosition;
        randomOffset = Random.Range(0f, 10f);
    }

    void Update()
    {
        // Логика покачивания остается без изменений
        float yOffset = Mathf.Sin((Time.time * bobSpeed) + randomOffset) * bobAmplitude;
        transform.localPosition = initialLocalPosition + new Vector3(0, yOffset, 0);
    }

    void LateUpdate()
    {
        // <<< ИЗМЕНЕНО: Логика поворота теперь основана на вводе игрока, а не на скорости >>>
        if (playerController != null)
        {
            // Получаем вектор последнего ввода из контроллера
            Vector3 movementDirection = playerController.LastInputDirection;

            // Поворачиваем рыбку, если есть направление
            if (movementDirection.sqrMagnitude > 0.01f)
            {
                // Создаем целевой поворот, который "смотрит" в сторону движения
                Quaternion targetRotation = Quaternion.LookRotation(movementDirection);
                // Плавно интерполируем текущий поворот к целевому
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
            }
        }
    }
}