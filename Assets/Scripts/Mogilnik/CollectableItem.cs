using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CollectableItem : MonoBehaviour, IPointerClickHandler
{
    [Header("Настройки предмета")]
    public string itemName = "Артефакт";
    public Sprite itemIcon;
    public int itemValue = 10;
    
    [Header("Настройки анимации")]
    public float flightDuration = 1f;
    public AnimationCurve flightCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private bool isCollected = false;
    private UIController uiController;
    
    void Start()
    {
        // Найдем UIController в сцене
        uiController = FindObjectOfType<UIController>();
        
        // Добавляем Collider2D если его нет (для обработки кликов)
        if (GetComponent<Collider2D>() == null)
        {
            gameObject.AddComponent<CircleCollider2D>();
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"OnPointerClick вызван для {itemName}");
        if (!isCollected)
        {
            CollectItem();
        }
    }
    
    void OnMouseDown()
    {
        // Альтернативный способ обработки клика для 3D объектов
        Debug.Log($"OnMouseDown вызван для {itemName}");
        if (!isCollected)
        {
            CollectItem();
        }
    }
    
    // Публичный метод для тестирования анимации из инспектора
    [ContextMenu("Тест анимации")]
    public void TestAnimation()
    {
        if (!isCollected)
        {
            Debug.Log("Тестируем анимацию...");
            CollectItem();
        }
    }
    
    private void CollectItem()
    {
        isCollected = true;
        
        // Уведомляем UIController о сборе предмета
        if (uiController != null)
        {
            uiController.OnItemCollected(this);
        }
        
        // Запускаем анимацию полета к UI
        Debug.Log($"Запускаем анимацию для {itemName}");
        StartCoroutine(FlyToUI());
    }
    
    private IEnumerator FlyToUI()
    {
        Debug.Log("Начинаем анимацию полета");
        
        // Проверяем наличие камеры
        Camera camera = Camera.main;
        if (camera == null)
        {
            camera = FindObjectOfType<Camera>();
            if (camera == null)
            {
                Debug.LogError("Камера не найдена! Анимация не может быть выполнена.");
                Destroy(gameObject);
                yield break;
            }
        }
        
        Vector3 startPosition = transform.position;
        
        // Рассчитываем целевую позицию для левого верхнего угла
        Vector3 screenPoint = new Vector3(100, Screen.height - 100, camera.nearClipPlane + 2f);
        Vector3 targetPosition = camera.ScreenToWorldPoint(screenPoint);
        
        // Для 2D игр сохраняем Z координату
        if (Mathf.Abs(startPosition.z) < 0.1f)
        {
            targetPosition.z = startPosition.z;
        }
        
        Debug.Log($"Стартовая позиция: {startPosition}, Целевая позиция: {targetPosition}");
        
        float elapsedTime = 0f;
        Vector3 originalScale = transform.localScale;
        
        while (elapsedTime < flightDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / flightDuration;
            
            // Применяем кривую анимации
            float curveValue = flightCurve.Evaluate(progress);
            
            // Интерполируем позицию
            transform.position = Vector3.Lerp(startPosition, targetPosition, curveValue);
            
            // Уменьшаем размер по мере приближения к UI
            float scale = Mathf.Lerp(1f, 0.3f, curveValue);
            transform.localScale = originalScale * scale;
            
            yield return null;
        }
        
        Debug.Log("Анимация завершена, уничтожаем объект");
        // Уничтожаем объект после анимации
        Destroy(gameObject);
    }
}