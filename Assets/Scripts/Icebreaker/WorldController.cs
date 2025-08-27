using System.Collections.Generic;
using UnityEngine;

public class WorldController : MonoBehaviour
{
    [Header("Настройки чанков")]
    [SerializeField] private GameObject chunkPrefab; // Массив префабов чанков
    [SerializeField] private GameObject crackedChunkPrefab; // Массив расколотых версий чанков

    [SerializeField] private GameObject[] obstaclePrefabs;

    [SerializeField] private int groundWidth = 2;
    [SerializeField] private Transform chunkSpawnPoint;
    [SerializeField] private float chunkSpeed = 10f;
    [SerializeField] private Transform replaceChunkPoint;
    [SerializeField] private float chunkLength = 40f; // точная длина чанка по оси Z для стыковки без шва
    private List<GameObject> activeChunks = new List<GameObject>();
    private List<GameObject> activeObjects = new List<GameObject>();

    private void Start()
    {
        SpawnChunk(chunkPrefab, chunkSpawnPoint.position.z);
    }

    private void Update()
    {
        MoveChunks();
        CheckNumberOfChunks();
        ReplaceChunk();
    }


    private void CheckNumberOfChunks()
    {
        // Чистим null-элементы из списка активных чанков (могли быть уничтожены где-то ещё)
        for (int i = activeChunks.Count - 1; i >= 0; i--)
        {
            if (activeChunks[i] == null)
            {
                activeChunks.RemoveAt(i);
            }
        }

        if (activeChunks.Count == 0)
        {
            SpawnChunk(chunkPrefab, chunkSpawnPoint.position.z);
            return;
        }

        GameObject lastChunk = activeChunks[activeChunks.Count - 1];
        if (lastChunk != null && activeChunks.Count < 10)
        {
            float targetZ = lastChunk.transform.position.z + chunkLength;
            if (targetZ <= chunkSpawnPoint.position.z)
            {
                SpawnChunk(chunkPrefab, targetZ);
            }
        }
    }


    private void SpawnChunk(GameObject prefab, float z)
    {
        for (int i = -40 * groundWidth; i <= 40 * groundWidth; i += 40)
        {
            GameObject newChunk = Instantiate(prefab, new Vector3(i, 0, z), Quaternion.identity, chunkSpawnPoint != null ? chunkSpawnPoint.parent : null);
            if (newChunk.transform.position.x == 0)
            {
                activeChunks.Add(newChunk);
                ChunkObstacleSpawner obstacleSpawner = newChunk.GetComponent<ChunkObstacleSpawner>();
                activeObjects.AddRange(obstacleSpawner.SpawnObstacles(chunkSpawnPoint));
            }

            activeObjects.Add(newChunk);

        }

    }

    private void MoveChunks()
    {
        // Идём с конца, чтобы безопасно удалять null-объекты из списка
        for (int i = activeObjects.Count - 1; i >= 0; i--)
        {
            GameObject obj = activeObjects[i];
            if (obj == null)
            {
                activeObjects.RemoveAt(i);
                continue;
            }

            Transform t = obj.transform;
            if (t == null)
            {
                activeObjects.RemoveAt(i);
                continue;
            }

            // Двигаем по Z, сохраняя текущие X/Y
            Vector3 p = t.position;
            p.z -= chunkSpeed * Time.deltaTime;
            t.position = p;
        }
    }

    private Vector3 GetClosestChunkPosition()
    {
        // Предполагается, что самый первый — ближайший к игроку
        for (int i = 0; i < activeChunks.Count; i++)
        {
            if (activeChunks[i] != null)
            {
                return activeChunks[i].transform.position;
            }
        }
        // Если нет валидных чанков — возвращаем большое значение вперёд
        return new Vector3(0, 0, float.MaxValue);
    }

    private void ReplaceChunk()
    {
        Vector3 closestPosition = GetClosestChunkPosition();
        if (closestPosition.z < replaceChunkPoint.position.z && closestPosition.x == 0 && closestPosition.y == 0)
        {
            // Находим первый валидный чанк (который собираемся заменить)
            int firstValidIndex = -1;
            for (int i = 0; i < activeChunks.Count; i++)
            {
                if (activeChunks[i] != null)
                {
                    firstValidIndex = i;
                    break;
                }
            }

            if (firstValidIndex == -1)
            {
                return;
            }

            GameObject chunkToReplace = activeChunks[firstValidIndex];
            // Сохраняем точное положение и родителя исходного чанка, чтобы не было зазора
            Vector3 pos = chunkToReplace.transform.position;
            Quaternion rot = chunkToReplace.transform.rotation;
            Transform parent = chunkToReplace.transform.parent;
            GameObject newChunk = Instantiate(crackedChunkPrefab, pos, rot, parent);

            // Уничтожаем старый чанк и удаляем его из списков
            if (chunkToReplace != null)
            {
                Destroy(chunkToReplace);
            }

            activeChunks.RemoveAt(firstValidIndex);

            // Удаляем соответствующий объект из activeObjects (если он там есть)
            int idxInActiveObjects = activeObjects.IndexOf(chunkToReplace);
            if (idxInActiveObjects >= 0)
            {
                activeObjects.RemoveAt(idxInActiveObjects);
            }

            // Добавляем новый расколотый чанк в общий список движимых объектов
            activeObjects.Add(newChunk);
        }
    }


}
