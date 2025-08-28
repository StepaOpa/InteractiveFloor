using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class CollectableItem : MonoBehaviour
{
    // ... (весь код до ExitInspectionMode без изменений) ...

    [Header("Настройки предмета")]
    public string itemName = "Артефакт";
    public Sprite itemIcon;
    public int itemValue = 10;

    [Header("Настройки анимации")]
    [SerializeField] private float flyToPlayerSpeed = 5f;
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float buttonRotationSpeed = 180f;
    [SerializeField] private float buttonRotationStep = 45f;
    [SerializeField] private float inspectionDistance = 1f;
    [SerializeField] private float inspectionScaleModifier = 0.5f;
    [SerializeField] private Vector3 inspectionOffset = new Vector3(0, 0, 0);

    [Header("Настройки полёта")]
    [SerializeField] private AnimationCurve flightCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    [SerializeField] private float arrivalPause = 0.1f;

    [Header("Звуки и эффекты")]
    [SerializeField] private GameObject pickupEffect;

    private Camera playerCamera;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;
    private Collider itemCollider;
    private Rigidbody itemRigidbody;

    private bool isBeingInspected = false;
    private bool isMovingToPlayer = false;
    private bool canBePickedUp = true;

    private static bool isAnyItemBeingInspected = false;

    private Vector2 lastMousePosition;
    private bool isRotating = false;

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

    private void InitializeItem()
    {
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = FindObjectOfType<Camera>();
        }
        
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        originalScale = transform.localScale;
        
        itemCollider = GetComponent<Collider>();
        itemRigidbody = GetComponent<Rigidbody>();

        if (itemCollider == null)
        {
            itemCollider = gameObject.AddComponent<BoxCollider>();
        }

        if (flightCurve == null || flightCurve.length == 0)
        {
            flightCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
        }

        Debug.Log($"[CollectableItem] Предмет '{itemName}' инициализирован");
    }

    void OnMouseDown()
    {
        if (canBePickedUp && !isMovingToPlayer && !isBeingInspected && !isAnyItemBeingInspected)
        {
            PickUpItem();
        }
        else if (isBeingInspected)
        {
            StartMouseRotation();
        }
    }

    private void StartMouseRotation()
    {
        if (!isRotatingByButton)
        {
            isRotating = true;
            lastMousePosition = Input.mousePosition;
        }
    }

    private void HandleInspectionInput()
    {
        if (Input.GetMouseButton(0) && isRotating && !isRotatingByButton)
        {
            Vector2 currentMousePosition = Input.mousePosition;
            Vector2 mouseDelta = currentMousePosition - lastMousePosition;
            float rotationX = -mouseDelta.y * rotationSpeed * Time.deltaTime;
            float rotationY = mouseDelta.x * rotationSpeed * Time.deltaTime;
            transform.Rotate(rotationX, rotationY, 0, Space.World);
            lastMousePosition = currentMousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            isRotating = false;
        }

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
        {
            ExitInspectionMode();
        }

        if (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Return))
        {
            CollectItem();
        }
        
        HandleKeyboardRotation();
    }

    private void HandleKeyboardRotation()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow)) RotateUp();
        if (Input.GetKeyDown(KeyCode.DownArrow)) RotateDown();
        if (Input.GetKeyDown(KeyCode.LeftArrow)) RotateLeft();
        if (Input.GetKeyDown(KeyCode.RightArrow)) RotateRight();
    }

    public void PickUpItem()
    {
        if (!canBePickedUp) return;

        StartCoroutine(FlyToPlayerCoroutine());

        if (SoundManager.Instance != null) SoundManager.Instance.PlayPickupItemSound();
        if (pickupEffect != null) Instantiate(pickupEffect, transform.position, Quaternion.identity);

        canBePickedUp = false;
        isMovingToPlayer = true;
    }

    private IEnumerator FlyToPlayerCoroutine()
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = GetInspectionPosition();
        Vector3 startScale = transform.localScale;
        Vector3 targetScale = originalScale * inspectionScaleModifier;

        float elapsedTime = 0f;
        float flightDuration = Vector3.Distance(startPosition, targetPosition) / flyToPlayerSpeed;

        while (elapsedTime < flightDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / flightDuration;
            float smoothT = flightCurve.Evaluate(t);

            transform.position = Vector3.Lerp(startPosition, targetPosition, smoothT);
            transform.localScale = Vector3.Lerp(startScale, targetScale, smoothT);

            float currentRotationSpeed = 50f * (1f - t * 0.5f);
            transform.Rotate(0, currentRotationSpeed * Time.deltaTime, 0);

            yield return null;
        }

        transform.position = targetPosition;
        transform.localScale = targetScale;

        if (arrivalPause > 0f) yield return new WaitForSeconds(arrivalPause);

        isMovingToPlayer = false;
        EnterInspectionMode();
    }

    private Vector3 GetInspectionPosition()
    {
        if (playerCamera != null)
        {
            return playerCamera.transform.position + playerCamera.transform.forward * inspectionDistance + inspectionOffset;
        }
        return transform.position + Vector3.up * 2f;
    }

    private void EnterInspectionMode()
    {
        isBeingInspected = true;
        isAnyItemBeingInspected = true;

        if (itemRigidbody != null)
        {
            itemRigidbody.linearVelocity = Vector3.zero;
            itemRigidbody.angularVelocity = Vector3.zero;
        }

        if (InspectionUI.Instance != null)
        {
            InspectionUI.Instance.ShowInspectionUI(this);
        }
    }

    private void ExitInspectionMode()
    {
        Debug.Log($"[CollectableItem] Выход из режима осмотра: {itemName}");

        isBeingInspected = false;
        isAnyItemBeingInspected = false;
        isRotating = false;

        if (currentRotationCoroutine != null)
        {
            StopCoroutine(currentRotationCoroutine);
            currentRotationCoroutine = null;
            isRotatingByButton = false;
        }

        if (InspectionUI.Instance != null)
        {
            InspectionUI.Instance.HideInspectionUI();
        }

        // <-- ИЗМЕНЕНИЕ 1: Добавляем звук выброса мусора
        if (itemValue < 0)
        {
            Debug.Log($"[CollectableItem] Мусор '{itemName}' был выброшен и уничтожен.");
            // Проигрываем звук и здесь
            if (SoundManager.Instance != null) SoundManager.Instance.PlayItemDropSound();
            Destroy(gameObject);
        }
        else
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlayItemDropSound();
            StartCoroutine(ReturnToOriginalPositionCoroutine());
        }
    }

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

        transform.position = originalPosition;
        transform.rotation = originalRotation;
        transform.localScale = originalScale;

        if (itemRigidbody != null)
        {
            itemRigidbody.isKinematic = false;
        }
        canBePickedUp = true;
    }

    private void CollectItem()
    {
        // <-- ВАЖНО: Мы сбрасываем флаг здесь, чтобы избежать его "залипания", если игра закончится
        isAnyItemBeingInspected = false;

        if (currentRotationCoroutine != null)
        {
            StopCoroutine(currentRotationCoroutine);
            currentRotationCoroutine = null;
            isRotatingByButton = false;
        }

        if (InspectionUI.Instance != null)
        {
            InspectionUI.Instance.HideInspectionUI();
        }

        if (SoundManager.Instance != null)
        {
            if (itemValue > 0)
            {
                SoundManager.Instance.PlayInventoryAddSound();
            }
            else
            {
                SoundManager.Instance.PlayErrorSound();
            }
        }

        if (UIController.Instance != null)
        {
            UIController.Instance.AddItemToInventory(itemName, itemIcon, itemValue);
        }
        else
        {
            Debug.LogWarning($"[CollectableItem] UIController.Instance не найден! Предмет '{itemName}' собран, но не добавлен в UI");
        }

        Destroy(gameObject);
    }
    
    // <-- ИЗМЕНЕНИЕ 3: Новый метод для сброса статического флага извне
    public static void ResetInspectionFlag()
    {
        isAnyItemBeingInspected = false;
        Debug.LogWarning("[CollectableItem] Статический флаг isAnyItemBeingInspected был принудительно сброшен.");
    }

    public bool IsBeingInspected()
    {
        return isBeingInspected;
    }

    // ... (весь остальной код без изменений) ...
    public void RotateUp() { if (isBeingInspected && !isRotatingByButton) StartSmoothRotation(new Vector3(-buttonRotationStep, 0, 0), "вверх"); }
    public void RotateDown() { if (isBeingInspected && !isRotatingByButton) StartSmoothRotation(new Vector3(buttonRotationStep, 0, 0), "вниз"); }
    public void RotateLeft() { if (isBeingInspected && !isRotatingByButton) StartSmoothRotation(new Vector3(0, -buttonRotationStep, 0), "влево"); }
    public void RotateRight() { if (isBeingInspected && !isRotatingByButton) StartSmoothRotation(new Vector3(0, buttonRotationStep, 0), "вправо"); }
    public void RotateByButton(string direction) { Vector3 targetRotation; switch (direction.ToLower()) { case "up": targetRotation = new Vector3(-buttonRotationStep, 0, 0); break; case "down": targetRotation = new Vector3(buttonRotationStep, 0, 0); break; case "left": targetRotation = new Vector3(0, -buttonRotationStep, 0); break; case "right": targetRotation = new Vector3(0, buttonRotationStep, 0); break; default: return; } StartSmoothRotation(targetRotation, direction); }
    private void StartSmoothRotation(Vector3 deltaRotation, string directionName) { if (currentRotationCoroutine != null) StopCoroutine(currentRotationCoroutine); currentRotationCoroutine = StartCoroutine(SmoothRotationCoroutine(deltaRotation, directionName)); }
    private IEnumerator SmoothRotationCoroutine(Vector3 deltaRotation, string directionName) { isRotatingByButton = true; Quaternion startRotation = transform.rotation; Quaternion targetRotation = startRotation * Quaternion.Euler(deltaRotation); float elapsedTime = 0f; float rotationDuration = buttonRotationStep / buttonRotationSpeed; while (elapsedTime < rotationDuration) { elapsedTime += Time.deltaTime; float t = Mathf.SmoothStep(0f, 1f, elapsedTime / rotationDuration); transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t); yield return null; } transform.rotation = targetRotation; isRotatingByButton = false; currentRotationCoroutine = null; }
    public void CollectItemPublic() { if (isBeingInspected) CollectItem(); }
    public void ExitInspectionPublic() { if (isBeingInspected) ExitInspectionMode(); }
}