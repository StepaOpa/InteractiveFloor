using UnityEngine;

public class CameraControllerLabyrinth : MonoBehaviour
{
    [Header("Цели для камеры")]
    public Transform closeViewTarget; // "Якорь", который дочерний у рыбы
    public Transform farViewTarget;   // "Якорь" для общего плана

    [Header("Настройки")]
    public float smoothSpeed = 5f; // Скорость, с которой камера будет двигаться

    private bool isCloseView = true; // Начинаем с приближенного вида

    // LateUpdate вызывается после всех Update. Идеально для камер,
    // чтобы избежать дрожания, когда камера и игрок движутся в одном кадре.
    void LateUpdate()
    {
        Transform target = isCloseView ? closeViewTarget : farViewTarget;

        // Плавно перемещаем камеру в позицию цели
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, target.position, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        // Плавно поворачиваем камеру в сторону цели
        Quaternion smoothedRotation = Quaternion.Lerp(transform.rotation, target.rotation, smoothSpeed * Time.deltaTime);
        transform.rotation = smoothedRotation;
    }

    // --- Методы для кнопок UI ---

    public void SetCloseView()
    {
        isCloseView = true;
    }

    public void SetFarView()
    {
        isCloseView = false;
    }

    // --- Метод для GameManager'а ---
    // Он будет принудительно переключать на общий план перед концом игры
    public void SwitchToFarViewImmediately()
    {
        isCloseView = false;
    }
}