using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform defaultCameraPosition;
    [SerializeField] private Transform defaultCameraLookAtPosition;
    [SerializeField] private GameManagerPetroglyphs gameManager;

    void Start()
    {
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log("Нажат объект: " + hit.collider.gameObject.name);
                PetroglyphLocation petroglyphLocation = hit.collider.GetComponent<PetroglyphLocation>();

                if (petroglyphLocation != null)
                {
                    gameManager.CheckFoundPetroglyph(petroglyphLocation);
                }

                InteractionPoint interactionPoint = hit.collider.gameObject.GetComponentInParent<InteractionPoint>();
                if (interactionPoint != null)
                {
                    interactionPoint.OnClick();
                    MoveCameraToInteractionPoint(interactionPoint);
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // --- ИЗМЕНЕНО ---
            // Так как метод теперь корутина, его нужно запускать через StartCoroutine
            StartCoroutine(MoveCameraToDefaultPosition());
        }
    }

    private void MoveCameraToInteractionPoint(InteractionPoint interactionPoint)
    {
        Vector3 targetPosition = interactionPoint.cameraPosition.position;
        Vector3 targetLookAtPosition = interactionPoint.transform.position;
        float targetDistance = interactionPoint.cameraDistance;

        StartCoroutine(MoveCameraToPointCoroutine(targetPosition, targetLookAtPosition, targetDistance));
    }

    private IEnumerator MoveCameraToPointCoroutine(Vector3 targetPosition, Vector3 targetLookAtPosition, float targetDistance)
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

    // --- МЕТОД ИЗМЕНЕН ---
    // Сделали его публичным и возвращающим IEnumerator
    public IEnumerator MoveCameraToDefaultPosition()
    {
        Vector3 targetPosition = defaultCameraPosition.position;
        Vector3 targetLookAtPosition = defaultCameraLookAtPosition.position;

        // Просто возвращаем корутину, чтобы другой скрипт мог ее дождаться
        yield return MoveCameraToPointCoroutine(targetPosition, targetLookAtPosition, 10f);
    }
}