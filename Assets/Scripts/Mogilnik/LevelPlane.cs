using UnityEngine;

public class LevelPlane : MonoBehaviour
{

    [SerializeField] private System.Collections.Generic.List<GameObject> itemPrefabs;
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 2f;
    [SerializeField] private float minX = -5f;
    [SerializeField] private float maxX = 5f;
    [SerializeField] private float minZ = -5f; 
    [SerializeField] private float maxZ = 5f;
    [SerializeField] private int itemsCount = 10;
    [SerializeField] private float minRotationY = 0f;
    [SerializeField] private float maxRotationY = 360f;
    [SerializeField] private float minRotationX = 0f;
    [SerializeField] private float maxRotationX = 360f;
    [SerializeField] private float absoluteScale = 1000f;
    [SerializeField] private float heightOffset = 0.1f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Генерация предметов теперь не запускается автоматически
        // Она будет вызвана из LevelController после создания всех уровней
    }

    /// Генерирует предметы на уровне. Вызывается из LevelController.
    public void GenerateItems()
    {

        if (itemPrefabs == null || itemPrefabs.Count == 0)
        {
            return;
        }
        int generatedItemsCount = 0;

        for (int i = 0; i < itemsCount; i++)
        {
            // Выбираем случайный предмет из списка
            GameObject prefab = itemPrefabs[Random.Range(0, itemPrefabs.Count)];
            
            if (prefab == null)
            {
                continue;
            }
            
            // Генерируем случайную позицию в пределах плоскости
            float randomX = Random.Range(minX, maxX);
            float randomZ = Random.Range(minZ, maxZ);
            // Используем высоту текущего уровня + небольшое смещение вверх
            Vector3 randomPosition = new Vector3(randomX, transform.position.y + heightOffset, randomZ);
            
            // Создаем объект на сцене
            GameObject item = Instantiate(prefab, randomPosition, Quaternion.identity, transform);
            
            if (item != null)
            {
                // Правильное случайное вращение по обеим осям одновременно
                float randomRotY = Random.Range(minRotationY, maxRotationY);
                float randomRotX = Random.Range(minRotationX, maxRotationX);
                item.transform.rotation = Quaternion.Euler(randomRotX, randomRotY, 0);
                
                // Случайный размер
                float randomScale = Random.Range(minScale, maxScale) * absoluteScale;
                item.transform.localScale = new Vector3(randomScale, randomScale, randomScale);
                
                item.name = $"Item_{generatedItemsCount + 1}";
                generatedItemsCount++;
            }
        }


    }

    // Update is called once per frame
    void Update()
    {
        // Логика удаления уровня теперь находится в LevelController
    }
}
