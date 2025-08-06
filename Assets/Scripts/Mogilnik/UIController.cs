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
    
    [Header("Префаб для иконки предмета")]
    [SerializeField] private GameObject inventoryItemPrefab; // Префаб UI элемента с Image компонентом
    
    [Header("Настройки")]
    [SerializeField] private string scorePrefix = "Очки: ";
    [SerializeField] private string levelPrefix = "Уровень: ";
    [SerializeField] private string inventoryPrefix = "Предметов: ";
    
    // Данные игрока
    private int currentScore = 0;
    private int currentLevel = 1;
    private List<InventoryItem> inventoryItems = new List<InventoryItem>();
    
    // Singleton pattern для легкого доступа
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
    
    /// <summary>
    /// Инициализация UI элементов
    /// </summary>
    private void InitializeUI()
    {
        Debug.Log("[UIController] Инициализация UI...");
        
        // Проверяем наличие необходимых UI элементов
        if (scoreText == null)
            Debug.LogWarning("[UIController] Score Text не назначен!");
        
        if (currentLevelText == null)
            Debug.LogWarning("[UIController] Current Level Text не назначен!");
            
        if (inventoryCountText == null)
            Debug.LogWarning("[UIController] Inventory Count Text не назначен!");
            
        if (inventoryGridParent == null)
            Debug.LogWarning("[UIController] Inventory Grid Parent не назначен!");
            
        if (inventoryItemPrefab == null)
            Debug.LogWarning("[UIController] Inventory Item Prefab не назначен!");
    }
    
    /// <summary>
    /// Обновляет все UI элементы
    /// </summary>
    private void UpdateAllUI()
    {
        UpdateScoreUI();
        UpdateLevelUI();
        UpdateInventoryCountUI();
    }
    
    /// <summary>
    /// Добавляет очки к текущему счету
    /// </summary>
    /// <param name="points">Количество добавляемых очков</param>
    public void AddScore(int points)
    {
        currentScore += points;
        UpdateScoreUI();
        Debug.Log($"[UIController] Добавлено очков: {points}. Общий счет: {currentScore}");
    }
    
    /// <summary>
    /// Устанавливает текущий уровень
    /// </summary>
    /// <param name="level">Номер уровня</param>
    public void SetCurrentLevel(int level)
    {
        currentLevel = level;
        UpdateLevelUI();
        Debug.Log($"[UIController] Установлен уровень: {level}");
    }
    
    /// <summary>
    /// Добавляет предмет в визуальный инвентарь
    /// </summary>
    /// <param name="itemName">Название предмета</param>
    /// <param name="itemIcon">Спрайт предмета</param>
    /// <param name="itemValue">Ценность предмета</param>
    public void AddItemToInventory(string itemName, Sprite itemIcon, int itemValue)
    {
        if (string.IsNullOrEmpty(itemName))
        {
            Debug.LogWarning("[UIController] Пустое название предмета!");
            return;
        }
        
        // Создаем данные предмета
        InventoryItem newItem = new InventoryItem(itemName, itemIcon, itemValue);
        
        // Проверяем и создаем префаб если нужно
        EnsureInventoryItemPrefab();
        
        // Создаем UI элемент для предмета
        if (inventoryGridParent != null && inventoryItemPrefab != null)
        {
            GameObject itemUI = Instantiate(inventoryItemPrefab, inventoryGridParent);
            newItem.uiElement = itemUI;
            
            // Настраиваем через компонент InventoryItemUI (если есть)
            InventoryItemUI itemUIComponent = itemUI.GetComponent<InventoryItemUI>();
            if (itemUIComponent != null)
            {
                itemUIComponent.SetupItem(itemName, itemIcon, itemValue);
            }
            else
            {
                // Fallback: настраиваем напрямую через Image компонент
                Image itemImage = itemUI.GetComponent<Image>();
                if (itemImage != null)
                {
                    if (itemIcon != null)
                    {
                        itemImage.sprite = itemIcon;
                        itemImage.preserveAspect = true;
                        itemImage.color = Color.white;
                    }
                    else
                    {
                        // Если спрайт не задан, используем цветной квадрат
                        itemImage.sprite = null;
                        itemImage.color = GetRandomColor();
                    }
                }
                
                // Добавляем подсказку с названием предмета (опционально)
                SetupItemTooltip(itemUI, itemName, itemValue);
            }
            
            Debug.Log($"[UIController] ✓ Создан UI элемент для предмета '{itemName}'");
        }
        else
        {
            Debug.LogWarning("[UIController] ✗ Не удалось создать UI элемент - отсутствуют ссылки!");
        }
        
        // Добавляем в список
        inventoryItems.Add(newItem);
        
        // Добавляем очки
        AddScore(itemValue);
        
        // Обновляем UI
        UpdateInventoryCountUI();
        
        Debug.Log($"[UIController] Предмет '{itemName}' добавлен в инвентарь. Всего предметов: {inventoryItems.Count}");
    }
    
    /// <summary>
    /// Настройка подсказки для предмета (опционально)
    /// </summary>
    private void SetupItemTooltip(GameObject itemUI, string itemName, int itemValue)
    {
        // Можно добавить компонент Tooltip или Button с обработчиком
        Button itemButton = itemUI.GetComponent<Button>();
        if (itemButton != null)
        {
            itemButton.onClick.AddListener(() => {
                Debug.Log($"[UIController] Нажат предмет: {itemName} (ценность: {itemValue})");
            });
        }
    }
    
    /// <summary>
    /// Получает случайный цвет для предметов без спрайта
    /// </summary>
    private Color GetRandomColor()
    {
        Color[] colors = {
            Color.red, Color.green, Color.blue, Color.yellow, 
            Color.magenta, Color.cyan, new Color(1f, 0.5f, 0f), // оранжевый
            new Color(0.5f, 0f, 1f) // фиолетовый
        };
        return colors[Random.Range(0, colors.Length)];
    }
    
    /// <summary>
    /// Очищает инвентарь
    /// </summary>
    public void ClearInventory()
    {
        // Удаляем все UI элементы
        foreach (var item in inventoryItems)
        {
            if (item.uiElement != null)
            {
                Destroy(item.uiElement);
            }
        }
        
        inventoryItems.Clear();
        UpdateInventoryCountUI();
        Debug.Log("[UIController] Инвентарь очищен");
    }
    
    /// <summary>
    /// Обновляет отображение счета
    /// </summary>
    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = scorePrefix + currentScore.ToString();
        }
    }
    
    /// <summary>
    /// Обновляет отображение текущего уровня
    /// </summary>
    private void UpdateLevelUI()
    {
        if (currentLevelText != null)
        {
            currentLevelText.text = levelPrefix + currentLevel.ToString();
        }
    }
    
    /// <summary>
    /// Обновляет отображение количества предметов в инвентаре
    /// </summary>
    private void UpdateInventoryCountUI()
    {
        if (inventoryCountText != null)
        {
            inventoryCountText.text = inventoryPrefix + inventoryItems.Count.ToString();
        }
    }
    
    /// <summary>
    /// Получает текущий счет
    /// </summary>
    public int GetCurrentScore()
    {
        return currentScore;
    }
    
    /// <summary>
    /// Получает текущий уровень
    /// </summary>
    public int GetCurrentLevel()
    {
        return currentLevel;
    }
    
    /// <summary>
    /// Получает количество предметов в инвентаре
    /// </summary>
    public int GetInventoryCount()
    {
        return inventoryItems.Count;
    }
    
    /// <summary>
    /// Получает копию списка предметов инвентаря
    /// </summary>
    public List<InventoryItem> GetInventoryItems()
    {
        return new List<InventoryItem>(inventoryItems);
    }
    
    /// <summary>
    /// Создает простой префаб для элемента инвентаря программно (если не задан в инспекторе)
    /// </summary>
    /// <returns>GameObject префаб для элемента инвентаря</returns>
    public GameObject CreateSimpleInventoryItemPrefab()
    {
        // Создаем основной GameObject
        GameObject prefab = new GameObject("InventoryItem");
        
        // Добавляем RectTransform
        RectTransform rectTransform = prefab.AddComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(64, 64); // Размер 64x64 пикселя
        
        // Добавляем Image компонент
        Image image = prefab.AddComponent<Image>();
        image.color = Color.white;
        image.preserveAspect = true;
        
        // Добавляем Button для интерактивности
        Button button = prefab.AddComponent<Button>();
        button.transition = Selectable.Transition.ColorTint;
        
        // Добавляем InventoryItemUI компонент
        InventoryItemUI itemUI = prefab.AddComponent<InventoryItemUI>();
        
        Debug.Log("[UIController] Создан простой префаб элемента инвентаря программно");
        return prefab;
    }
    /// Проверяет и создает префаб элемента инвентаря если он не задан
    private void EnsureInventoryItemPrefab()
    {
        if (inventoryItemPrefab == null)
        {
            Debug.LogWarning("[UIController] Inventory Item Prefab не назначен! Создаем простой префаб программно...");
            inventoryItemPrefab = CreateSimpleInventoryItemPrefab();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
