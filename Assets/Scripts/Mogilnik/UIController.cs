using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UIController : MonoBehaviour
{
    [Header("UI Элементы")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI currentLevelText;
    [SerializeField] private TextMeshProUGUI inventoryCountText;
    [SerializeField] private Transform inventoryGridParent; 

    [Header("Префабы для инвентаря")]
    [SerializeField] private GameObject inventoryItemPrefab; 
    [SerializeField] private GameObject itemBackgroundPrefab; 

    [Header("Настройки")]
    [SerializeField] private string scorePrefix = "Очки: ";
    [SerializeField] private string levelPrefix = "Уровень: ";
    [SerializeField] private string inventoryPrefix = "Предметов: ";

    private int currentScore = 0;
    private int currentLevel = 1;
    private List<InventoryItem> inventoryItems = new List<InventoryItem>();
    
    private int totalValuableItems = 0; // Общее кол-во ЦЕННЫХ предметов на уровне
    private int collectedOnLevel = 0; // Собрано ЦЕННЫХ предметов на уровне

    public static UIController Instance { get; private set; }

    [System.Serializable]
    public class InventoryItem
    {
        public string itemName;
        public Sprite itemIcon;
        public int itemValue;
        public GameObject uiElement;

        public InventoryItem(string name, Sprite icon, int value)
        {
            itemName = name;
            itemIcon = icon;
            itemValue = value;
        }
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        InitializeUI();
        UpdateAllUI();
    }

    private void InitializeUI()
    {
        if (scoreText == null) Debug.LogWarning("[UIController] Score Text не назначен!");
        if (currentLevelText == null) Debug.LogWarning("[UIController] Current Level Text не назначен!");
        if (inventoryCountText == null) Debug.LogWarning("[UIController] Inventory Count Text не назначен!");
        if (inventoryGridParent == null) Debug.LogWarning("[UIController] Inventory Grid Parent не назначен!");
        if (inventoryItemPrefab == null) Debug.LogWarning("[UIController] Inventory Item Prefab не назначен!");
        if (itemBackgroundPrefab == null) Debug.LogWarning("[UIController] Item Background Prefab не назначен!");
    }

    private void UpdateAllUI()
    {
        UpdateScoreUI();
        UpdateLevelUI();
        UpdateInventoryCountUI();
    }
    
    public void SetTotalItemsCount(int totalCount)
    {
        totalValuableItems = totalCount;
        UpdateInventoryCountUI(); 
        Debug.Log($"[UIController] Установлено общее количество ценных предметов: {totalCount}");
    }

    public void ResetForNewLevel()
    {
        foreach (Transform child in inventoryGridParent)
        {
            Destroy(child.gameObject);
        }
        inventoryItems.Clear();
        collectedOnLevel = 0;
        Debug.Log("[UIController] Инвентарь и счетчики сброшены для нового уровня.");
        UpdateInventoryCountUI();
    }

    public void AddScore(int points)
    {
        currentScore += points;
        UpdateScoreUI();
    }
    public void SetCurrentLevel(int level)
    {
        currentLevel = level;
        UpdateLevelUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = scorePrefix + currentScore.ToString();
        }
    }

    private void UpdateLevelUI()
    {
        if (currentLevelText != null)
        {
            currentLevelText.text = levelPrefix + currentLevel.ToString();
        }
    }

    private void UpdateInventoryCountUI()
    {
        if (inventoryCountText != null)
        {
            inventoryCountText.text = inventoryPrefix + collectedOnLevel.ToString() + " / " + totalValuableItems.ToString();
        }
    }

    public int GetCurrentScore() { return currentScore; }
    public int GetCurrentLevel() { return currentLevel; }

    // --- ВОТ ИЗМЕНЕННЫЙ МЕТОД ---
    public void AddItemToInventory(string name, Sprite icon, int value)
    {
        // Шаг 1: Очки начисляются или списываются для ЛЮБОГО предмета.
        AddScore(value);

        // Шаг 2: Создаем визуальное представление (иконку) в инвентаре для ЛЮБОГО предмета.
        // Этот блок кода теперь находится вне всяких условий.
        InventoryItem newItem = new InventoryItem(name, icon, value);

        GameObject backgroundUI = null;
        if (itemBackgroundPrefab != null)
        {
            backgroundUI = Instantiate(itemBackgroundPrefab, inventoryGridParent);
        }

        GameObject itemUI = Instantiate(inventoryItemPrefab, backgroundUI != null ? backgroundUI.transform : inventoryGridParent);
        newItem.uiElement = backgroundUI != null ? backgroundUI : itemUI;

        InventoryItemUI itemUIComponent = itemUI.GetComponent<InventoryItemUI>();
        if (itemUIComponent != null)
        {
            itemUIComponent.SetSprite(icon);
        }
        
        inventoryItems.Add(newItem); // Добавляем в логический список, чтобы можно было его очистить при сбросе уровня

        // Шаг 3: А вот счетчик "собрано / всего" мы увеличиваем ТОЛЬКО для ценных предметов.
        if (value > 0)
        {
            collectedOnLevel++;
        }
        
        // Шаг 4: Обновляем текст счетчика. Он покажет правильное количество собранных ЦЕННОСТЕЙ.
        UpdateInventoryCountUI();
    }
}