using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Эта переменная будет видна в инспекторе Unity.
    // Она определяет, как быстро будет двигаться рыба.
    public float moveSpeed = 5.0f;

    // Ссылка на компонент Rigidbody, который отвечает за физику.
    private Rigidbody rb;

    // Start вызывается один раз при запуске игры.
    void Start()
    {
        // Находим и сохраняем компонент Rigidbody, который висит на этом же объекте.
        rb = GetComponent<Rigidbody>();
    }

    // Update вызывается каждый кадр. Здесь мы будем считывать ввод.
    // Но двигать будем в FixedUpdate для стабильной работы физики.
    void FixedUpdate()
    {
        // Получаем ввод с клавиатуры (стрелки или WASD).
        // Input.GetAxis вернет значение от -1 до 1.
        float moveHorizontal = Input.GetAxis("Horizontal"); // A, D, стрелки влево/вправо
        float moveVertical = Input.GetAxis("Vertical");     // W, S, стрелки вверх/вниз

        // Создаем вектор направления движения.
        // Мы двигаемся по осям X и Z (по горизонтальной плоскости).
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

        // Применяем силу к Rigidbody, чтобы двигать рыбу.
        // Мы умножаем направление на скорость, чтобы контролировать быстроту.
        rb.AddForce(movement * moveSpeed);
    }
}