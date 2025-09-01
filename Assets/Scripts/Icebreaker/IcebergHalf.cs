using UnityEngine;
using System.Collections;

public class IcebergHalf : MonoBehaviour
{
    // Эта функция будет принимать команду "начать тонуть" извне
    public void StartSinking(float delay, float speed)
    {
        // Запускаем корутину на ЭТОМ ЖЕ объекте (половинке)
        StartCoroutine(SinkCoroutine(delay, speed));
    }

    private IEnumerator SinkCoroutine(float sinkDelay, float sinkSpeed)
    {
        // Ждем немного
        yield return new WaitForSeconds(sinkDelay);

        // Плавно топим объект
        while (this != null) // Используем 'this' для большей надежности
        {
            transform.Translate(Vector3.down * sinkSpeed * Time.deltaTime, Space.World);
            yield return null;
        }
    }
}