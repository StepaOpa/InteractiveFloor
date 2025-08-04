using UnityEngine;
using UnityEngine.UI;

public class ItemIcon : MonoBehaviour
{
    [Header("Компоненты иконки")]
    public Image backgroundImage; // Фоновая подложка (цветная рамка)
    public Image iconImage; // Иконка артефакта (автопоиск если не назначена)
    public GameObject glowEffect; // Эффект свечения при добавлении
    
    [Header("Настройки анимации")]
    public float appearDuration = 0.5f;
    public AnimationCurve appearCurve;
    
    private void Awake()
    {
        // Создаем кривую по умолчанию если не настроена
        if (appearCurve == null || appearCurve.keys.Length == 0)
        {
            appearCurve = new AnimationCurve();
            // Создаем эффект "отскока"
            appearCurve.AddKey(0f, 0f);
            appearCurve.AddKey(0.7f, 1.1f); // Перелет
            appearCurve.AddKey(0.9f, 0.95f); // Отскок назад
            appearCurve.AddKey(1f, 1f); // Финальная позиция
        }
        
        // Автопоиск компонентов если не назначены
        FindUIComponents();
    }
    
    private void FindUIComponents()
    {
        // Если backgroundImage не назначен, используем Image на этом объекте
        if (backgroundImage == null)
        {
            backgroundImage = GetComponent<Image>();
        }
        
        // Если iconImage не назначен, ищем дочерний объект
        if (iconImage == null)
        {
            // Ищем дочерний объект с именем "ItemIcon" или "Icon"
            Transform iconTransform = transform.Find("ItemIcon");
            if (iconTransform == null)
                iconTransform = transform.Find("Icon");
            
            if (iconTransform != null)
            {
                iconImage = iconTransform.GetComponent<Image>();
            }
        }
    }
    
    private void Start()
    {
        // Анимация появления иконки
        StartCoroutine(AnimateAppear());
    }
    
    private System.Collections.IEnumerator AnimateAppear()
    {
        Vector3 originalScale = transform.localScale;
        transform.localScale = Vector3.zero;
        
        // Включаем эффект свечения если есть
        if (glowEffect != null)
            glowEffect.SetActive(true);
        
        float elapsedTime = 0f;
        
        while (elapsedTime < appearDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / appearDuration;
            
            float curveValue = appearCurve.Evaluate(progress);
            transform.localScale = originalScale * curveValue;
            
            yield return null;
        }
        
        transform.localScale = originalScale;
        
        // Выключаем эффект свечения после анимации
        if (glowEffect != null)
        {
            yield return new WaitForSeconds(0.3f);
            glowEffect.SetActive(false);
        }
    }
    
    public void SetIcon(Sprite sprite)
    {
        // Убеждаемся что компоненты найдены
        if (iconImage == null)
            FindUIComponents();
        
        if (iconImage != null)
        {
            iconImage.sprite = sprite;
            iconImage.color = Color.white; // Убеждаемся что иконка видна
        }
        else
        {
            Debug.LogWarning($"IconImage не найден на {gameObject.name}. Иконка не установлена.");
        }
    }
    
    public void SetBackgroundColor(Color color)
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = color;
        }
    }
    
    public void SetBackgroundSprite(Sprite sprite)
    {
        if (backgroundImage != null)
        {
            backgroundImage.sprite = sprite;
        }
    }
    
    // Получение компонентов для внешнего доступа
    public Image GetIconImage() => iconImage;
    public Image GetBackgroundImage() => backgroundImage;
}