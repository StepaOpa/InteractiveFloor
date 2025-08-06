using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// Компонент для UI элемента предмета в инвентаре
/// </summary>
public class InventoryItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Компоненты")]
    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI itemNameText; // Опционально
    [SerializeField] private GameObject tooltipPanel; // Опционально
    [SerializeField] private TextMeshProUGUI tooltipText; // Опционально
    
    // Данные предмета
    private string itemName;
    private int itemValue;
    
    void Start()
    {
        // Автоматически находим компоненты если не назначены
        if (itemImage == null)
            itemImage = GetComponent<Image>();
            
        // Изначально скрываем подсказку
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
    }
    
    /// <summary>
    /// Настройка элемента инвентаря
    /// </summary>
    /// <param name="name">Название предмета</param>
    /// <param name="icon">Иконка предмета</param>
    /// <param name="value">Ценность предмета</param>
    public void SetupItem(string name, Sprite icon, int value)
    {
        itemName = name;
        itemValue = value;
        
        // Устанавливаем иконку
        if (itemImage != null)
        {
            if (icon != null)
            {
                itemImage.sprite = icon;
                itemImage.color = Color.white;
                itemImage.preserveAspect = true;
            }
            else
            {
                // Если спрайт не задан, показываем цветной квадрат
                itemImage.sprite = null;
                itemImage.color = GetItemColor(name);
            }
        }
        
        // Устанавливаем текст названия (если есть)
        if (itemNameText != null)
        {
            itemNameText.text = name;
        }
        
        Debug.Log($"[InventoryItemUI] Настроен элемент инвентаря: {name} (ценность: {value})");
    }
    
    /// <summary>
    /// Получить цвет для предмета на основе его названия
    /// </summary>
    /// <param name="name">Название предмета</param>
    /// <returns>Цвет для предмета</returns>
    private Color GetItemColor(string name)
    {
        // Генерируем цвет на основе хеша названия для консистентности
        int hash = name.GetHashCode();
        Random.State oldState = Random.state;
        Random.InitState(hash);
        
        Color color = new Color(
            Random.Range(0.3f, 1f),
            Random.Range(0.3f, 1f),
            Random.Range(0.3f, 1f),
            1f
        );
        
        Random.state = oldState;
        return color;
    }
    
    /// <summary>
    /// Вызывается при наведении мыши
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        ShowTooltip();
    }
    
    /// <summary>
    /// Вызывается при уходе мыши
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        HideTooltip();
    }
    
    /// <summary>
    /// Показать подсказку с информацией о предмете
    /// </summary>
    private void ShowTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(true);
            
            if (tooltipText != null)
            {
                tooltipText.text = $"{itemName}\nЦенность: {itemValue}";
            }
        }
        
        // Увеличиваем при наведении
        transform.localScale = Vector3.one * 1.1f;
        
        Debug.Log($"[InventoryItemUI] Показана подсказка для: {itemName}");
    }
    
    /// <summary>
    /// Скрыть подсказку
    /// </summary>
    private void HideTooltip()
    {
        if (tooltipPanel != null)
        {
            tooltipPanel.SetActive(false);
        }
        
        // Возвращаем нормальный размер
        transform.localScale = Vector3.one;
    }
    
    /// <summary>
    /// Получить название предмета
    /// </summary>
    public string GetItemName()
    {
        return itemName;
    }
    
    /// <summary>
    /// Получить ценность предмета
    /// </summary>
    public int GetItemValue()
    {
        return itemValue;
    }
}