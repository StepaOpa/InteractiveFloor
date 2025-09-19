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
        // По-прежнему проверяем, зажата ли левая кнопка мыши
        if (Input.GetMouseButton(0))
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // --- ГЛАВНОЕ ИЗМЕНЕНИЕ ---
            // Мы убрали всю логику про 'lastTouchedPile'.
            // Теперь, если луч попадает в кучку, мы просто вызываем ее метод Dig().
            // Это будет происходить каждый кадр, пока мышь зажата над кучкой.
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