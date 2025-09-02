using System.Collections.Generic;
using UnityEngine;

public class WorldController : MonoBehaviour
{
    [Header("Настройки чанков")]
    [SerializeField] private GameObject chunkPrefab;
    [SerializeField] private GameObject crackedChunkPrefab;
    [SerializeField] private int groundWidth = 2;
    [SerializeField] private float chunkLength = 40f;

    [Header("Настройки стен")]
    [Tooltip("Список префабов стен для случайного спавна")]
    [SerializeField] private GameObject[] wallPrefabs; // Массив (список) префабов
    [SerializeField] private float leftWallXPosition = -12f;
    [SerializeField] private float rightWallXPosition = 12f;

    [Header("Общие настройки")]
    [SerializeField] private Transform chunkSpawnPoint;
    [SerializeField] private float chunkSpeed = 10f;
    [SerializeField] private Transform replaceChunkPoint;

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

    /// <summary>
    /// Создает новый участок мира, включая пол и стены.
    /// </summary>
    private void SpawnChunk(GameObject prefab, float z)
    {
        for (int i = -40 * groundWidth; i <= 40 * groundWidth; i += 40)
        {
            GameObject newChunk = Instantiate(prefab, new Vector3(i, 0, z), Quaternion.identity, chunkSpawnPoint != null ? chunkSpawnPoint.parent : null);

            // Логика для центрального чанка (спавн препятствий и стен)
            if (newChunk.transform.position.x == 0)
            {
                activeChunks.Add(newChunk);
                ChunkObstacleSpawner obstacleSpawner = newChunk.GetComponent<ChunkObstacleSpawner>();
                if (obstacleSpawner != null)
                {
                    activeObjects.AddRange(obstacleSpawner.SpawnObstacles(chunkSpawnPoint));
                }

                // Проверяем, есть ли вообще префабы стен в списке
                if (wallPrefabs != null && wallPrefabs.Length > 0)
                {
                    // 1. Выбираем СЛУЧАЙНЫЙ префаб из нашего списка
                    int randomIndex = Random.Range(0, wallPrefabs.Length);
                    GameObject randomWallPrefab = wallPrefabs[randomIndex];

                    // 2. Создаем левую стену, используя случайный префаб
                    Vector3 baseLeftPos = new Vector3(leftWallXPosition, 0, z);
                    Vector3 finalLeftPos = baseLeftPos + randomWallPrefab.transform.position;
                    GameObject leftWall = Instantiate(randomWallPrefab, finalLeftPos, randomWallPrefab.transform.rotation, chunkSpawnPoint.parent);
                    activeObjects.Add(leftWall);

                    // 3. Создаем правую стену из ТОГО ЖЕ случайного префаба
                    Vector3 baseRightPos = new Vector3(rightWallXPosition, 0, z);
                    Vector3 finalRightPos = baseRightPos + randomWallPrefab.transform.position;
                    GameObject rightWall = Instantiate(randomWallPrefab, finalRightPos, randomWallPrefab.transform.rotation, chunkSpawnPoint.parent);
                    activeObjects.Add(rightWall);
                }
            }
            activeObjects.Add(newChunk);
        }
    }

    /// <summary>
    /// Двигает все активные объекты (чанки, стены, препятствия) на игрока.
    /// </summary>
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

    /// <summary>
    /// Проверяет, не пора ли создать новый чанк.
    /// </summary>
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
                SpawnChunk(chunkPrefab, targetZ);
            }
        }
    }

    /// <summary>
    /// Заменяет старый чанк на "расколотую" версию.
    /// </summary>
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

    /// <summary>
    /// Вспомогательный метод для нахождения ближайшего к игроку чанка.
    /// </summary>
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