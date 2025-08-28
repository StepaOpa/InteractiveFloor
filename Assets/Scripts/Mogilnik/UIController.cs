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
    [SerializeField] private Transform inventoryGridParent; // Родитель с Grid Layout Group для предметов

    [Header("Префабы для инвентаря")]
    [SerializeField] private GameObject inventoryItemPrefab; // Префаб UI элемента с Image компонентом
    [SerializeField] private GameObject itemBackgroundPrefab; // Префаб подложки под иконкой

    [Header("Настройки")]
    [SerializeField] private string scorePrefix = "Очки: ";
    [SerializeField] private string levelPrefix = "Уровень: ";
    [SerializeField] private string inventoryPrefix = "Предметов: ";

    // Данные игрока
    private int currentScore = 0;
    private int currentLevel = 1;
    private List<InventoryItem> inventoryItems = new List<InventoryItem>();
    
    // ИЗМЕНЕНИЕ: Переменная для хранения общего количества предметов
    private int totalCollectableItems = 0;

    public static UIController Instance { get; private set; }

    // Класс для хранения данных предмета в инвентаре
    [System.Serializable]
    public class InventoryItem
    {
        public string itemName;
        public Sprite itemIcon;
        public int itemValue;
        public GameObject uiElement; // Ссылка на UI элемент в инвентаре

        public InventoryItem(string name, Sprite icon, int value)
        {
            itemName = name;
            itemIcon = icon;
            itemValue = value;
        }
    }

    void Awake()
    {
        // Реализация Singleton
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
        Debug.Log("[UIController] Инициализация UI...");

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
    
    // ИЗМЕНЕНИЕ: Новый публичный метод для установки общего числа предметов
    public void SetTotalItemsCount(int totalCount)
    {
        totalCollectableItems = totalCount;
        UpdateInventoryCountUI(); 
        Debug.Log($"[UIController] Установлено общее количество предметов: {totalCount}");
    }

    public void AddScore(int points)
    {
        currentScore += points;
        UpdateScoreUI();
        Debug.Log($"[UIController] Добавлено очков: {points}. Общий счет: {currentScore}");
    }
    public void SetCurrentLevel(int level)
    {
        currentLevel = level;
        UpdateLevelUI();
        Debug.Log($"[UIController] Установлен уровень: {level}");
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

    // ИЗМЕНЕНИЕ: Логика отображения счетчика
    private void UpdateInventoryCountUI()
    {
        if (inventoryCountText != null)
        {
            inventoryCountText.text = inventoryPrefix + inventoryItems.Count.ToString() + " / " + totalCollectableItems.ToString();
        }
    }

    public int GetCurrentScore()
    {
        return currentScore;
    }

    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    public int GetInventoryCount()
    {
        return inventoryItems.Count;
    }

    public List<InventoryItem> GetInventoryItems()
    {
        return new List<InventoryItem>(inventoryItems);
    }

    public void AddItemToInventory(string name, Sprite icon, int value)
    {
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
            Debug.Log($"[UIController] Устанавливаю спрайт для '{name}': {icon?.name ?? "null"}");
            itemUIComponent.SetSprite(icon);
        }
        else
        {
            Debug.LogError($"[UIController] InventoryItemUI компонент не найден на префабе для '{name}'!");
        }

        inventoryItems.Add(newItem);
        AddScore(value);
        UpdateInventoryCountUI();
    }

    public void ClearInventory()
    {
        foreach (var item in inventoryItems)
        {
            if (item.uiElement != null)
                Destroy(item.uiElement);
        }

        inventoryItems.Clear();
        UpdateInventoryCountUI();
    }
}