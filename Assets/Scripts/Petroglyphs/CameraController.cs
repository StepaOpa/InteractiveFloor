using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform defaultCameraPosition;
    [SerializeField] private Transform defaultCameraLookAtPosition;

    [Header("Настройки лупы")]
    [SerializeField] private RectTransform magnifyingGlassRect;
    private float initialMagnifyingGlassScale;
    private float initialCameraDistance;

    private List<InteractionPoint> allInteractionPoints;

    void Start()
    {
        if (magnifyingGlassRect != null)
        {
            initialMagnifyingGlassScale = magnifyingGlassRect.localScale.x;
            initialCameraDistance = Vector3.Distance(transform.position, defaultCameraLookAtPosition.position);
        }

        allInteractionPoints = new List<InteractionPoint>(FindObjectsOfType<InteractionPoint>());
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            HandleInteractionClick();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(MoveCameraToDefaultPosition());
        }

        UpdateMagnifyingGlassScale();
    }

    // --- ВРЕМЕННЫЙ МЕТОД ДЛЯ ДИАГНОСТИКИ ---
    private void HandleInteractionClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            // Сначала проверяем, не был ли клик по UI
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return; // Если да, то ничего не делаем
            }

            // Если клик был не по UI, ищем точку интереса
            InteractionPoint interactionPoint = hit.collider.gameObject.GetComponentInParent<InteractionPoint>();
            if (interactionPoint != null)
            {
                interactionPoint.OnClick();
                MoveCameraToInteractionPoint(interactionPoint);
            }
        }
    }

    // --- ИСПРАВЛЕННЫЙ МЕТОД ---
    private void MoveCameraToInteractionPoint(InteractionPoint interactionPoint)
    {
        // ШАГ 1: Сначала получаем всю необходимую информацию из объекта, пока он еще активен.
        Vector3 targetPosition = interactionPoint.cameraPosition.position;
        Vector3 targetLookAtPosition = interactionPoint.transform.position;

        // ШАГ 2: Теперь, когда у нас есть все данные, можно безопасно спрятать точки.
        SetInteractionPointsVisibility(false);

        // ШАГ 3: Запускаем движение камеры, используя уже сохраненные данные.
        StartCoroutine(MoveCameraToPointCoroutine(targetPosition, targetLookAtPosition));
    }
    // --- КОНЕЦ ИСПРАВЛЕНИЯ ---

    private IEnumerator MoveCameraToPointCoroutine(Vector3 targetPosition, Vector3 targetLookAtPosition)
    {
        float duration = 1.0f;
        float elapsed = 0f;

        Vector3 startPosition = transform.position;
        Quaternion startRotation = transform.rotation;
        Quaternion targetRotation = Quaternion.LookRotation(targetLookAtPosition - targetPosition);

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t);

            transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPosition;
        transform.rotation = targetRotation;
    }

    private void UpdateMagnifyingGlassScale()
    {
        if (magnifyingGlassRect == null) return;

        Ray ray = new Ray(transform.position, transform.forward);
        Plane plane = new Plane(Vector3.up, defaultCameraLookAtPosition.position);
        float distance;
        if (plane.Raycast(ray, out distance))
        {
            float scaleMultiplier = distance / initialCameraDistance;
            magnifyingGlassRect.localScale = Vector3.one * initialMagnifyingGlassScale * scaleMultiplier;
        }
    }

    public IEnumerator MoveCameraToDefaultPosition()
    {
        SetInteractionPointsVisibility(true);

        Vector3 targetPosition = defaultCameraPosition.position;
        Vector3 targetLookAtPosition = defaultCameraLookAtPosition.position;

        yield return MoveCameraToPointCoroutine(targetPosition, targetLookAtPosition);
    }

    private void SetInteractionPointsVisibility(bool isVisible)
    {
        foreach (InteractionPoint point in allInteractionPoints)
        {
            if (point != null)
            {
                point.gameObject.SetActive(isVisible);
            }
        }
    }
}