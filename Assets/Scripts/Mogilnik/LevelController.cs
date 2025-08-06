using UnityEngine;

public class LevelController : MonoBehaviour
{

    [Header("Настройки уровней")]
    [SerializeField] private System.Collections.Generic.List<GameObject> levelPlanePrefabs = new System.Collections.Generic.List<GameObject>(); // Префабы уровней-плоскостей
    [SerializeField] private float levelHeight = 2f;
    [SerializeField] private float startLevelHeight = -0.1f;
    
    // Отслеживание активных уровней
    private System.Collections.Generic.List<GameObject> activeLevels = new System.Collections.Generic.List<GameObject>();
    private int currentLevelIndex = 0;
    // Высота между уровнями
    void Start()
    {
        
        // Создаем уровни последовательно друг над другом
        float currentHeight = startLevelHeight;
        int createdLevelsCount = 0;
        int nullPrefabsCount = 0;
        
        for (int i = 0; i < levelPlanePrefabs.Count; i++)
        {
            if (levelPlanePrefabs[i] != null)
            {
                // Создаем новый уровень как дочерний объект
                Vector3 spawnPosition = transform.position + new Vector3(0f, currentHeight, 0f);
                GameObject level = Instantiate(levelPlanePrefabs[i], spawnPosition, Quaternion.identity, transform);
                level.name = $"Level_{i + 1}";

                // Проверяем, что уровень действительно создался
                if (level != null)
                {
                    createdLevelsCount++;
                    
                    // Добавляем уровень в список активных
                    activeLevels.Add(level);
                    
                    // Проверяем компонент LevelPlane
                    LevelPlane levelPlane = level.GetComponent<LevelPlane>();

                }

                currentHeight += levelHeight; // Фиксированное расстояние между уровнями
            }
            else
            {
                nullPrefabsCount++;
            }
        }

        // Итоговая проверка
        
        if (createdLevelsCount == levelPlanePrefabs.Count - nullPrefabsCount)
        {
            
            // Теперь запускаем генерацию предметов на всех уровнях
            StartItemGeneration();
        }

     }

    /// Запускает генерацию предметов на всех созданных уровнях
    private void StartItemGeneration()
    {
        int levelPlane.
        int levelPlanesFound = 0;
        int itemGenerationStarted = 0;
        
        // Проходим по всем дочерним объектам (созданным уровням)
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform levelTransform = transform.GetChild(i);
            LevelPlane levelPlane = levelTransform.GetComponent<LevelPlane>();
            
            if (levelPlane != null)
            {
                levelPlanesFound++;
                
                try
                {
                    levelPlane.GenerateItems();
                    itemGenerationStarted++;
                }
                catch (System.Exception e)
                {
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        CheckLevelCompletion();
    }
    /// Проверяет завершение текущего уровня и удаляет его при необходимости
    private void CheckLevelCompletion()
    {
        // Проверяем только если есть активные уровни
        if (activeLevels.Count > 0 && currentLevelIndex < activeLevels.Count)
        {
            GameObject currentLevel = activeLevels[currentLevelIndex];
            
            // Проверяем, существует ли еще уровень
            if (currentLevel == null)
            {
                // Уровень уже удален, переходим к следующему
                MoveToNextLevel();
                return;
            }
            
            // Проверяем, собраны ли все предметы на уровне
            if (IsLevelCompleted(currentLevel))
            {
                CompleteCurrentLevel();
            }
        }
    }
    
    /// <summary>
    /// Проверяет, завершен ли уровень (собраны все предметы)
    /// </summary>
    /// <param name="level">Объект уровня для проверки</param>
    /// <returns>True если уровень завершен</returns>
    private bool IsLevelCompleted(GameObject level)
    {
        if (level == null) return true;
        
        // Считаем количество CollectableItem среди дочерних объектов
        int collectableItemsCount = 0;
        for (int i = 0; i < level.transform.childCount; i++)
        {
            Transform child = level.transform.GetChild(i);
            if (child.GetComponent<CollectableItem>() != null)
            {
                collectableItemsCount++;
            }
        }
        
        return collectableItemsCount == 0;
    }
    
    /// <summary>
    /// Завершает текущий уровень и переходит к следующему
    /// </summary>
    private void CompleteCurrentLevel()
    {
        if (currentLevelIndex < activeLevels.Count)
        {
            GameObject completedLevel = activeLevels[currentLevelIndex];
            
            if (completedLevel != null)
            {
                // Удаляем завершенный уровень
                Destroy(completedLevel);
            }
            
            // Переходим к следующему уровню
            MoveToNextLevel();
        }
    }
    
    /// <summary>
    /// Переходит к следующему уровню
    /// </summary>
    private void MoveToNextLevel()
    {
        currentLevelIndex++;
        
        // Обновляем UI с новым номером уровня
        if (UIController.Instance != null)
        {
            UIController.Instance.SetCurrentLevel(currentLevelIndex + 1);
        }
        
        // Проверяем, есть ли еще уровни
        if (currentLevelIndex >= activeLevels.Count)
        {
            // Все уровни пройдены
            OnAllLevelsCompleted();
        }
    }
    
    /// Вызывается когда все уровни пройдены
    private void OnAllLevelsCompleted()
    {
        // Здесь можно добавить логику завершения игры
        // Например, показать экран победы, сохранить результат и т.д.
    }
}
