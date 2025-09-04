using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform defaultCameraPosition;
    [SerializeField] private Transform defaultCameraLookAtPosition;

    [Header("Настройки лупы")]
    [SerializeField] private RectTransform magnifyingGlassRect;

    [Header("Анимация лупы")]
    [Tooltip("Во сколько раз увеличится лупа при приближении")]
    [SerializeField] private float zoomInMagnifierMultiplier = 1.5f;
    [Tooltip("За сколько секунд произойдет анимация увеличения/уменьшения")]
    [SerializeField] private float magnifierScaleDuration = 0.5f;

    private float initialCameraDistance;
    private List<InteractionPoint> allInteractionPoints;

    private float initialMagnifierScale;
    private bool isDynamicScalingActive = true;

    void Start()
    {
        if (magnifyingGlassRect != null)
        {
            initialMagnifierScale = magnifyingGlassRect.localScale.x;
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

        if (isDynamicScalingActive)
        {
            UpdateMagnifyingGlassScale();
        }
    }

    private void HandleInteractionClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject()) return;

            InteractionPoint interactionPoint = hit.collider.gameObject.GetComponentInParent<InteractionPoint>();
            if (interactionPoint != null)
            {
                interactionPoint.OnClick();
                MoveCameraToInteractionPoint(interactionPoint);
            }
        }
    }

    private void MoveCameraToInteractionPoint(InteractionPoint interactionPoint)
    {
        isDynamicScalingActive = false;
        AnimateMagnifierScale(initialMagnifierScale * zoomInMagnifierMultiplier);

        Vector3 targetPosition = interactionPoint.cameraPosition.position;
        Vector3 targetLookAtPosition = interactionPoint.transform.position;

        SetInteractionPointsVisibility(false);
        StartCoroutine(MoveCameraToPointCoroutine(targetPosition, targetLookAtPosition));
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
            magnifyingGlassRect.localScale = Vector3.one * initialMagnifierScale * scaleMultiplier;
        }
    }

    // --- ГЛАВНОЕ ИЗМЕНЕНИЕ ЗДЕСЬ ---
    public IEnumerator MoveCameraToDefaultPosition()
    {
        // 1. Запускаем анимацию уменьшения лупы. Теперь она НЕ включает компенсацию в конце.
        AnimateMagnifierScale(initialMagnifierScale);

        // 2. Показываем точки интереса.
        SetInteractionPointsVisibility(true);

        Vector3 targetPosition = defaultCameraPosition.position;
        Vector3 targetLookAtPosition = defaultCameraLookAtPosition.position;

        // 3. ЖДЕМ, ПОКА КАМЕРА ПОЛНОСТЬЮ ЗАВЕРШИТ ДВИЖЕНИЕ.
        yield return MoveCameraToPointCoroutine(targetPosition, targetLookAtPosition);

        // 4. И ТОЛЬКО ТЕПЕРЬ, когда камера на месте, включаем компенсацию.
        isDynamicScalingActive = true;
    }

    // --- Метод и корутина анимации упрощены ---
    private void AnimateMagnifierScale(float targetScale)
    {
        StopCoroutine("AnimateScaleCoroutine");
        StartCoroutine(AnimateScaleCoroutine(targetScale, magnifierScaleDuration));
    }

    private IEnumerator AnimateScaleCoroutine(float targetScale, float duration)
    {
        Vector3 startScale = magnifyingGlassRect.localScale;
        Vector3 endScale = Vector3.one * targetScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);
            magnifyingGlassRect.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        magnifyingGlassRect.localScale = endScale;
        // Мы убрали отсюда включение isDynamicScalingActive
    }

    // Остальные методы без изменений
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