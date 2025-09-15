using UnityEngine;
using UnityEngine.EventSystems; // <-- ¬от недостающа€ строка!

// Ётот маленький компонент нужно будет повесить на кнопки-стрелки,
// чтобы отслеживать, зажаты ли они.
public class ButtonState : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public bool isPressed { get; private set; }

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isPressed = false;
    }
}