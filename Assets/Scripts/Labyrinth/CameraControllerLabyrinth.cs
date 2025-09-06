using UnityEngine;

public class CameraControllerLabyrinth : MonoBehaviour
{
    [Header("Цели для камеры")]
    public Transform closeViewTarget; // "Якорь", который дочерний у рыбы
    public Transform farViewTarget;   // "Якорь" для общего плана

    [Header("Настройки")]
    // Было: public float smoothSpeed = 5f;
    // Стало: Время, за которое камера догонит цель. Меньше = быстрее!
    public float smoothTime = 0.2f;

    // Эта переменная нужна для работы SmoothDamp, она хранит текущую скорость камеры
    private Vector3 velocity = Vector3.zero;

    private bool isCloseView = true; // Начинаем с приближенного вида

    void LateUpdate()
    {
        Transform target = isCloseView ? closeViewTarget : farViewTarget;

        // --- ЭТА СТРОКА ИЗМЕНИЛАСЬ ---
        // Было: Vector3.Lerp(...)
        // Стало: Vector3.SmoothDamp(...)
        transform.position = Vector3.SmoothDamp(transform.position, target.position, ref velocity, smoothTime);

        // Поворот можно оставить с Lerp, он работает хорошо для вращения
        Quaternion smoothedRotation = Quaternion.Lerp(transform.rotation, target.rotation, 5f * Time.deltaTime);
        transform.rotation = smoothedRotation;
    }

    // --- Остальные методы остаются без изменений ---

    public void SetCloseView()
    {
        isCloseView = true;
    }

    public void SetFarView()
    {
        isCloseView = false;
    }

    public void SwitchToFarViewImmediately()
    {
        isCloseView = false;
    }
}