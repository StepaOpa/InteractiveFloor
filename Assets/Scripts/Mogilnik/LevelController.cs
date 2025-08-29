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
    [SerializeField] private GameObject newLevelEffectPrefab;

    private List<GameObject> activeLevels = new List<GameObject>();
    private int currentLevelIndex = 0;
    
    // Флаг levelIsComplete нам все еще нужен, чтобы не запускать логику завершения много раз
    private bool levelIsComplete = false; 
    // А вот флаг diggingPhaseComplete больше не нужен!

    void Start()
    {
        InitializeLevel();
    }

    void InitializeLevel()
    {
        levelIsComplete = false;
        
        if (UIController.Instance != null)
        {
            UIController.Instance.ResetForNewLevel();
        }

        if (currentLevelIndex < levelPlanePrefabs.Count && activeLevels.Count <= currentLevelIndex)
        {
            float currentHeight = startLevelHeight + (currentLevelIndex * levelHeight);
            Vector3 spawnPosition = transform.position + new Vector3(0f, currentHeight, 0f);
            GameObject level = Instantiate(levelPlanePrefabs[currentLevelIndex], spawnPosition, Quaternion.identity, transform);
            level.name = $"Level_{currentLevelIndex + 1}";
            activeLevels.Add(level);

            if (newLevelEffectPrefab != null && currentLevelIndex > 0) 
            {
                Instantiate(newLevelEffectPrefab, spawnPosition, Quaternion.identity);
            }

            LevelPlane levelPlane = level.GetComponent<LevelPlane>();
            if (levelPlane != null)
            {
                levelPlane.GenerateItems();
                int valuableItems = levelPlane.GetTotalItemsCount();
                if (UIController.Instance != null)
                {
                    UIController.Instance.SetTotalItemsCount(valuableItems);
                }
            }
        }
        
        if (levelTimer != null)
        {
            levelTimer.StartTimer();
        }

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

    // --- ПОЛНОСТЬЮ ПЕРЕПИСАННЫЙ МЕТОД ---
    private void CheckLevelCompletion()
    {
        if (activeLevels.Count <= 0 || currentLevelIndex >= activeLevels.Count) return;
        GameObject currentLevelObject = activeLevels[currentLevelIndex];
        if (currentLevelObject == null) return;

        // ШАГ 1: Проверяем предметы, которые уже выкопаны и лежат на земле.
        CollectableItem[] activeItems = currentLevelObject.GetComponentsInChildren<CollectableItem>();
        foreach (CollectableItem item in activeItems)
        {
            // Если находим хотя бы один ценный предмет, значит, играть еще нужно.
            if (item.itemValue > 0)
            {
                return; // Выходим из проверки, уровень не пройден.
            }
        }

        // ШАГ 2: Если мы дошли сюда, значит, среди выкопанных предметов ценных нет.
        // Теперь нужно проверить, не остались ли ценные предметы под землей.
        DigSpot[] remainingDigSpots = currentLevelObject.GetComponentsInChildren<DigSpot>();
        foreach (DigSpot spot in remainingDigSpots)
        {
            if (spot.hiddenItemPrefab != null)
            {
                CollectableItem hiddenItemInfo = spot.hiddenItemPrefab.GetComponent<CollectableItem>();
                // Если находим хотя бы один закопанный ценный предмет...
                if (hiddenItemInfo != null && hiddenItemInfo.itemValue > 0)
                {
                    return; // ...то уровень тоже еще не пройден.
                }
            }
        }

        // ШАГ 3: Если код дошел до этой точки, это значит, что мы не нашли
        // ценных предметов ни на земле, ни под землей. Уровень пройден!
        levelIsComplete = true;
        Debug.Log("Все ценные предметы собраны! Уровень пройден, игнорируем закопанный мусор.");
        CompleteCurrentLevel();
    }
    
    private void CompleteCurrentLevel()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.StopAllSounds();
        }

        if (levelTimer != null)
        {
            levelTimer.StopTimer();
        }
        
        // Теперь этот метод уничтожит и выкопанный мусор, и закопанный
        ClearRemainingLevelObjects();

        if (currentLevelIndex < activeLevels.Count && activeLevels[currentLevelIndex] != null)
        {
            Destroy(activeLevels[currentLevelIndex]);
        }
        
        currentLevelIndex++;
        MoveToNextLevel();
    }
    
    // --- УЛУЧШЕННЫЙ МЕТОД ОЧИСТКИ ---
    private void ClearRemainingLevelObjects()
    {
        if (activeLevels.Count <= 0 || currentLevelIndex >= activeLevels.Count) return;
        GameObject currentLevelObject = activeLevels[currentLevelIndex];
        if (currentLevelObject == null) return;

        // Уничтожаем оставшийся выкопанный мусор
        CollectableItem[] remainingItems = currentLevelObject.GetComponentsInChildren<CollectableItem>();
        foreach (CollectableItem item in remainingItems)
        {
            Destroy(item.gameObject);
        }
        
        // Уничтожаем оставшиеся точки копания (в которых спрятан мусор)
        DigSpot[] remainingSpots = currentLevelObject.GetComponentsInChildren<DigSpot>();
        foreach (DigSpot spot in remainingSpots)
        {
            Destroy(spot.gameObject);
        }
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