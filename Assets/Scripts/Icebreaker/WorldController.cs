using System.Collections.Generic;
using UnityEngine;

public class WorldController : MonoBehaviour
{
    // ... (все ваши поля остаются без изменений) ...
    [Header("Настройки чанков")]
    [SerializeField] private GameObject chunkPrefab;
    [SerializeField] private GameObject crackedChunkPrefab;
    [SerializeField] private int groundWidth = 2;
    [SerializeField] private float chunkLength = 40f;

    [Header("Настройки стен")]
    [Tooltip("Список префабов стен для случайного спавна")]
    [SerializeField] private GameObject[] wallPrefabs;
    [SerializeField] private float leftWallXPosition = -12f;
    [SerializeField] private float rightWallXPosition = 12f;

    [Header("Общие настройки")]
    [SerializeField] private Transform chunkSpawnPoint;
    [SerializeField] private float chunkSpeed = 10f;
    [SerializeField] private Transform replaceChunkPoint;

    [Header("Настройки стартовой зоны")]
    [Tooltip("Сколько чанков сгенерировать на старте, чтобы не было пустоты.")]
    [SerializeField] private int initialChunkCount = 5;
    [Tooltip("Сколько из этих стартовых чанков будут без препятствий.")]
    [SerializeField] private int safeZoneChunks = 2;

    private List<GameObject> activeChunks = new List<GameObject>();
    private List<GameObject> activeObjects = new List<GameObject>();

    private void Start()
    {
        GenerateInitialWorld();
    }

    // ... (Update и другие методы остаются без изменений) ...

    // ИЗМЕНЕННЫЙ МЕТОД ДЛЯ ГЕНЕРАЦИИ СТАРТОВОЙ ЛОКАЦИИ
    private void GenerateInitialWorld()
    {
        for (int i = 0; i < initialChunkCount; i++)
        {
            // === ВОТ ИЗМЕНЕНИЕ ===
            // Вместо далекой точки chunkSpawnPoint.position.z, мы начинаем отсчет от позиции
            // самого этого объекта (WorldController), которая должна быть в центре мира (Z=0).
            float spawnZ = transform.position.z + (i * chunkLength);

            bool spawnObstacles = i >= safeZoneChunks;
            SpawnChunk(chunkPrefab, spawnZ, spawnObstacles);
        }
    }

    // ... (остальной код остается точно таким же, как в прошлый раз) ...

    private void Update()
    {
        MoveChunks();
        CheckNumberOfChunks();
        ReplaceChunk();
    }

    private void SpawnChunk(GameObject prefab, float z, bool shouldSpawnObstacles = true)
    {
        for (int i = -40 * groundWidth; i <= 40 * groundWidth; i += 40)
        {
            GameObject newChunk = Instantiate(prefab, new Vector3(i, 0, z), Quaternion.identity, chunkSpawnPoint != null ? chunkSpawnPoint.parent : null);

            if (newChunk.transform.position.x == 0)
            {
                activeChunks.Add(newChunk);

                if (shouldSpawnObstacles)
                {
                    ChunkObstacleSpawner obstacleSpawner = newChunk.GetComponent<ChunkObstacleSpawner>();
                    if (obstacleSpawner != null)
                    {
                        activeObjects.AddRange(obstacleSpawner.SpawnObstacles(chunkSpawnPoint));
                    }
                }

                if (wallPrefabs != null && wallPrefabs.Length > 0)
                {
                    int randomIndex = UnityEngine.Random.Range(0, wallPrefabs.Length);
                    GameObject randomWallPrefab = wallPrefabs[randomIndex];

                    Vector3 baseLeftPos = new Vector3(leftWallXPosition, 0, z);
                    Vector3 finalLeftPos = baseLeftPos + randomWallPrefab.transform.position;
                    GameObject leftWall = Instantiate(randomWallPrefab, finalLeftPos, randomWallPrefab.transform.rotation, chunkSpawnPoint.parent);
                    activeObjects.Add(leftWall);

                    Vector3 baseRightPos = new Vector3(rightWallXPosition, 0, z);
                    Vector3 finalRightPos = baseRightPos + randomWallPrefab.transform.position;
                    GameObject rightWall = Instantiate(randomWallPrefab, finalRightPos, randomWallPrefab.transform.rotation, chunkSpawnPoint.parent);
                    activeObjects.Add(rightWall);
                }
            }
            activeObjects.Add(newChunk);
        }
    }

    private void MoveChunks()
    {
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

            Vector3 p = t.position;
            p.z -= chunkSpeed * Time.deltaTime;
            t.position = p;
        }
    }

    private void CheckNumberOfChunks()
    {
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
                SpawnChunk(chunkPrefab, targetZ, true);
            }
        }
    }

    private void ReplaceChunk()
    {
        Vector3 closestPosition = GetClosestChunkPosition();
        if (closestPosition.z < replaceChunkPoint.position.z && closestPosition.x == 0 && closestPosition.y == 0)
        {
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
            Vector3 pos = chunkToReplace.transform.position;
            Quaternion rot = chunkToReplace.transform.rotation;
            Transform parent = chunkToReplace.transform.parent;
            GameObject newChunk = Instantiate(crackedChunkPrefab, pos, rot, parent);

            if (chunkToReplace != null)
            {
                Destroy(chunkToReplace);
            }

            activeChunks.RemoveAt(firstValidIndex);

            int idxInActiveObjects = activeObjects.IndexOf(chunkToReplace);
            if (idxInActiveObjects >= 0)
            {
                activeObjects.RemoveAt(idxInActiveObjects);
            }

            activeObjects.Add(newChunk);
        }
    }

    private Vector3 GetClosestChunkPosition()
    {
        for (int i = 0; i < activeChunks.Count; i++)
        {
            if (activeChunks[i] != null)
            {
                return activeChunks[i].transform.position;
            }
        }
        return new Vector3(0, 0, float.MaxValue);
    }
}