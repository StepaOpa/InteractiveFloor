using UnityEngine;

public class StoneMovement : MonoBehaviour
{
    public float speed = 5f; // Скорость движения камня

    void Update()
    {
        // Двигаем объект "назад" (в сторону игрока) с постоянной скоростью
        transform.Translate(Vector3.back * speed * Time.deltaTime);
    }
}