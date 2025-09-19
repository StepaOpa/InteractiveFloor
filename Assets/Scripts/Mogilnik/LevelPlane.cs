using UnityEngine;
using System.Collections.Generic;


public class LevelPlane : MonoBehaviour
{
    // --- ИЗМЕНЕНО: DigSpot больше не нужен, теперь нам нужен префаб кучки ---
    [Header("Шаблоны для создания")]
    [SerializeField] private GameObject dirtPilePrefab;

    [Header("Список возможных находок")]
    [SerializeField] private List<GameObject> possibleItemPrefabs = new List<GameObject>();

    [Header("Настройки генерации предметов")]
    [SerializeField] private int itemsToHideCount = 4; // Переименовал для ясности
    [SerializeField] private float minDistanceBetweenItems = 1.0f;
    [SerializeField] private float itemHeightOffset = 0.0f; // Предметы лежат прямо на плоскости

    // --- НОВОЕ: Настройки для сетки земли ---
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

    public int GetTotalItemsCount()
    {
        return valuableItemsCount;
    }

    // Этот метод теперь называется GenerateLevel, так как он делает и предметы, и землю
    public void GenerateLevel()
    {
        // --- ИЗМЕНЕНО: Проверяем новый префаб ---
        if (dirtPilePrefab == null || possibleItemPrefabs.Count == 0)
        {
            Debug.LogError("[LevelPlane] Префаб кучки земли (Dirt Pile) или префабы находок не назначены!");
            return;
        }

        // Сначала размещаем спрятанные предметы
        PlaceHiddenItems();
        // Затем покрываем все слоем земли
        GenerateDirtGrid();
    }

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
                // --- ИЗМЕНЕНО: Используем новый offset для предметов ---
                spawnPosition = new Vector3(randomX, transform.position.y + itemHeightOffset, randomZ);

                attempts++;
                if (attempts > 100) { return; }

            } while (!IsPositionValid(spawnPosition));

            spawnedItemPositions.Add(spawnPosition);

            int randomItemIndex = Random.Range(0, possibleItemPrefabs.Count);
            GameObject itemToHidePrefab = possibleItemPrefabs[randomItemIndex];

            // --- ВАЖНОЕ ИЗМЕНЕНИЕ ---
            // Мы больше не создаем DigSpot. Мы создаем сам предмет!
            // Он будет лежать в мире и ждать, пока его "откопают".
            Instantiate(itemToHidePrefab, spawnPosition, Quaternion.identity, transform);

            // Логика подсчета ценных предметов остается прежней
            CollectableItem itemInfo = itemToHidePrefab.GetComponent<CollectableItem>();
            if (itemInfo != null && itemInfo.itemValue > 0)
            {
                valuableItemsCount++;
            }
        }

        Debug.Log($"[LevelPlane] Размещение предметов завершено. Ценных предметов: {valuableItemsCount}");
    }

    // --- НОВЫЙ МЕТОД ---
    private void GenerateDirtGrid()
    {
        // Проходим по всей площадке с заданным шагом (gridSpacing)
        for (float x = minX; x <= maxX; x += gridSpacing)
        {
            for (float z = minZ; z <= maxZ; z += gridSpacing)
            {
                // Вычисляем позицию для каждой кучки
                Vector3 dirtPosition = new Vector3(x, transform.position.y + dirtHeightOffset, z);

                // Создаем экземпляр кучки
                Instantiate(dirtPilePrefab, dirtPosition, Quaternion.identity, transform);
            }
        }
        Debug.Log("[LevelPlane] Сетка из кучек земли успешно создана.");
    }

    // Этот метод теперь проверяет позиции для предметов
    private bool IsPositionValid(Vector3 position)
    {
        foreach (Vector3 spawnedPos in spawnedItemPositions)
        {
            // --- ИЗМЕНЕНО: Используем новую переменную дистанции ---
            if (Vector3.Distance(position, spawnedPos) < minDistanceBetweenItems)
            {
                return false;
            }
        }
        return true;
    }

    // --- НЕ ЗАБУДЬ ИЗМЕНИТЬ ВЫЗОВ В LevelController! ---
    // В скрипте LevelController найди строку `levelPlane.GenerateItems();`
    // и замени ее на `levelPlane.GenerateLevel();`
}