
using UnityEngine;

public class Spawner : MonoBehaviour
{
    // Массив для хранения всех ваших префабов камней
    public GameObject[] stonePrefabs;
    public float spawnRate = 1f; // Как часто появляются камни (1 раз в секунду)
    public float spawnAreaWidth = 5f; // Ширина зоны спауна

    private float nextSpawnTime;

    void Update()
    {
        // Проверка, чтобы избежать ошибок, если вы не добавили префабы в инспекторе
        if (stonePrefabs.Length == 0)
        {
            Debug.LogError("Массив префабов камней пуст! Добавьте префабы в инспекторе.");
            return;
        }

        if (Time.time > nextSpawnTime)
        {
            SpawnStone();
            nextSpawnTime = Time.time + 1f / spawnRate;
        }
    }

    void SpawnStone()
    {
        // Выбираем случайный префаб из массива
        int randomIndex = Random.Range(0, stonePrefabs.Length);
        GameObject randomStonePrefab = stonePrefabs[randomIndex];

        // Вычисляем случайную позицию для спауна по ширине
        float randomX = Random.Range(-spawnAreaWidth / 2, spawnAreaWidth / 2);
        Vector3 spawnPosition = new Vector3(randomX, transform.position.y, transform.position.z);

        // Создаем (спауним) выбранный камень в вычисленной позиции
        Instantiate(randomStonePrefab, spawnPosition, Quaternion.identity);
    }
}