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
    [SerializeField] private GameObject wallPrefab;
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

    private void SpawnChunk(GameObject prefab, float z)
    {
        for (int i = -40 * groundWidth; i <= 40 * groundWidth; i += 40)
        {
            GameObject newChunk = Instantiate(prefab, new Vector3(i, 0, z), Quaternion.identity, chunkSpawnPoint != null ? chunkSpawnPoint.parent : null);

            if (newChunk.transform.position.x == 0)
            {
                activeChunks.Add(newChunk);
                ChunkObstacleSpawner obstacleSpawner = newChunk.GetComponent<ChunkObstacleSpawner>();
                if (obstacleSpawner != null)
                {
                    activeObjects.AddRange(obstacleSpawner.SpawnObstacles(chunkSpawnPoint));
                }

                if (wallPrefab != null)
                {
                    // ========== НАЧАЛО ИЗМЕНЕНИЙ ==========

                    // Создаем левую стену
                    // 1. Рассчитываем базовую позицию (где должен быть центр стены в мире)
                    Vector3 baseLeftPos = new Vector3(leftWallXPosition, 0, z);
                    // 2. Добавляем к ней смещение, сохраненное в префабе (особенно важно для высоты Y)
                    Vector3 finalLeftPos = baseLeftPos + wallPrefab.transform.position;
                    // 3. Создаем стену в финальной позиции с поворотом из префаба
                    GameObject leftWall = Instantiate(wallPrefab, finalLeftPos, wallPrefab.transform.rotation, chunkSpawnPoint.parent);
                    activeObjects.Add(leftWall);

                    // Создаем правую стену
                    Vector3 baseRightPos = new Vector3(rightWallXPosition, 0, z);
                    Vector3 finalRightPos = baseRightPos + wallPrefab.transform.position;
                    GameObject rightWall = Instantiate(wallPrefab, finalRightPos, wallPrefab.transform.rotation, chunkSpawnPoint.parent);
                    activeObjects.Add(rightWall);

                    // ========== КОНЕЦ ИЗМЕНЕНИЙ ==========
                }
            }
            activeObjects.Add(newChunk);
        }
    }

    // Остальные методы остаются без изменений, просто скопируй все для уверенности
    private void CheckNumberOfChunks() { for (int i = activeChunks.Count - 1; i >= 0; i--) { if (activeChunks[i] == null) { activeChunks.RemoveAt(i); } } if (activeChunks.Count == 0) { SpawnChunk(chunkPrefab, chunkSpawnPoint.position.z); return; } GameObject lastChunk = activeChunks[activeChunks.Count - 1]; if (lastChunk != null && activeChunks.Count < 10) { float targetZ = lastChunk.transform.position.z + chunkLength; if (targetZ <= chunkSpawnPoint.position.z) { SpawnChunk(chunkPrefab, targetZ); } } }
    private void MoveChunks() { for (int i = activeObjects.Count - 1; i >= 0; i--) { GameObject obj = activeObjects[i]; if (obj == null) { activeObjects.RemoveAt(i); continue; } Transform t = obj.transform; if (t == null) { activeObjects.RemoveAt(i); continue; } Vector3 p = t.position; p.z -= chunkSpeed * Time.deltaTime; t.position = p; } }
    private Vector3 GetClosestChunkPosition() { for (int i = 0; i < activeChunks.Count; i++) { if (activeChunks[i] != null) { return activeChunks[i].transform.position; } } return new Vector3(0, 0, float.MaxValue); }
    private void ReplaceChunk() { Vector3 closestPosition = GetClosestChunkPosition(); if (closestPosition.z < replaceChunkPoint.position.z && closestPosition.x == 0 && closestPosition.y == 0) { int firstValidIndex = -1; for (int i = 0; i < activeChunks.Count; i++) { if (activeChunks[i] != null) { firstValidIndex = i; break; } } if (firstValidIndex == -1) { return; } GameObject chunkToReplace = activeChunks[firstValidIndex]; Vector3 pos = chunkToReplace.transform.position; Quaternion rot = chunkToReplace.transform.rotation; Transform parent = chunkToReplace.transform.parent; GameObject newChunk = Instantiate(crackedChunkPrefab, pos, rot, parent); if (chunkToReplace != null) { Destroy(chunkToReplace); } activeChunks.RemoveAt(firstValidIndex); int idxInActiveObjects = activeObjects.IndexOf(chunkToReplace); if (idxInActiveObjects >= 0) { activeObjects.RemoveAt(idxInActiveObjects); } activeObjects.Add(newChunk); } }
}