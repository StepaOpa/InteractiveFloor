using UnityEngine;
using UnityEngine.EventSystems;

public class Digger : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        // --- НОВАЯ ПРОВЕРКА ---
        // Перед тем как что-либо делать, проверяем, не активен ли режим осмотра.
        // Если да - полностью игнорируем все дальнейшие действия в этом кадре.
        if (InspectionUI.Instance != null && InspectionUI.Instance.IsInspectionUIActive())
        {
            return; // Немедленно выходим из метода Update
        }
        // --- КОНЕЦ ПРОВЕРКИ ---

        // Весь остальной код остается без изменений.
        // Он выполнится, только если проверка выше не сработала.
        if (Input.GetMouseButton(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                DirtPile currentPile = hit.collider.GetComponent<DirtPile>();

                if (currentPile != null)
                {
                    currentPile.Dig();
                }
            }
        }
    }
}