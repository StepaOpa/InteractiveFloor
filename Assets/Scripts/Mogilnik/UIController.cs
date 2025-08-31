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

    private int totalValuableItems = 0;
    private int collectedOnLevel = 0;

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

    // --- ВОТ ГЛАВНОЕ ИЗМЕНЕНИЕ ---
    public void ResetForNewLevel()
    {
        // Весь код ниже закомментирован, чтобы инвентарь НЕ сбрасывался при переходе на новый уровень.
        // Теперь этот метод можно безопасно вызывать, и он ничего не сделает.

        /*
        foreach (Transform child in inventoryGridParent)
        {
            Destroy(child.gameObject);
        }
        inventoryItems.Clear();
        collectedOnLevel = 0; // Эту переменную тоже можно оставить, если счетчик "собрано/всего" должен быть уникальным для каждого уровня
        Debug.Log("[UIController] Инвентарь и счетчики сброшены для нового уровня.");
        UpdateInventoryCountUI();
        */
        Debug.Log("[UIController] Попытка сброса инвентаря проигнорирована для сохранения предметов между уровнями.");
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

    public void AddItemToInventory(string name, Sprite icon, int value)
    {
        AddScore(value);

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

        inventoryItems.Add(newItem);

        if (value > 0)
        {
            collectedOnLevel++;
        }

        UpdateInventoryCountUI();
    }
}