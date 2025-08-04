using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class UIController : MonoBehaviour
{
    [Header("UI Элементы")]
    public Transform collectedItemsContainer; // Контейнер для иконок в левом углу
    public GameObject itemIconPrefab; // Префаб для иконки предмета
    public TextMeshProUGUI scoreText; // Текст счета
    public TextMeshProUGUI itemCountText; // Количество собранных предметов
    public TextMeshProUGUI timerText; // Таймер
    
    [Header("Настройки игры")]
    public float gameTime = 180f; // 3 минуты на уровень
    public int maxItemsToShow = 10; // Максимум иконок для показа
    
    private int currentScore = 0;
    private int itemsCollected = 0;
    private float timeRemaining;
    private List<GameObject> itemIcons = new List<GameObject>();
    private bool gameActive = true;
    
    void Start()
    {
        timeRemaining = gameTime;
        UpdateUI();
        
        // Создаем UI если он не существует
        CreateUIIfNeeded();
    }
    
    void Update()
    {
        if (gameActive)
        {
            // Обновляем таймер
            timeRemaining -= Time.deltaTime;
            
            if (timeRemaining <= 0)
            {
                timeRemaining = 0;
                EndGame();
            }
            
            UpdateTimer();
        }
    }
    
    public void OnItemCollected(CollectableItem item)
    {
        if (!gameActive) return;
        
        // Увеличиваем счет
        currentScore += item.itemValue;
        itemsCollected++;
        
        // Добавляем иконку в интерфейс
        AddItemIcon(item);
        
        // Обновляем UI
        UpdateUI();
        
        Debug.Log($"Собран предмет: {item.itemName}, Очки: {item.itemValue}");
    }
    
    private void AddItemIcon(CollectableItem item)
    {
        if (collectedItemsContainer == null || itemIconPrefab == null) return;
        
        // Создаем новую иконку (подложку)
        GameObject newIcon = Instantiate(itemIconPrefab, collectedItemsContainer);
        
        // Находим или создаем Image компонент для иконки артефакта
        Image itemIconImage = FindOrCreateItemIconImage(newIcon);
        
        // Настраиваем иконку артефакта
        if (itemIconImage != null && item.itemIcon != null)
        {
            itemIconImage.sprite = item.itemIcon;
            itemIconImage.color = Color.white; // Убеждаемся что иконка видна
        }
        
        // Настраиваем компонент ItemIcon если есть
        ItemIcon itemIconComponent = newIcon.GetComponent<ItemIcon>();
        if (itemIconComponent != null)
        {
            itemIconComponent.SetIcon(item.itemIcon);
        }
        
        // Добавляем в список
        itemIcons.Add(newIcon);
        
        // Ограничиваем количество показываемых иконок
        if (itemIcons.Count > maxItemsToShow)
        {
            Destroy(itemIcons[0]);
            itemIcons.RemoveAt(0);
        }
    }
    
    private Image FindOrCreateItemIconImage(GameObject iconPrefabInstance)
    {
        // Сначала ищем дочерний объект с именем "ItemIcon" или "Icon"
        Transform iconTransform = iconPrefabInstance.transform.Find("ItemIcon");
        if (iconTransform == null)
            iconTransform = iconPrefabInstance.transform.Find("Icon");
        
        if (iconTransform != null)
        {
            Image existingImage = iconTransform.GetComponent<Image>();
            if (existingImage != null)
                return existingImage;
        }
        
        // Если не найден, создаем новый дочерний объект для иконки
        GameObject iconChild = new GameObject("ItemIcon");
        iconChild.transform.SetParent(iconPrefabInstance.transform, false);
        
        // Настраиваем RectTransform для полного покрытия родителя
        RectTransform rectTransform = iconChild.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        
        // Добавляем Image компонент
        Image iconImage = iconChild.AddComponent<Image>();
        iconImage.preserveAspect = true; // Сохраняем пропорции иконки
        
        return iconImage;
    }
    
    private void UpdateUI()
    {
        if (scoreText != null)
            scoreText.text = $"Очки: {currentScore}";
            
        if (itemCountText != null)
            itemCountText.text = $"Предметов: {itemsCollected}";
    }
    
    private void UpdateTimer()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timeRemaining / 60);
            int seconds = Mathf.FloorToInt(timeRemaining % 60);
            timerText.text = $"Время: {minutes:00}:{seconds:00}";
        }
    }
    
    private void EndGame()
    {
        gameActive = false;
        Debug.Log($"Игра окончена! Финальный счет: {currentScore}, Собрано предметов: {itemsCollected}");
        
        // Здесь можно добавить экран результатов
        ShowGameResults();
    }
    
    private void ShowGameResults()
    {
        // Показываем результаты игры
        Debug.Log("=== РЕЗУЛЬТАТЫ РАСКОПОК ===");
        Debug.Log($"Собрано артефактов: {itemsCollected}");
        Debug.Log($"Общий счет: {currentScore}");
        Debug.Log($"Время: {Mathf.FloorToInt((gameTime - timeRemaining) / 60):00}:{Mathf.FloorToInt((gameTime - timeRemaining) % 60):00}");
    }
    
    public void RestartGame()
    {
        // Сброс игры
        currentScore = 0;
        itemsCollected = 0;
        timeRemaining = gameTime;
        gameActive = true;
        
        // Очищаем иконки
        foreach (GameObject icon in itemIcons)
        {
            if (icon != null) Destroy(icon);
        }
        itemIcons.Clear();
        
        UpdateUI();
    }
    
    private void CreateUIIfNeeded()
    {
        // Автоматически создаем базовые UI элементы если их нет
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("Canvas не найден! Создайте Canvas для отображения UI.");
            return;
        }
        
        // Здесь можно добавить автоматическое создание UI элементов
    }
    
    // Публичные методы для внешнего управления
    public int GetCurrentScore() => currentScore;
    public int GetItemsCollected() => itemsCollected;
    public float GetTimeRemaining() => timeRemaining;
    public bool IsGameActive() => gameActive;
}
