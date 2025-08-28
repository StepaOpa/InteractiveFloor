using UnityEngine;
using System.Collections.Generic;

public class LevelPlane : MonoBehaviour
{
    [Header("Шаблон для создания")]
    [SerializeField] private GameObject digSpotPrefab; 

    [Header("Список возможных находок")]
    [SerializeField] private List<GameObject> possibleItemPrefabs = new List<GameObject>();

    [Header("Настройки генерации")]
    [SerializeField] private int digSpotsCount = 4;
    [SerializeField] private float minDistanceBetweenSpots = 1.0f; 
    [SerializeField] private float heightOffset = 0.1f; 

    [Header("Границы для размещения")]
    [SerializeField] private float minX = -1.8f;
    [SerializeField] private float maxX = 1.8f;
    [SerializeField] private float minZ = -1.8f;
    [SerializeField] private float maxZ = 1.8f;

    private List<Vector3> spawnedPositions = new List<Vector3>();
    
    public void GenerateItems()
    {
        if (digSpotPrefab == null || possibleItemPrefabs.Count == 0) 
        {
            Debug.LogError("[LevelPlane] Префабы в инспекторе не назначены!");
            return; 
        }

        spawnedPositions.Clear();

        for (int i = 0; i < digSpotsCount; i++)
        {
            Vector3 spawnPosition;
            int attempts = 0; 
            do
            {
                float randomX = Random.Range(minX, maxX);
                float randomZ = Random.Range(minZ, maxZ);
                spawnPosition = new Vector3(randomX, transform.position.y + heightOffset, randomZ);
                
                attempts++;
                if (attempts > 100) { return; }

            } while (!IsPositionValid(spawnPosition));

            spawnedPositions.Add(spawnPosition);
            
            int randomItemIndex = Random.Range(0, possibleItemPrefabs.Count);
            GameObject itemToHide = possibleItemPrefabs[randomItemIndex];

            GameObject newDigSpotObject = Instantiate(digSpotPrefab, spawnPosition, Quaternion.identity, transform);

            DigSpot digSpotScript = newDigSpotObject.GetComponent<DigSpot>();
            if (digSpotScript != null)
            {
                digSpotScript.hiddenItemPrefab = itemToHide;
            }
        }
    }
    
    private bool IsPositionValid(Vector3 position)
    {
        foreach (Vector3 spawnedPos in spawnedPositions)
        {
            if (Vector3.Distance(position, spawnedPos) < minDistanceBetweenSpots)
            {
                return false;
            }
        }
        return true;
    }
}