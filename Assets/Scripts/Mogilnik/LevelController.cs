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

    private bool diggingPhaseComplete = false;
    private bool levelIsComplete = false; 

    void Start()
    {
        InitializeLevel();
    }

    void InitializeLevel()
    {
        diggingPhaseComplete = false;
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
                // --- ВОТ ИСПРАВЛЕННАЯ ЛОГИКА ---
                // 1. СНАЧАЛА генерируем предметы. Внутри этого метода происходит их подсчет.
                levelPlane.GenerateItems();
                
                // 2. ПОТОМ получаем уже готовый результат подсчета.
                int valuableItems = levelPlane.GetTotalItemsCount();
                
                // 3. И ТОЛЬКО ТЕПЕРЬ отправляем правильное число в UI.
                if (UIController.Instance != null)
                {
                    UIController.Instance.SetTotalItemsCount(valuableItems);
                }
                // ------------------------------------
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

    private void CheckLevelCompletion()
    {
        if (activeLevels.Count <= 0 || currentLevelIndex >= activeLevels.Count) return;

        GameObject currentLevelObject = activeLevels[currentLevelIndex];
        if (currentLevelObject == null) return;
        
        if (!diggingPhaseComplete)
        {
            if (currentLevelObject.GetComponentInChildren<DigSpot>() == null)
            {
                diggingPhaseComplete = true;
                Debug.Log("Фаза раскопок завершена! Теперь соберите все ценные предметы.");
            }
        }
        else 
        {
            CollectableItem[] remainingItems = currentLevelObject.GetComponentsInChildren<CollectableItem>();
            
            bool valuableItemsRemain = false;
            foreach (CollectableItem item in remainingItems)
            {
                if (item.itemValue > 0)
                {
                    valuableItemsRemain = true;
                    break;
                }
            }

            if (!valuableItemsRemain)
            {
                levelIsComplete = true;
                Debug.Log("Все ценные предметы собраны! Уровень пройден.");
                CompleteCurrentLevel();
            }
        }
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
        
        DestroyRemainingTrash();

        if (currentLevelIndex < activeLevels.Count && activeLevels[currentLevelIndex] != null)
        {
            Destroy(activeLevels[currentLevelIndex]);
        }
        
        currentLevelIndex++;
        MoveToNextLevel();
    }
    
    private void DestroyRemainingTrash()
    {
        if (activeLevels.Count <= 0 || currentLevelIndex >= activeLevels.Count) return;
        GameObject currentLevelObject = activeLevels[currentLevelIndex];
        if (currentLevelObject == null) return;

        CollectableItem[] remainingItems = currentLevelObject.GetComponentsInChildren<CollectableItem>();
        foreach (CollectableItem item in remainingItems)
        {
            Destroy(item.gameObject);
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