using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{

    [SerializeField] private Transform defaultCameraPosition;
    [SerializeField] private Transform defaultCameraLookAtPosition;

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
            MoveCameraToDefaultPosition();
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

    private void MoveCameraToDefaultPosition()
    {
        Vector3 targetPosition = defaultCameraPosition.position;
        Vector3 targetLookAtPosition = defaultCameraLookAtPosition.position;

        StartCoroutine(MoveCameraToPointCoroutine(targetPosition, targetLookAtPosition, 10f));
    }



}

