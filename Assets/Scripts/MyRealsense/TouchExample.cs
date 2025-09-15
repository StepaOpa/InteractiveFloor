using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Пример использования системы касаний TouchProcessor.
/// Этот скрипт показывает, как подписаться на события касаний и использовать их.
/// </summary>
public class TouchExample : MonoBehaviour
{
    [Header("Ссылки")]
    public TouchProcessor touchProcessor;
    
    [Header("Визуализация касаний")]
    public GameObject touchVisualizationPrefab; // Префаб для визуализации касания (например, простой Cube)
    public Transform touchContainer; // Родительский объект для визуализации касаний

    // Словарь для отслеживания визуальных представлений касаний
    private Dictionary<int, GameObject> _touchVisuals = new Dictionary<int, GameObject>();

    void Start()
    {
        if (touchProcessor == null)
        {
            Debug.LogError("TouchProcessor не назначен!", this);
            enabled = false;
            return;
        }

        // Подписываемся на события касаний
        touchProcessor.OnTouchBegan += OnTouchBegan;
        touchProcessor.OnTouchMoved += OnTouchMoved;
        touchProcessor.OnTouchEnded += OnTouchEnded;
        
        Debug.Log("TouchExample инициализирован. Готов к обработке касаний.");
    }

    void OnDestroy()
    {
        // Отписываемся от событий при уничтожении объекта
        if (touchProcessor != null)
        {
            touchProcessor.OnTouchBegan -= OnTouchBegan;
            touchProcessor.OnTouchMoved -= OnTouchMoved;
            touchProcessor.OnTouchEnded -= OnTouchEnded;
        }
    }

    /// <summary>
    /// Вызывается, когда начинается новое касание.
    /// </summary>
    private void OnTouchBegan(TouchInfo touch)
    {
        Debug.Log($"🟢 Касание началось! ID: {touch.TouchId}, Позиция в проекции: {touch.PositionInProjection}, Пикселей: {touch.PixelCount}");
        
        // Создаем визуальное представление касания
        CreateTouchVisual(touch);
        
        // Здесь можно добавить игровую логику, например:
        // - Создать эффект частиц в месте касания
        // - Проиграть звук
        // - Проверить, попало ли касание в какой-то игровой объект
    }

    /// <summary>
    /// Вызывается, когда касание движется.
    /// </summary>
    private void OnTouchMoved(TouchInfo touch)
    {
        Debug.Log($"🟡 Касание движется! ID: {touch.TouchId}, Новая позиция: {touch.PositionInProjection}, Смещение: {touch.DeltaPosition.magnitude:F2} пикселей");
        
        // Обновляем визуальное представление
        UpdateTouchVisual(touch);
        
        // Здесь можно добавить логику для жестов:
        // - Отслеживание свайпов
        // - Обновление позиции перетаскиваемых объектов
        // - Рисование траектории движения
    }

    /// <summary>
    /// Вызывается, когда касание заканчивается.
    /// </summary>
    private void OnTouchEnded(TouchInfo touch)
    {
        Debug.Log($"🔴 Касание закончилось! ID: {touch.TouchId}, Длительность: {touch.Duration:F2} сек");
        
        // Удаляем визуальное представление
        RemoveTouchVisual(touch.TouchId);
        
        // Здесь можно добавить логику завершения:
        // - Обработать клик/тап
        // - Завершить перетаскивание
        // - Анализировать жесты (например, был ли это свайп)
    }

    /// <summary>
    /// Создает визуальное представление касания в 3D-пространстве.
    /// </summary>
    private void CreateTouchVisual(TouchInfo touch)
    {
        if (touchVisualizationPrefab == null || touchContainer == null) return;

        // Создаем объект для визуализации
        GameObject visual = Instantiate(touchVisualizationPrefab, touchContainer);
        
        // Преобразуем координаты проекции в мировые координаты
        Vector3 worldPosition = ProjectionToWorldPosition(touch.PositionInProjection);
        visual.transform.position = worldPosition;
        
        // Сохраняем ссылку на визуал
        _touchVisuals[touch.TouchId] = visual;
        
        // Можно настроить размер в зависимости от количества пикселей касания
        float scale = Mathf.Clamp(touch.PixelCount / 20f, 0.5f, 2f);
        visual.transform.localScale = Vector3.one * scale;
    }

    /// <summary>
    /// Обновляет позицию визуального представления касания.
    /// </summary>
    private void UpdateTouchVisual(TouchInfo touch)
    {
        if (_touchVisuals.TryGetValue(touch.TouchId, out GameObject visual))
        {
            Vector3 worldPosition = ProjectionToWorldPosition(touch.PositionInProjection);
            visual.transform.position = worldPosition;
        }
    }

    /// <summary>
    /// Удаляет визуальное представление касания.
    /// </summary>
    private void RemoveTouchVisual(int touchId)
    {
        if (_touchVisuals.TryGetValue(touchId, out GameObject visual))
        {
            Destroy(visual);
            _touchVisuals.Remove(touchId);
        }
    }

    /// <summary>
    /// Преобразует координаты проекции (0-1) в мировые координаты.
    /// Эта функция зависит от вашей конкретной настройки сцены.
    /// </summary>
    private Vector3 ProjectionToWorldPosition(Vector2 projectionPos)
    {
        // Пример преобразования для плоскости на уровне Y=0
        // Предполагаем, что проекция покрывает область от -5 до +5 по X и Z
        float worldX = Mathf.Lerp(-5f, 5f, projectionPos.x);
        float worldZ = Mathf.Lerp(-5f, 5f, 1f - projectionPos.y); // Инвертируем Y для корректного отображения
        
        return new Vector3(worldX, 0.1f, worldZ);
    }

    void Update()
    {
        // Выводим информацию о текущих касаниях (для отладки)
        if (touchProcessor != null && touchProcessor.ActiveTouches.Count > 0)
        {
            // Эту строку можно закомментировать, если вывод в консоль не нужен
            // Debug.Log($"Активных касаний: {touchProcessor.ActiveTouches.Count}");
        }
    }

    void OnGUI()
    {
        // Простая отладочная информация на экране
        if (touchProcessor == null) return;

        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label($"Активных касаний: {touchProcessor.ActiveTouches.Count}");
        
        foreach (var touch in touchProcessor.ActiveTouches)
        {
            GUILayout.Label($"ID {touch.TouchId}: {touch.State} в ({touch.PositionInProjection.x:F2}, {touch.PositionInProjection.y:F2})");
        }
        GUILayout.EndArea();
    }
}

