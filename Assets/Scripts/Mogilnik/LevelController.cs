using UnityEngine;
using System.Collections.Generic;

public class LevelController : MonoBehaviour
{
    [Header("Настройки уровней")]
    [SerializeField] private List<GameObject> levelPlanePrefabs = new List<GameObject>();
    [SerializeField] private float levelHeight = 2f;
    [SerializeField] private float startLevelHeight = -0.1f;
    
    [Header("Компоненты игры")]
    [SerializeField] private LevelTimer levelTimer;
    [SerializeField] private GameObject newLevelEffectPrefab; // <-- ВОТ НОВОЕ ПОЛЕ ДЛЯ ЭФФЕКТА

    private List<GameObject> activeLevels = new List<GameObject>();
    private int currentLevelIndex = 0;

    // --- ФЛАГИ ДЛЯ УПРАВЛЕНИЯ ФАЗАМИ ---
    private bool diggingPhaseComplete = false;
    private bool levelIsComplete = false; 

    void Start()
    {
        InitializeLevel();
    }

    // Метод для инициализации уровня (используется в Start и при переходе на новый)
    void InitializeLevel()
    {
        diggingPhaseComplete = false;
        levelIsComplete = false;

        // Если мы переходим на новый уровень, а не стартуем игру,
        // нужно создать новый объект уровня
        if (currentLevelIndex < levelPlanePrefabs.Count && activeLevels.Count <= currentLevelIndex)
        {
            float currentHeight = startLevelHeight + (currentLevelIndex * levelHeight);
            Vector3 spawnPosition = transform.position + new Vector3(0f, currentHeight, 0f);
            GameObject level = Instantiate(levelPlanePrefabs[currentLevelIndex], spawnPosition, Quaternion.identity, transform);
            level.name = $"Level_{currentLevelIndex + 1}";
            activeLevels.Add(level);

            // --- ИЗМЕНЕНИЕ: ЗАПУСКАЕМ ЭФФЕКТ ПЕРЕХОДА ---
            // Проверяем, что префаб назначен и что это не самый первый уровень
            if (newLevelEffectPrefab != null && currentLevelIndex > 0) 
            {
                // Создаем эффект в центре нового уровня
                Instantiate(newLevelEffectPrefab, spawnPosition, Quaternion.identity);
            }
            // ---------------------------------------------

            // Генерируем DigSpot'ы для нового уровня
            LevelPlane levelPlane = level.GetComponent<LevelPlane>();
            if (levelPlane != null)
            {
                // -- КЛЮЧЕВОЕ ИЗМЕНЕНИЕ --
                // 1. Получаем общее количество предметов с уровня
                int totalItems = levelPlane.GetTotalItemsCount();
                
                // 2. Сразу же отправляем это число в UI
                if (UIController.Instance != null)
                {
                    UIController.Instance.SetTotalItemsCount(totalItems);
                }

                // 3. Запускаем генерацию, как и раньше
                levelPlane.GenerateItems();
            }
        }
        
        if (levelTimer != null)
        {
            levelTimer.StartTimer();
        }

        // Звук играем только если это не первый уровень
        if (currentLevelIndex > 0 && SoundManager.Instance != null)
        {
            SoundManager.Instance.PlayNewLevelSound();
        }
    }

    void Update()
    {
        if (!levelIsComplete)
        {
            CheckLevelCompletion();
        }
    }

    private void CheckLevelCompletion()
    {
        if (activeLevels.Count <= 0 || currentLevelIndex >= activeLevels.Count) return;

        GameObject currentLevelObject = activeLevels[currentLevelIndex];
        if (currentLevelObject == null) return;
        
        // --- ИСПРАВЛЕННАЯ ЛОГИКА ПОИСКА ---
        // Теперь мы будем искать предметы и зоны раскопок только внутри текущего уровня.

        // --- ФАЗА 1: РАСКОПКИ ---
        if (!diggingPhaseComplete)
        {
            // GetComponentInChildren найдет компонент в себе или в любом из своих детей
            if (currentLevelObject.GetComponentInChildren<DigSpot>() == null)
            {
                // Если ни одного DigSpot'а не найдено, значит, фаза раскопок завершена
                diggingPhaseComplete = true;
                Debug.Log("Фаза раскопок завершена! Теперь соберите все предметы.");
            }
        }
        // --- ФАЗА 2: СБОР ---
        else 
        {
            // Точно так же ищем CollectableItem
            if (currentLevelObject.GetComponentInChildren<CollectableItem>() == null)
            {
                // Если ни одного предмета не найдено, уровень пройден!
                levelIsComplete = true;
                Debug.Log("Все предметы собраны! Уровень пройден.");
                CompleteCurrentLevel();
            }
        }
    }
    
    private void CompleteCurrentLevel()
    {
        if (levelTimer != null)
        {
            levelTimer.StopTimer();
        }

        // Уничтожаем объект пройденного уровня
        if (currentLevelIndex < activeLevels.Count && activeLevels[currentLevelIndex] != null)
        {
            Destroy(activeLevels[currentLevelIndex]);
        }
        
        currentLevelIndex++;
        MoveToNextLevel();
    }
    
    private void MoveToNextLevel()
    {
        if (UIController.Instance != null)
        {
            UIController.Instance.SetCurrentLevel(currentLevelIndex + 1);
        }
        
        if (currentLevelIndex >= levelPlanePrefabs.Count)
        {
            OnAllLevelsCompleted();
        }
        else
        {
            // Инициализируем следующий уровень
            InitializeLevel();
        }
    }
    
    private void OnAllLevelsCompleted()
    {
        GameManager.Instance.ShowWinScreen(
            UIController.Instance.GetCurrentScore(),
            levelPlanePrefabs.Count
        );
    }
}