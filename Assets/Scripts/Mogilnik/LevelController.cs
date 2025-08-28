using UnityEngine;

public class LevelController : MonoBehaviour
{
    [Header("Настройки уровней")]
    [SerializeField] private System.Collections.Generic.List<GameObject> levelPlanePrefabs = new System.Collections.Generic.List<GameObject>();
    [SerializeField] private float levelHeight = 2f;
    [SerializeField] private float startLevelHeight = -0.1f;
    
    [Header("Компоненты игры")]
    [SerializeField] private LevelTimer levelTimer;

    private System.Collections.Generic.List<GameObject> activeLevels = new System.Collections.Generic.List<GameObject>();
    private int currentLevelIndex = 0;

    void Start()
    {
        float currentHeight = startLevelHeight;
        for (int i = 0; i < levelPlanePrefabs.Count; i++)
        {
            if (levelPlanePrefabs[i] != null)
            {
                Vector3 spawnPosition = transform.position + new Vector3(0f, currentHeight, 0f);
                GameObject level = Instantiate(levelPlanePrefabs[i], spawnPosition, Quaternion.identity, transform);
                level.name = $"Level_{i + 1}";
                activeLevels.Add(level);
                currentHeight += levelHeight;
            }
        }
        
        StartItemGeneration();
        
        if (levelTimer != null)
        {
            levelTimer.StartTimer();
        }
        else
        {
            Debug.LogError("[LevelController] Ссылка на LevelTimer не установлена в инспекторе!");
        }

        // <-- ЗВУК НАЧАЛА ПЕРВОГО УРОВНЯ
        if (SoundManager.Instance != null) SoundManager.Instance.PlayNewLevelSound();
    }

    private void StartItemGeneration()
    {
        foreach (var level in activeLevels)
        {
            if (level != null)
            {
                LevelPlane levelPlane = level.GetComponent<LevelPlane>();
                if (levelPlane != null)
                {
                    levelPlane.GenerateItems();
                }
            }
        }
    }

    void Update()
    {
        CheckLevelCompletion();
    }

    private void CheckLevelCompletion()
    {
        if (activeLevels.Count > 0 && currentLevelIndex < activeLevels.Count)
        {
            GameObject currentLevel = activeLevels[currentLevelIndex];
            if (currentLevel == null)
            {
                return;
            }
            
            int itemsLeft = 0;
            foreach (Transform child in currentLevel.transform)
            {
                if (child.GetComponent<CollectableItem>() != null)
                {
                    itemsLeft++;
                }
            }

            if (itemsLeft == 0)
            {
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

        if (currentLevelIndex < activeLevels.Count)
        {
            GameObject completedLevel = activeLevels[currentLevelIndex];
            if (completedLevel != null)
            {
                Destroy(completedLevel);
            }
            
            currentLevelIndex++;
            MoveToNextLevel();
        }
    }
    
    private void MoveToNextLevel()
    {
        if (UIController.Instance != null)
        {
            UIController.Instance.SetCurrentLevel(currentLevelIndex + 1);
        }
        
        if (currentLevelIndex >= activeLevels.Count)
        {
            OnAllLevelsCompleted();
        }
        else
        {
            if (levelTimer != null)
            {
                levelTimer.StartTimer();
            }
            // <-- ЗВУК НАЧАЛА СЛЕДУЮЩЕГО УРОВНЯ
            if (SoundManager.Instance != null) SoundManager.Instance.PlayNewLevelSound();
        }
    }
    
    private void OnAllLevelsCompleted()
    {
        Debug.Log("ВСЕ УРОВНИ ПРОЙДЕНЫ!");
        if (levelTimer != null)
        {
            levelTimer.StopTimer();
        }
        
        if (InspectionUI.Instance != null)
        {
            InspectionUI.Instance.ForceHide();
        }
        
        GameManager.Instance.ShowWinScreen(
            UIController.Instance.GetCurrentScore(),
            levelPlanePrefabs.Count
        );
    }
}