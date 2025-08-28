# Настройка системы инвентаря в Unity

## 📋 Обзор системы

Система инвентаря состоит из:
- **UIController.cs** - основной контроллер UI с инвентарем
- **InventoryItemUI.cs** - компонент для элементов в инвентаре
- **CollectableItem.cs** - предметы, которые можно подобрать
- **InspectionUI.cs** - UI для осмотра предметов

## 🎯 Быстрая настройка

### Шаг 1: Создание UI Canvas
```
1. GameObject → UI → Canvas
2. Добавить GameObject "UIController" как дочерний к Canvas
3. Добавить компонент UIController на UIController GameObject
```

### Шаг 2: Создание элементов UI

#### A. Счет и уровень
```
1. GameObject → UI → Text - TextMeshPro (scoreText)
2. GameObject → UI → Text - TextMeshPro (currentLevelText)  
3. GameObject → UI → Text - TextMeshPro (inventoryCountText)
```

#### B. Панель инвентаря с Grid Layout
```
1. GameObject → UI → Panel (inventoryPanel)
2. Добавить компонент Grid Layout Group:
   - Cell Size: (64, 64)
   - Spacing: (5, 5)
   - Constraint: Fixed Column Count = 5
3. Назначить этот GameObject в поле "Inventory Grid Parent"
```

### Шаг 3: Создание префаба элемента инвентаря (опционально)

#### Автоматический способ:
- Оставьте поле "Inventory Item Prefab" пустым
- Система создаст простой префаб автоматически

#### Ручной способ:
```
1. GameObject → UI → Image
2. Добавить компонент Button
3. Добавить компонент InventoryItemUI
4. Сохранить как Prefab
5. Назначить в поле "Inventory Item Prefab"
```

## ⚙️ Настройка UIController в инспекторе

```csharp
[Header("UI Элементы")]
scoreText               // TextMeshProUGUI для отображения очков
currentLevelText        // TextMeshProUGUI для отображения уровня
inventoryCountText      // TextMeshProUGUI для количества предметов
inventoryGridParent     // Transform с Grid Layout Group

[Header("Префаб для иконки предмета")]
inventoryItemPrefab     // GameObject с Image и InventoryItemUI (опционально)

[Header("Настройки")]
scorePrefix = "Очки: "
levelPrefix = "Уровень: "
inventoryPrefix = "Предметов: "
```

## 🎮 Настройка предметов

### В CollectableItem.cs:
```csharp
[Header("Настройки предмета")]
itemName = "Артефакт"    // Название предмета
itemIcon                 // Sprite иконка предмета (назначается в инспекторе)
itemValue = 10          // Ценность предмета в очках
```

### В префабах предметов:
1. Добавить Collider для клика мыши
2. Назначить Sprite в поле itemIcon
3. Установить название и ценность

## 🔧 Структура UI иерархии

```
Canvas
├── UIController (UIController.cs)
│   ├── ScoreText (TextMeshProUGUI)
│   ├── LevelText (TextMeshProUGUI)
│   ├── InventoryCountText (TextMeshProUGUI)
│   └── InventoryPanel (Panel + Grid Layout Group)
│       └── [Динамически создаваемые элементы]
└── InspectionUI (InspectionUI.cs)
    └── InspectionPanel
        ├── UpButton
        ├── DownButton
        ├── LeftButton
        ├── RightButton
        ├── CollectButton
        └── CancelButton
```

## 🎨 Рекомендуемые настройки Grid Layout Group

```
Cell Size: (64, 64) - размер каждой ячейки инвентаря
Spacing: (5, 5) - расстояние между ячейками
Start Corner: Upper Left
Start Axis: Horizontal
Child Alignment: Upper Left
Constraint: Fixed Column Count
Count: 5-8 (количество колонок)
```

## 🚀 Тестирование системы

1. **Запустите сцену**
2. **Нажмите на предмет** - он полетит к центру экрана
3. **Осмотрите предмет** - вращайте мышью или кнопками
4. **Нажмите E или кнопку Collect** - предмет добавится в инвентарь
5. **Проверьте UI** - счет, количество предметов, визуальный инвентарь

## 🐛 Troubleshooting

### Предметы не добавляются в инвентарь:
- Проверьте, что UIController.Instance не null
- Убедитесь, что inventoryGridParent назначен
- Проверьте консоль на ошибки

### UI элементы не отображаются:
- Проверьте ссылки в инспекторе UIController
- Убедитесь, что Canvas настроен правильно
- Проверьте, что Grid Layout Group добавлен

### Предметы выглядят неправильно:
- Назначьте спрайты в поле itemIcon у предметов
- Проверьте настройки preserveAspect = true
- Настройте размеры Cell Size в Grid Layout Group

## 📱 Адаптация для мобильных устройств

Для мобильных устройств рекомендуется:
- Увеличить Cell Size до (80, 80) или больше
- Увеличить размеры кнопок управления
- Добавить touch-friendly элементы управления

## 🔄 Интеграция с существующим UI

Если у вас уже есть UI система:
1. Добавьте UIController как дополнительный компонент
2. Настройте ссылки на существующие UI элементы
3. Используйте UIController.Instance для интеграции