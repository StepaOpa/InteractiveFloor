using System.Collections.Generic;
using UnityEngine;

public class WorldController : MonoBehaviour
{
    [Header("Настройки чанков")]
    [SerializeField] private GameObject chunkPrefab; // Массив префабов чанков
    [SerializeField] private GameObject crackedChunkPrefab; // Массив расколотых версий чанков

    [SerializeField] private GameObject[] obstaclePrefabs;

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

        // Если нет ни одного чанка — создаём стартовый
        if (activeChunks.Count == 0)
        {
            SpawnChunk(chunkPrefab, chunkSpawnPoint.position.z);
            return;
        }

        // Гарантируем ровную стыковку: новый чанк ставим ровно за последним по оси Z
        GameObject lastChunk = activeChunks[activeChunks.Count - 1];
        if (lastChunk != null && activeChunks.Count < 10)
        {
            float targetZ = lastChunk.transform.position.z + chunkLength;
            // Спауним впереди, если запас закончился (за пределом spawnPoint)
            if (targetZ <= chunkSpawnPoint.position.z)
            {
                SpawnChunk(chunkPrefab, targetZ);
            }
        }
    }


    private void SpawnChunk(GameObject prefab, float z)
    {
        GameObject newChunk = Instantiate(prefab, new Vector3(0, 0, z), Quaternion.identity, chunkSpawnPoint != null ? chunkSpawnPoint.parent : null);
        activeChunks.Add(newChunk);
        activeObjects.Add(newChunk);

        ChunkObstacleSpawner obstacleSpawner = newChunk.GetComponent<ChunkObstacleSpawner>();
        activeObjects.AddRange(obstacleSpawner.SpawnObstacles(chunkSpawnPoint));
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

    private float GetClosestChunkZPosition()
    {
        // Предполагается, что самый первый — ближайший к игроку
        for (int i = 0; i < activeChunks.Count; i++)
        {
            if (activeChunks[i] != null)
            {
                return activeChunks[i].transform.position.z;
            }
        }
        // Если нет валидных чанков — возвращаем большое значение вперёд
        return float.MaxValue;
    }

    private void ReplaceChunk()
    {
        float closestZ = GetClosestChunkZPosition();
        if (closestZ < replaceChunkPoint.position.z)
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
