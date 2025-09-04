using UnityEngine;
using UnityEngine.UI;

public class UIControllerPetroglyphs : MonoBehaviour
{
    [Header("Ссылки")]
    [SerializeField] private MagnifyingGlassController magnifyingGlass; // Ссылка на контроллер лупы

    [Header("Кнопки управления")]
    [SerializeField] private Button acceptButton; // Кнопка "Принять"
    [SerializeField] private Button upButton;
    [SerializeField] private Button downButton;
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;

    private Vector2 moveDirection = Vector2.zero; // Направление движения

    void Start()
    {
        // Добавляем слушатель на кнопку "Принять"
        if (acceptButton != null)
        {
            acceptButton.onClick.AddListener(OnAcceptButtonClick);
        }
    }

    void Update()
    {
        // Если лупа не назначена, ничего не делаем
        if (magnifyingGlass == null) return;

        // Сбрасываем направление
        moveDirection = Vector2.zero;

        // Проверяем, нажаты ли кнопки.
        if (IsButtonPressed(upButton)) moveDirection += Vector2.up;
        if (IsButtonPressed(downButton)) moveDirection += Vector2.down;
        if (IsButtonPressed(leftButton)) moveDirection += Vector2.left;
        if (IsButtonPressed(rightButton)) moveDirection += Vector2.right;


        // Если есть направление, двигаем лупу
        if (moveDirection != Vector2.zero)
        {
            // Normalize() делает вектор единичной длины, чтобы скорость по диагонали была такой же, как и по прямой.
            magnifyingGlass.Move(moveDirection.normalized);
        }
    }

    // Вспомогательный метод для проверки, зажата ли кнопка
    private bool IsButtonPressed(Button button)
    {
        // Кнопка считается "нажатой", если она существует, активна и на ней есть компонент ButtonState, который сообщает о нажатии.
        return button != null && button.GetComponent<ButtonState>() != null && button.GetComponent<ButtonState>().isPressed;
    }

    private void OnAcceptButtonClick()
    {
        // Если лупа есть, вызываем у нее метод проверки
        if (magnifyingGlass != null)
        {
            magnifyingGlass.TryToFindPetroglyph();
        }
    }
}