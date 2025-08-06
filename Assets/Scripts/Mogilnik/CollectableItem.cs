using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class CollectableItem : MonoBehaviour
{
    [Header("Настройки предмета")]
    public string itemName = "Артефакт";
    public Sprite itemIcon;
    public int itemValue = 10;
    
    [Header("Настройки анимации")]
    [SerializeField] private float flyToPlayerSpeed = 5f;
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float buttonRotationSpeed = 180f; // Скорость плавного вращения кнопками (градусов в секунду)
    [SerializeField] private float buttonRotationStep = 45f; // Шаг поворота кнопками (градусы)
    [SerializeField] private float inspectionDistance = 2f;
    [SerializeField] private Vector3 inspectionOffset = new Vector3(0, 0, 0);
    
    [Header("Настройки полёта")]
    [SerializeField] private AnimationCurve flightCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f); // Кривая полёта
    [SerializeField] private float arrivalPause = 0.1f; // Пауза перед входом в режим осмотра
    
    [Header("Звуки и эффекты")]
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] private GameObject pickupEffect;
    
    // Компоненты
    private Camera playerCamera;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;
    private Collider itemCollider;
    private Rigidbody itemRigidbody;
    
    // Состояния
    private bool isBeingInspected = false;
    private bool isMovingToPlayer = false;
    private bool canBePickedUp = true;
    
    // Вращение для осмотра
    private Vector2 lastMousePosition;
    private bool isRotating = false;
    
    // Плавное вращение кнопками
    private bool isRotatingByButton = false;
    private Coroutine currentRotationCoroutine = null;
    
    void Start()
    {
        InitializeItem();
    }
    
    void Update()
    {
        if (isBeingInspected)
        {
            HandleInspectionInput();
        }
    }
    
    /// <summary>
    /// Инициализация предмета
    /// </summary>
    private void InitializeItem()
    {
        // Находим камеру игрока
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = FindObjectOfType<Camera>();
        }
        
        // Сохраняем исходные параметры
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;
        
        // Получаем компоненты
        itemCollider = GetComponent<Collider>();
        itemRigidbody = GetComponent<Rigidbody>();
        
        // Добавляем коллайдер если его нет
        if (itemCollider == null)
        {
            itemCollider = gameObject.AddComponent<BoxCollider>();
        }
        
        // Инициализируем кривую полёта по умолчанию если она не задана
        if (flightCurve == null || flightCurve.length == 0)
        {
            flightCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
            Debug.Log($"[CollectableItem] Создана кривая полёта по умолчанию для '{itemName}'");
        }
        
        Debug.Log($"[CollectableItem] Предмет '{itemName}' инициализирован");
    }
    
    /// <summary>
    /// Обработка клика по предмету
    /// </summary>
    void OnMouseDown()
    {
        if (canBePickedUp && !isMovingToPlayer && !isBeingInspected)
        {
            PickUpItem();
        }
        else if (isBeingInspected)
        {
            StartRotation();
        }
    }
    
    /// <summary>
    /// Начало поворота предмета мышью
    /// </summary>
    private void StartRotation()
    {
        // Не начинаем мышиное вращение если идет анимация кнопок
        if (!isRotatingByButton)
        {
            isRotating = true;
            lastMousePosition = Input.mousePosition;
        }
    }
    
    /// <summary>
    /// Обработка ввода для осмотра предмета
    /// </summary>
    private void HandleInspectionInput()
    {
        // Вращение мышью (только если не идет вращение кнопками)
        if (Input.GetMouseButton(0) && isRotating && !isRotatingByButton)
        {
            Vector2 currentMousePosition = Input.mousePosition;
            Vector2 mouseDelta = currentMousePosition - lastMousePosition;
            
            // Вращаем предмет по осям X и Y
            float rotationX = -mouseDelta.y * rotationSpeed * Time.deltaTime;
            float rotationY = mouseDelta.x * rotationSpeed * Time.deltaTime;
            
            transform.Rotate(rotationX, rotationY, 0, Space.World);
            
            lastMousePosition = currentMousePosition;
        }
        
        if (Input.GetMouseButtonUp(0))
        {
            isRotating = false;
        }
        
        // Выход из режима осмотра (клавиша Escape или правая кнопка мыши)
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
        {
            ExitInspectionMode();
        }
        
        // Подтверждение сбора (клавиша E или Enter)
        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return))
        {
            CollectItem();
        }
        
        // Вращение стрелками клавиатуры
        HandleKeyboardRotation();
    }
    
    /// <summary>
    /// Обработка вращения через клавиши клавиатуры
    /// </summary>
    private void HandleKeyboardRotation()
    {
        // Используем GetKeyDown для однократного срабатывания (как кнопки UI)
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            RotateUp();
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            RotateDown();
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            RotateLeft();
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            RotateRight();
        }
    }
    
    /// <summary>
    /// Подбор предмета (начало анимации полета к игроку)
    /// </summary>
    public void PickUpItem()
    {
        if (!canBePickedUp) return;
        
        Debug.Log($"[CollectableItem] Подбираем предмет: {itemName}");
        
        // Отключаем физику
        if (itemRigidbody != null)
        {
            itemRigidbody.isKinematic = true;
        }
        
        // Запускаем анимацию полета к игроку
        StartCoroutine(FlyToPlayerCoroutine());
        
        // Воспроизводим звук подбора
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }
        
        // Создаем эффект подбора
        if (pickupEffect != null)
        {
            Instantiate(pickupEffect, transform.position, Quaternion.identity);
        }
        
        canBePickedUp = false;
        isMovingToPlayer = true;
    }
    
    /// <summary>
    /// Корутина для плавной анимации полета предмета к игроку
    /// </summary>
    private IEnumerator FlyToPlayerCoroutine()
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = GetInspectionPosition();
        Quaternion startRotation = transform.rotation;
        
        float elapsedTime = 0f;
        float flightDuration = Vector3.Distance(startPosition, targetPosition) / flyToPlayerSpeed;
        
        Debug.Log($"[CollectableItem] Начинаем полёт на расстояние {Vector3.Distance(startPosition, targetPosition):F2}м за {flightDuration:F2}с");
        
        while (elapsedTime < flightDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / flightDuration;
            
            // Используем настраиваемую кривую анимации для движения
            float smoothT = flightCurve.Evaluate(t);
            
            // Плавная интерполяция позиции
            transform.position = Vector3.Lerp(startPosition, targetPosition, smoothT);
            
            // Плавное вращение во время полета с замедлением
            float currentRotationSpeed = 50f * (1f - t * 0.5f); // Замедляем вращение к концу
            transform.Rotate(0, currentRotationSpeed * Time.deltaTime, 0);
            
            yield return null;
        }
        
        // Плавно устанавливаем финальную позицию
        transform.position = targetPosition;
        
        // Настраиваемая пауза для визуального эффекта прибытия
        if (arrivalPause > 0f)
        {
            yield return new WaitForSeconds(arrivalPause);
        }
        
        isMovingToPlayer = false;
        EnterInspectionMode();
        
        Debug.Log($"[CollectableItem] ✓ Полёт завершён плавно");
    }
    
    /// <summary>
    /// Получить позицию для осмотра предмета
    /// </summary>
    private Vector3 GetInspectionPosition()
    {
        if (playerCamera != null)
        {
            return playerCamera.transform.position + 
                   playerCamera.transform.forward * inspectionDistance + 
                   inspectionOffset;
        }
        
        return transform.position + Vector3.up * 2f;
    }
    
    /// <summary>
    /// Вход в режим осмотра предмета
    /// </summary>
    private void EnterInspectionMode()
    {
        isBeingInspected = true;
        Debug.Log($"[CollectableItem] Вход в режим осмотра предмета: {itemName}");
        
        // Увеличиваем предмет для лучшего осмотра
        transform.localScale = originalScale * 1.5f;
        
        // Останавливаем все движения
        if (itemRigidbody != null)
        {
            itemRigidbody.linearVelocity = Vector3.zero;
            itemRigidbody.angularVelocity = Vector3.zero;
        }
        
        // Показываем подсказку
        Debug.Log("[CollectableItem] === РЕЖИМ ОСМОТРА ===");
        Debug.Log("[CollectableItem] Управление:");
        Debug.Log("[CollectableItem] • Мышь: Зажмите ЛКМ и вращайте плавно");
        Debug.Log($"[CollectableItem] • Клавиши/Кнопки: ↑↓←→ плавные повороты по {buttonRotationStep}°");
        Debug.Log("[CollectableItem] • E/Enter: Подобрать предмет");
        Debug.Log("[CollectableItem] • Escape/ПКМ: Отменить осмотр");
        
        // Показываем UI кнопки (если InspectionUI существует)
        if (InspectionUI.Instance != null)
        {
            InspectionUI.Instance.ShowInspectionUI(this);
        }
    }
    
    /// <summary>
    /// Выход из режима осмотра
    /// </summary>
    private void ExitInspectionMode()
    {
        Debug.Log($"[CollectableItem] Выход из режима осмотра: {itemName}");
        
        isBeingInspected = false;
        isRotating = false;
        
        // Останавливаем плавное вращение кнопками если оно идет
        if (currentRotationCoroutine != null)
        {
            StopCoroutine(currentRotationCoroutine);
            currentRotationCoroutine = null;
            isRotatingByButton = false;
        }
        
        // Скрываем UI кнопки (если InspectionUI существует)
        if (InspectionUI.Instance != null)
        {
            InspectionUI.Instance.HideInspectionUI();
        }
        
        // Возвращаем предмет на исходное место
        StartCoroutine(ReturnToOriginalPositionCoroutine());
    }
    
    /// <summary>
    /// Возврат предмета на исходное место
    /// </summary>
    private IEnumerator ReturnToOriginalPositionCoroutine()
    {
        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        Vector3 startScale = transform.localScale;
        
        float elapsedTime = 0f;
        float returnDuration = 1f;
        
        while (elapsedTime < returnDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / returnDuration;
            
            transform.position = Vector3.Lerp(startPosition, originalPosition, t);
            transform.rotation = Quaternion.Lerp(startRotation, originalRotation, t);
            transform.localScale = Vector3.Lerp(startScale, originalScale, t);
            
            yield return null;
        }
        
        // Восстанавливаем параметры
        transform.position = originalPosition;
        transform.rotation = originalRotation;
        transform.localScale = originalScale;
        
        // Включаем физику обратно
        if (itemRigidbody != null)
        {
            itemRigidbody.isKinematic = false;
        }
        
        canBePickedUp = true;
    }
    
    /// <summary>
    /// Окончательный сбор предмета в инвентарь
    /// </summary>
    private void CollectItem()
    {
        Debug.Log($"[CollectableItem] Предмет '{itemName}' собран в инвентарь!");
        
        // Останавливаем плавное вращение кнопками если оно идет
        if (currentRotationCoroutine != null)
        {
            StopCoroutine(currentRotationCoroutine);
            currentRotationCoroutine = null;
            isRotatingByButton = false;
        }
        
        // Скрываем UI кнопки (если InspectionUI существует)
        if (InspectionUI.Instance != null)
        {
            InspectionUI.Instance.HideInspectionUI();
        }
        
        // Добавляем в инвентарь через UIController
        if (UIController.Instance != null)
        {
            UIController.Instance.AddItemToInventory(itemName, itemIcon, itemValue);
            Debug.Log($"[CollectableItem] ✓ Предмет '{itemName}' добавлен в визуальный инвентарь!");
        }
        else
        {
            Debug.LogWarning($"[CollectableItem] UIController.Instance не найден! Предмет '{itemName}' собран, но не добавлен в UI");
        }
        
        // Уничтожаем предмет
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Проверка, можно ли подобрать предмет
    /// </summary>
    public bool CanBePickedUp()
    {
        return canBePickedUp && !isMovingToPlayer && !isBeingInspected;
    }
    
    /// <summary>
    /// Получить состояние предмета
    /// </summary>
    public bool IsBeingInspected()
    {
        return isBeingInspected;
    }
    
    // ======= ПУБЛИЧНЫЕ МЕТОДЫ ДЛЯ UI КНОПОК =======
    
    /// <summary>
    /// Плавное вращение вверх (по оси X отрицательное)
    /// </summary>
    public void RotateUp()
    {
        if (isBeingInspected && !isRotatingByButton)
        {
            Vector3 targetRotation = new Vector3(-buttonRotationStep, 0, 0);
            StartSmoothRotation(targetRotation, "вверх");
        }
    }
    
    /// <summary>
    /// Плавное вращение вниз (по оси X положительное)
    /// </summary>
    public void RotateDown()
    {
        if (isBeingInspected && !isRotatingByButton)
        {
            Vector3 targetRotation = new Vector3(buttonRotationStep, 0, 0);
            StartSmoothRotation(targetRotation, "вниз");
        }
    }
    
    /// <summary>
    /// Плавное вращение влево (по оси Y отрицательное)
    /// </summary>
    public void RotateLeft()
    {
        if (isBeingInspected && !isRotatingByButton)
        {
            Vector3 targetRotation = new Vector3(0, -buttonRotationStep, 0);
            StartSmoothRotation(targetRotation, "влево");
        }
    }
    
    /// <summary>
    /// Плавное вращение вправо (по оси Y положительное)
    /// </summary>
    public void RotateRight()
    {
        if (isBeingInspected && !isRotatingByButton)
        {
            Vector3 targetRotation = new Vector3(0, buttonRotationStep, 0);
            StartSmoothRotation(targetRotation, "вправо");
        }
    }
    
    /// <summary>
    /// Универсальный метод для вращения кнопками (для UI)
    /// Вызывается однократно при нажатии кнопки
    /// </summary>
    /// <param name="direction">Направление: "up", "down", "left", "right"</param>
    public void RotateByButton(string direction)
    {
        if (!isBeingInspected || isRotatingByButton) return;
        
        Vector3 targetRotation = Vector3.zero;
        
        switch (direction.ToLower())
        {
            case "up":
                targetRotation = new Vector3(-buttonRotationStep, 0, 0);
                break;
            case "down":
                targetRotation = new Vector3(buttonRotationStep, 0, 0);
                break;
            case "left":
                targetRotation = new Vector3(0, -buttonRotationStep, 0);
                break;
            case "right":
                targetRotation = new Vector3(0, buttonRotationStep, 0);
                break;
            default:
                Debug.LogWarning($"[CollectableItem] Неизвестное направление вращения: {direction}");
                return;
        }
        
        StartSmoothRotation(targetRotation, direction);
    }
    
    /// <summary>
    /// Начать плавное вращение к целевому углу
    /// </summary>
    /// <param name="deltaRotation">Изменение поворота в градусах</param>
    /// <param name="directionName">Название направления для лога</param>
    private void StartSmoothRotation(Vector3 deltaRotation, string directionName)
    {
        // Останавливаем предыдущее вращение если оно есть
        if (currentRotationCoroutine != null)
        {
            StopCoroutine(currentRotationCoroutine);
        }
        
        // Запускаем новое плавное вращение
        currentRotationCoroutine = StartCoroutine(SmoothRotationCoroutine(deltaRotation, directionName));
    }
    
    /// <summary>
    /// Корутина для плавного вращения предмета
    /// </summary>
    /// <param name="deltaRotation">Изменение поворота в градусах</param>
    /// <param name="directionName">Название направления для лога</param>
    private IEnumerator SmoothRotationCoroutine(Vector3 deltaRotation, string directionName)
    {
        isRotatingByButton = true;
        
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = startRotation * Quaternion.Euler(deltaRotation);
        
        float elapsedTime = 0f;
        float rotationDuration = buttonRotationStep / buttonRotationSpeed; // Время основано на скорости
        
        Debug.Log($"[CollectableItem] Начинаем плавное вращение {directionName} на {buttonRotationStep}° за {rotationDuration:F2}с");
        
        while (elapsedTime < rotationDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / rotationDuration;
            
            // Используем плавную интерполяцию
            t = Mathf.SmoothStep(0f, 1f, t);
            
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
            
            yield return null;
        }
        
        // Убеждаемся, что достигли точной целевой позиции
        transform.rotation = targetRotation;
        
        isRotatingByButton = false;
        currentRotationCoroutine = null;
        
        Debug.Log($"[CollectableItem] ✓ Плавное вращение {directionName} завершено");
    }
    
    // ======= ПУБЛИЧНЫЕ МЕТОДЫ ДЛЯ UI КНОПОК (ДЕЙСТВИЯ) =======
    
    /// <summary>
    /// Публичный метод для сбора предмета (для UI кнопки)
    /// </summary>
    public void CollectItemPublic()
    {
        if (isBeingInspected)
        {
            CollectItem();
        }
        else
        {
            Debug.LogWarning("[CollectableItem] Нельзя собрать предмет - не в режиме осмотра!");
        }
    }
    
    /// <summary>
    /// Публичный метод для выхода из режима осмотра (для UI кнопки)
    /// </summary>
    public void ExitInspectionPublic()
    {
        if (isBeingInspected)
        {
            ExitInspectionMode();
        }
        else
        {
            Debug.LogWarning("[CollectableItem] Нельзя выйти из режима осмотра - предмет не осматривается!");
        }
    }
}