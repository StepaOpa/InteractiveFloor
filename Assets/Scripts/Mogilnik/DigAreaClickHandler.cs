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
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                // --- ГЛАВНОЕ ИЗМЕНЕНИЕ ---
                // Теперь мы проверяем не сам объект, а его тег.
                // Если у объекта, в который мы попали, есть тег "Diggable"...
                if (hit.collider.CompareTag("Diggable"))
                {
                    // ...то проигрываем звук копания.
                    if (SoundManager.Instance != null)
                    {
                        SoundManager.Instance.PlayDigSound();
                    }
                }
                // -------------------------
            }
        }
    }
}