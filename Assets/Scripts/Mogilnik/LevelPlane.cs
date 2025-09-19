using UnityEngine;
using System.Collections.Generic;


public class LevelPlane : MonoBehaviour
{
    [Header("Шаблоны для создания")]
    [Tooltip("Список префабов кучек земли. При генерации будет выбран случайный из этого списка.")]
    [SerializeField] private List<GameObject> dirtPilePrefabs = new List<GameObject>();

    [Header("Список возможных находок")]
    [SerializeField] private List<GameObject> possibleItemPrefabs = new List<GameObject>();

    [Header("Настройки генерации предметов")]
    [SerializeField] private int itemsToHideCount = 4;
    [SerializeField] private float minDistanceBetweenItems = 1.0f;
    [SerializeField] private float itemHeightOffset = 0.0f;

    [Header("Настройки генерации земли")]
    [Tooltip("Как плотно будут стоять кучки. Чем меньше значение, тем плотнее.")]
    [SerializeField] private float gridSpacing = 0.25f;
    [Tooltip("На какой высоте над плоскостью появится земля.")]
    [SerializeField] private float dirtHeightOffset = 0.1f;

    [Header("Границы для размещения")]
    [SerializeField] private float minX = -1.8f;
    [SerializeField] private float maxX = 1.8f;
    [SerializeField] private float minZ = -1.8f;
    [SerializeField] private float maxZ = 1.8f;

    private List<Vector3> spawnedItemPositions = new List<Vector3>();
    private int valuableItemsCount = 0;

    // --- ДОБАВЛЕНО ---
    // Публичное свойство, чтобы другие скрипты могли безопасно узнать высоту слоя земли
    public float DirtHeightOffset => dirtHeightOffset;

    public int GetTotalItemsCount()
    {
        return valuableItemsCount;
    }

    // Главный метод, который запускает создание и предметов, и земли
    public void GenerateLevel()
    {
        // Проверяем, что все необходимые префабы назначены в инспекторе
        if (dirtPilePrefabs == null || dirtPilePrefabs.Count == 0 || possibleItemPrefabs == null || possibleItemPrefabs.Count == 0)
        {
            Debug.LogError("[LevelPlane] Список префабов кучек земли или находок пуст! Назначьте их в инспекторе.");
            return;
        }

        // Сначала размещаем спрятанные предметы
        PlaceHiddenItems();
        // Затем покрываем все слоем земли
        GenerateDirtGrid();
    }

    // Метод для расстановки спрятанных предметов
    private void PlaceHiddenItems()
    {
        spawnedItemPositions.Clear();
        valuableItemsCount = 0;

        for (int i = 0; i < itemsToHideCount; i++)
        {
            Vector3 spawnPosition;
            int attempts = 0;
            do
            {
                float randomX = Random.Range(minX, maxX);
                float randomZ = Random.Range(minZ, maxZ);
                spawnPosition = new Vector3(randomX, transform.position.y + itemHeightOffset, randomZ);

                attempts++;
                if (attempts > 100)
                {
                    Debug.LogWarning("[LevelPlane] Не удалось найти подходящее место для предмета после 100 попыток. Прерываю размещение.");
                    return;
                }
            } while (!IsPositionValid(spawnPosition));

            spawnedItemPositions.Add(spawnPosition);

            int randomItemIndex = Random.Range(0, possibleItemPrefabs.Count);
            GameObject itemToHidePrefab = possibleItemPrefabs[randomItemIndex];

            // Создаем сам предмет на сцене
            Instantiate(itemToHidePrefab, spawnPosition, Quaternion.identity, transform);

            // Подсчитываем ценные предметы для UI
            CollectableItem itemInfo = itemToHidePrefab.GetComponent<CollectableItem>();
            if (itemInfo != null && itemInfo.itemValue > 0)
            {
                valuableItemsCount++;
            }
        }

        Debug.Log($"[LevelPlane] Размещение предметов завершено. Ценных предметов: {valuableItemsCount}");
    }

    // Метод для создания сетки из кучек земли
    private void GenerateDirtGrid()
    {
        for (float x = minX; x <= maxX; x += gridSpacing)
        {
            for (float z = minZ; z <= maxZ; z += gridSpacing)
            {
                int randomIndex = Random.Range(0, dirtPilePrefabs.Count);
                GameObject randomDirtPrefab = dirtPilePrefabs[randomIndex];

                if (randomDirtPrefab == null)
                {
                    Debug.LogWarning($"[LevelPlane] В списке 'Dirt Pile Prefabs' есть пустой элемент с индексом {randomIndex}. Пропускаю его.");
                    continue;
                }

                Vector3 dirtPosition = new Vector3(x, transform.position.y + dirtHeightOffset, z);
                Instantiate(randomDirtPrefab, dirtPosition, Quaternion.identity, transform);
            }
        }

        Debug.Log("[LevelPlane] Сетка из случайных кучек земли успешно создана.");
    }

    // Вспомогательный метод для проверки, не слишком ли близко новый предмет к уже существующим
    private bool IsPositionValid(Vector3 position)
    {
        foreach (Vector3 spawnedPos in spawnedItemPositions)
        {
            if (Vector3.Distance(position, spawnedPos) < minDistanceBetweenItems)
            {
                return false;
            }
        }
        return true;
    }
}