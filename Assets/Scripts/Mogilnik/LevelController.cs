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
    private bool levelIsComplete = false;

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

            LevelPlane levelPlane = level.GetComponent<LevelPlane>();
            if (levelPlane != null)
            {
                // --- ИЗМЕНЕННЫЙ БЛОК ---
                // Создаем эффект перехода ДО генерации земли, но уже зная, на какой высоте она будет
                if (newLevelEffectPrefab != null && currentLevelIndex > 0)
                {
                    // Вычисляем позицию для эффекта: базовая позиция + высота слоя земли + небольшой запас (0.1f)
                    Vector3 effectPosition = spawnPosition + new Vector3(0, levelPlane.DirtHeightOffset + 0.1f, 0);

                    // Создаем эффект в новой, приподнятой точке
                    Instantiate(newLevelEffectPrefab, effectPosition, Quaternion.identity);
                }
                // --- КОНЕЦ ИЗМЕНЕННОГО БЛОКА ---

                // Теперь генерируем сам уровень
                levelPlane.GenerateLevel();
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

    private void CheckLevelCompletion()
    {
        if (activeLevels.Count <= 0 || currentLevelIndex >= activeLevels.Count) return;
        GameObject currentLevelObject = activeLevels[currentLevelIndex];
        if (currentLevelObject == null) return;

        CollectableItem[] activeItems = currentLevelObject.GetComponentsInChildren<CollectableItem>();
        foreach (CollectableItem item in activeItems)
        {
            if (item.itemValue > 0)
            {
                return;
            }
        }

        DigSpot[] remainingDigSpots = currentLevelObject.GetComponentsInChildren<DigSpot>();
        foreach (DigSpot spot in remainingDigSpots)
        {
            if (spot.hiddenItemPrefab != null)
            {
                CollectableItem hiddenItemInfo = spot.hiddenItemPrefab.GetComponent<CollectableItem>();
                if (hiddenItemInfo != null && hiddenItemInfo.itemValue > 0)
                {
                    return;
                }
            }
        }

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

        ClearRemainingLevelObjects();

        if (currentLevelIndex < activeLevels.Count && activeLevels[currentLevelIndex] != null)
        {
            Destroy(activeLevels[currentLevelIndex]);
        }

        currentLevelIndex++;
        MoveToNextLevel();
    }

    private void ClearRemainingLevelObjects()
    {
        if (activeLevels.Count <= 0 || currentLevelIndex >= activeLevels.Count) return;
        GameObject currentLevelObject = activeLevels[currentLevelIndex];
        if (currentLevelObject == null) return;

        CollectableItem[] remainingItems = currentLevelObject.GetComponentsInChildren<CollectableItem>();
        foreach (CollectableItem item in remainingItems)
        {
            Destroy(item.gameObject);
        }

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

    public int GetTotalLevelCount()
    {
        return levelPlanePrefabs.Count;
    }

    private void OnAllLevelsCompleted()
    {
        GameManager.Instance.ShowWinScreen(
            UIController.Instance.GetCurrentScore(),
            levelPlanePrefabs.Count
        );
    }
}