using UnityEngine;
using UnityEngine.EventSystems;

public class DigAreaClickHandler : MonoBehaviour
{
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // --- НОВАЯ ПРОВЕРКА ---
            // Спрашиваем у UI, не активен ли сейчас режим осмотра.
            // Если да, то ничего не делаем и сразу выходим из метода.
            if (InspectionUI.Instance != null && InspectionUI.Instance.IsInspectionUIActive())
            {
                return;
            }
            // --------------------

            // Эта проверка остается, она нужна, чтобы не копать при клике на кнопки инвентаря.
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                if (hit.collider.CompareTag("Diggable"))
                {
                    if (SoundManager.Instance != null)
                    {
                        SoundManager.Instance.PlayDigSound();
                    }
                }
            }
        }
    }
}