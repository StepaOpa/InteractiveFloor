
using UnityEngine;

// Название класса теперь SpawnerStones
public class SpawnerStones : MonoBehaviour
{
    public GameObject[] stonePrefabs;
    public float spawnRate = 1f;
    public float spawnAreaWidth = 5f;

    private float nextSpawnTime;

    void Update()
    {
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
        int randomIndex = Random.Range(0, stonePrefabs.Length);
        GameObject randomStonePrefab = stonePrefabs[randomIndex];

        float randomX = Random.Range(-spawnAreaWidth / 2, spawnAreaWidth / 2);
        Vector3 spawnPosition = new Vector3(randomX, transform.position.y, transform.position.z);

        Instantiate(randomStonePrefab, spawnPosition, Quaternion.identity);
    }
}