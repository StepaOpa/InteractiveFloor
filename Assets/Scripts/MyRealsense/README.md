# Система касаний для интерактивного пола

## Обзор

Эта система превращает данные с камеры глубины RealSense в события касаний, аналогично сенсорному экрану. Система состоит из двух основных компонентов:

- **TouchProcessor** - основной компонент, который обрабатывает данные камеры и генерирует события касаний
- **TouchExample** - пример использования системы касаний

## Настройка

### 1. Настройка TouchProcessor

1. Добавьте компонент `TouchProcessor` на любой GameObject в сцене
2. Настройте следующие поля в инспекторе:

**RealSense:**
- `Source` - перетащите объект с компонентом `RsDevice` или `RsProcessingPipe`

**Калибровка:**
- `Calibration Markers` - массив из 4 UI элементов для калибровки углов проекции
- `Calibration UI` - родительский объект для UI калибровки
- `Touch Threshold Mm` - чувствительность касания (по умолчанию 50мм)

**Визуализация:**
- `Debug Image` - (необязательно) RawImage для отображения карты глубины
- `Max Color Depth Mm` - максимальная глубина для цветовой карты
- `Visualization Mode` - режим отображения (ContourBands рекомендуется для детектирования объектов)

**Система касаний:**
- `Touch Cluster Radius` - радиус группировки точек (30 пикселей по умолчанию)
- `Min Touch Pixels` - минимальное количество пикселей для касания (5 по умолчанию)
- `Touch Timeout Seconds` - время до завершения касания (0.5 сек по умолчанию)
- `Movement Threshold` - минимальное смещение для регистрации движения (5 пикселей)

### 2. Калибровка

1. Запустите сцену
2. Нажмите кнопку "Начать калибровку"
3. Перетащите 4 маркера по углам области проекции
4. Убедитесь, что в области проекции нет посторонних объектов
5. Нажмите "Сохранить"

Система запомнит калибровку между запусками.

## Использование в коде

### Подписка на события

```csharp
public class MyTouchHandler : MonoBehaviour
{
    public TouchProcessor touchProcessor;

    void Start()
    {
        touchProcessor.OnTouchBegan += OnTouchBegan;
        touchProcessor.OnTouchMoved += OnTouchMoved;
        touchProcessor.OnTouchEnded += OnTouchEnded;
    }

    private void OnTouchBegan(TouchInfo touch)
    {
        Debug.Log($"Касание началось: {touch.TouchId} в позиции {touch.PositionInProjection}");
    }

    private void OnTouchMoved(TouchInfo touch)
    {
        Debug.Log($"Касание движется: {touch.TouchId}, смещение: {touch.DeltaPosition}");
    }

    private void OnTouchEnded(TouchInfo touch)
    {
        Debug.Log($"Касание закончилось: {touch.TouchId}, длительность: {touch.Duration}");
    }
}
```

### Класс TouchInfo

Каждое касание представлено объектом `TouchInfo` с следующими свойствами:

- `TouchId` - уникальный ID касания
- `PositionInCamera` - позиция в пикселях камеры
- `PositionInProjection` - позиция в координатах проекции (0-1)
- `PositionInScreen` - позиция в экранных координатах Unity
- `State` - состояние (Began, Moved, Stationary, Ended)
- `StartTime` - время начала касания
- `Duration` - длительность касания
- `DeltaPosition` - смещение с предыдущего кадра
- `PixelCount` - размер области касания

### Доступ к активным касаниям

```csharp
// Получить все активные касания
List<TouchInfo> activeTouches = touchProcessor.ActiveTouches;

// Найти касание по ID
TouchInfo touch = activeTouches.FirstOrDefault(t => t.TouchId == targetId);
```

## Примеры использования

### Простое реагирование на касания

```csharp
private void OnTouchBegan(TouchInfo touch)
{
    // Создать эффект в месте касания
    Vector3 worldPos = ProjectionToWorldPosition(touch.PositionInProjection);
    Instantiate(touchEffectPrefab, worldPos, Quaternion.identity);
}
```

### Перетаскивание объектов

```csharp
private Dictionary<int, GameObject> draggedObjects = new Dictionary<int, GameObject>();

private void OnTouchBegan(TouchInfo touch)
{
    // Найти объект под касанием
    GameObject obj = FindObjectAtPosition(touch.PositionInProjection);
    if (obj != null)
    {
        draggedObjects[touch.TouchId] = obj;
    }
}

private void OnTouchMoved(TouchInfo touch)
{
    if (draggedObjects.TryGetValue(touch.TouchId, out GameObject obj))
    {
        Vector3 newPos = ProjectionToWorldPosition(touch.PositionInProjection);
        obj.transform.position = newPos;
    }
}

private void OnTouchEnded(TouchInfo touch)
{
    draggedObjects.Remove(touch.TouchId);
}
```

### Детектирование жестов

```csharp
private void OnTouchEnded(TouchInfo touch)
{
    float distance = touch.DeltaPosition.magnitude;
    float duration = touch.Duration;
    
    if (distance > 50 && duration < 1f)
    {
        Debug.Log("Обнаружен свайп!");
        Vector2 direction = touch.DeltaPosition.normalized;
        // Обработать свайп...
    }
    else if (distance < 10 && duration < 0.5f)
    {
        Debug.Log("Обнаружен тап!");
        // Обработать тап...
    }
}
```

## Настройка производительности

- Уменьшите `Touch Cluster Radius` для более точного разделения касаний
- Увеличьте `Min Touch Pixels` для фильтрации случайных шумов
- Настройте `Touch Threshold Mm` в зависимости от высоты установки камеры
- Используйте режим визуализации `LocalContrast` для лучшего выделения объектов

## Устранение неполадок

**Касания не детектируются:**
- Проверьте калибровку
- Убедитесь, что `Touch Threshold Mm` настроен правильно
- Проверьте, что объекты достаточно близко к поверхности

**Слишком много ложных касаний:**
- Увеличьте `Min Touch Pixels`
- Увеличьте `Touch Threshold Mm`
- Улучшите освещение сцены

**Касания "дрожат":**
- Уменьшите `Movement Threshold`
- Увеличьте `Touch Cluster Radius`
- Проверьте стабильность камеры

