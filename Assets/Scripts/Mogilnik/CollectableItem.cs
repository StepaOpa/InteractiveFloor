using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class CollectableItem : MonoBehaviour
{
    // =========================================================================
    // ПОЛЯ И НАСТРОЙКИ
    // =========================================================================

    [Header("Настройки предмета")]
    public string itemName = "Артефакт";
    public Sprite itemIcon;
    public int itemValue = 10;

    [Header("Настройки очистки")]
    [Tooltip("Перетащи сюда дочерний объект с 'шубой' из грязи.")]
    [SerializeField] private DirtCoat dirtCoat;

    // --- НОВОЕ: Настройки для анимации кисти и пыли ---
    [Header("Анимация очистки")]
    [SerializeField] private GameObject brushPrefab;
    [Tooltip("Насколько выше самой высокой точки предмета появится кисть")]
    [SerializeField] private float brushVerticalPadding = 0.1f;
    [SerializeField] private float brushSwipeSpeed = 2f;
    [SerializeField] private float brushSwipeDistance = 0.2f;
    [SerializeField] private int brushSwipeCount = 1; // Обычно достаточно одного взмаха на тап
    [SerializeField] private Vector3 brushAnimationRotation = new Vector3(60f, 70f, 0f);

    [Header("Настройки осмотра")]
    [SerializeField] private float flyToPlayerSpeed = 5f;
    [SerializeField] private float rotationSpeed = 100f;
    [SerializeField] private float inspectionDistance = 1f;
    [SerializeField] private float inspectionScaleModifier = 0.5f;
    [SerializeField] private Vector3 inspectionOffset = new Vector3(0, 0, 0);

    [Header("Вращение кнопками")]
    [SerializeField] private float buttonRotationSpeed = 180f;
    [SerializeField] private float buttonRotationStep = 45f;

    // --- Приватные переменные ---
    private Camera playerCamera;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Vector3 originalScale;
    private Collider itemCollider;
    private bool isBeingInspected = false;
    private static bool isAnyItemBeingInspected = false;
    private bool isRotatingByButton = false;
    private Coroutine currentRotationCoroutine = null;

    // --- ВОЗВРАЩАЕМ ЭТИ ПЕРЕМЕННЫЕ ---
    private bool isCleaning = false; // Флаг, что сейчас идет анимация очистки
    private bool isClean = false;    // Флаг, что предмет полностью чист

    // =========================================================================
    // МЕТОДЫ ЖИЗНЕННОГО ЦИКЛА UNITY
    // =========================================================================

    void Start()
    {
        InitializeItem();
        // Если "шубы" изначально нет, предмет считается чистым
        if (dirtCoat == null)
        {
            isClean = true;
        }
    }

    // --- ИЗМЕНЕНО: Логика клика мыши ---
    void OnMouseDown()
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        // Если начинаем осмотр
        if (!isBeingInspected && !isAnyItemBeingInspected)
        {
            StartCoroutine(FlyToPlayerCoroutine());
        }
        // Если тапаем для очистки: предмет осматривается, он не чистый, и анимация ЕЩЕ не идет
        else if (isBeingInspected && !isClean && dirtCoat != null && !isCleaning)
        {
            // Запускаем корутину, которая сначала покажет анимацию, а потом обновит "шубу"
            StartCoroutine(AnimateAndCleanCoroutine());
        }
    }

    // =========================================================================
    // ПРИВАТНЫЕ МЕТОДЫ И ЛОГИКА
    // =========================================================================

    // ... (остальные методы, такие как InitializeItem, RotateByButton, CollectItem и т.д. остаются без изменений) ...

    private void InitializeItem() { playerCamera = Camera.main; originalPosition = transform.position; originalRotation = transform.rotation; originalScale = transform.localScale; itemCollider = GetComponent<Collider>(); if (GetComponent<MeshRenderer>() == null) { UnityEngine.Debug.LogError($"На предмете '{itemName}' отсутствует MeshRenderer!"); } }
    public void RotateByButton(string direction) { if (isCleaning || isRotatingByButton) return; Vector3 targetRotation; switch (direction.ToLower()) { case "up": targetRotation = new Vector3(-buttonRotationStep, 0, 0); break; case "down": targetRotation = new Vector3(buttonRotationStep, 0, 0); break; case "left": targetRotation = new Vector3(0, -buttonRotationStep, 0); break; case "right": targetRotation = new Vector3(0, buttonRotationStep, 0); break; default: return; } StartSmoothRotation(targetRotation); }
    public void ExitInspectionPublic() { if (isBeingInspected && !isCleaning && !isRotatingByButton) ExitInspectionMode(); }
    public void CollectItemPublic() { if (isBeingInspected && !isCleaning && !isRotatingByButton) CollectItem(); }
    public static void ResetInspectionFlag() { isAnyItemBeingInspected = false; }
    public bool IsBeingInspected() { return isBeingInspected; }
    private void EnterInspectionMode() { isBeingInspected = true; isAnyItemBeingInspected = true; if (InspectionUI.Instance != null) { InspectionUI.Instance.ShowInspectionUI(this); } }
    private void ExitInspectionMode() { isBeingInspected = false; isAnyItemBeingInspected = false; if (InspectionUI.Instance != null) { InspectionUI.Instance.HideInspectionUI(); } if (itemValue < 0) { if (SoundManager.Instance != null) SoundManager.Instance.PlayItemDropSound(); Destroy(gameObject); } else { if (SoundManager.Instance != null) SoundManager.Instance.PlayItemDropSound(); StartCoroutine(ReturnToOriginalPositionCoroutine()); } }
    private void CollectItem() { isAnyItemBeingInspected = false; if (InspectionUI.Instance != null) { InspectionUI.Instance.HideInspectionUI(); } if (SoundManager.Instance != null) { if (itemValue > 0) SoundManager.Instance.PlayInventoryAddSound(); else SoundManager.Instance.PlayErrorSound(); } if (UIController.Instance != null) { UIController.Instance.AddItemToInventory(itemName, itemIcon, itemValue); } Destroy(gameObject); }
    private void StartSmoothRotation(Vector3 deltaRotation) { if (currentRotationCoroutine != null) StopCoroutine(currentRotationCoroutine); currentRotationCoroutine = StartCoroutine(SmoothRotationCoroutine(deltaRotation)); }
    private IEnumerator FlyToPlayerCoroutine() { Vector3 startPosition = transform.position; Vector3 targetPosition = playerCamera.transform.position + playerCamera.transform.forward * inspectionDistance + inspectionOffset; Vector3 startScale = transform.localScale; Vector3 targetScale = originalScale * inspectionScaleModifier; float elapsedTime = 0f; float flightDuration = Vector3.Distance(startPosition, targetPosition) / flyToPlayerSpeed; while (elapsedTime < flightDuration) { elapsedTime += Time.deltaTime; float t = elapsedTime / flightDuration; transform.position = Vector3.Lerp(startPosition, targetPosition, t); transform.localScale = Vector3.Lerp(startScale, targetScale, t); yield return null; } transform.position = targetPosition; transform.localScale = targetScale; EnterInspectionMode(); }
    private IEnumerator ReturnToOriginalPositionCoroutine() { Vector3 startPosition = transform.position; Quaternion startRotation = transform.rotation; Vector3 startScale = transform.localScale; float elapsedTime = 0f; float returnDuration = 1f; while (elapsedTime < returnDuration) { elapsedTime += Time.deltaTime; float t = elapsedTime / returnDuration; transform.position = Vector3.Lerp(startPosition, originalPosition, t); transform.rotation = Quaternion.Lerp(startRotation, originalRotation, t); transform.localScale = Vector3.Lerp(startScale, originalScale, t); yield return null; } transform.position = originalPosition; transform.rotation = originalRotation; transform.localScale = originalScale; }
    private IEnumerator SmoothRotationCoroutine(Vector3 deltaRotation) { isRotatingByButton = true; Quaternion startRotation = transform.rotation; Quaternion targetRotation = startRotation * Quaternion.Euler(deltaRotation); float elapsedTime = 0f; float rotationDuration = buttonRotationStep / buttonRotationSpeed; while (elapsedTime < rotationDuration) { elapsedTime += Time.deltaTime; float t = Mathf.SmoothStep(0f, 1f, elapsedTime / rotationDuration); transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t); yield return null; } transform.rotation = targetRotation; isRotatingByButton = false; currentRotationCoroutine = null; }

    // =========================================================================
    // КОРУТИНЫ (АСИНХРОННЫЕ ОПЕРАЦИИ)
    // =========================================================================

    // --- НОВАЯ ОБЪЕДИНЕННАЯ КОРУТИНА ---
    private IEnumerator AnimateAndCleanCoroutine()
    {
        // 1. Ставим флаг, чтобы нельзя было кликнуть еще раз, пока идет анимация
        isCleaning = true;

        // 2. Создаем эффекты
        if (InspectionUI.Instance != null) { InspectionUI.Instance.CreateDustEffect(this.transform, transform.position); }
        if (brushPrefab == null) { UnityEngine.Debug.LogError("Префаб кисточки (Brush Prefab) не назначен в инспекторе!"); isCleaning = false; yield break; }

        GameObject brushInstance = Instantiate(brushPrefab);

        // 3. Рассчитываем позицию и поворот кисти
        float brushY = transform.position.y + GetComponent<Collider>().bounds.extents.y + brushVerticalPadding;
        Vector3 brushStartPosition = new Vector3(transform.position.x, brushY, transform.position.z);
        brushInstance.transform.position = brushStartPosition;
        Quaternion cameraFacingRotation = Quaternion.LookRotation(playerCamera.transform.forward);
        Quaternion desiredTilt = Quaternion.Euler(brushAnimationRotation);
        brushInstance.transform.rotation = cameraFacingRotation * desiredTilt;

        // 4. Проигрываем анимацию взмахов
        Vector3 swipeDirection = playerCamera.transform.right;
        Vector3 centerPos = brushInstance.transform.position;
        Vector3 leftPos = centerPos - swipeDirection * brushSwipeDistance;
        Vector3 rightPos = centerPos + swipeDirection * brushSwipeDistance;

        for (int i = 0; i < brushSwipeCount; i++)
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlayBrushSwipeSound();
            if (InspectionUI.Instance != null) InspectionUI.Instance.PlayDustEffect();

            yield return StartCoroutine(MoveBrush(brushInstance.transform, leftPos, rightPos, brushSwipeSpeed));

            if (SoundManager.Instance != null) SoundManager.Instance.PlayBrushSwipeSound();

            yield return StartCoroutine(MoveBrush(brushInstance.transform, rightPos, leftPos, brushSwipeSpeed));

            if (InspectionUI.Instance != null) InspectionUI.Instance.StopDustEffect();
        }

        // 5. Уничтожаем временные объекты
        Destroy(brushInstance);
        if (InspectionUI.Instance != null) { InspectionUI.Instance.DestroyDustEffect(); }

        // 6. ----> ВОТ ГЛАВНАЯ ЧАСТЬ <----
        // ТОЛЬКО ПОСЛЕ ЗАВЕРШЕНИЯ АНИМАЦИИ мы регистрируем тап в "шубе"
        bool wasFullyCleaned = dirtCoat.RegisterTap();
        if (wasFullyCleaned)
        {
            isClean = true; // Отмечаем, что предмет теперь чистый
        }

        // 7. Снимаем флаг, разрешая следующий тап
        isCleaning = false;
    }

    private IEnumerator MoveBrush(Transform brushTransform, Vector3 from, Vector3 to, float speed)
    {
        float progress = 0f;
        while (progress < 1f)
        {
            if (brushTransform == null) yield break;
            progress += Time.deltaTime * speed;
            brushTransform.position = Vector3.Lerp(from, to, progress);
            yield return null;
        }
        if (brushTransform != null) brushTransform.position = to;
    }
}