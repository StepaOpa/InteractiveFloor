using UnityEngine;
using UnityEngine.UI;

public class UIControllerPetroglyphs : MonoBehaviour
{
    [Header("������")]
    [SerializeField] private MagnifyingGlassController magnifyingGlass; // ������ �� ���������� ����

    [Header("������ ����������")]
    [SerializeField] private Button acceptButton; // ������ "�������"
    [SerializeField] private Button upButton;
    [SerializeField] private Button downButton;
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;

    private Vector2 moveDirection = Vector2.zero; // ����������� ��������

    void Start()
    {
        // ��������� ��������� �� ������ "�������"
        if (acceptButton != null)
        {
            acceptButton.onClick.AddListener(OnAcceptButtonClick);
        }
    }

    void Update()
    {
        // ���� ���� �� ���������, ������ �� ������
        if (magnifyingGlass == null) return;

        // ���������� �����������
        moveDirection = Vector2.zero;

        // ���������, ������ �� ������.
        if (IsButtonPressed(upButton)) moveDirection += Vector2.up;
        if (IsButtonPressed(downButton)) moveDirection += Vector2.down;
        if (IsButtonPressed(leftButton)) moveDirection += Vector2.left;
        if (IsButtonPressed(rightButton)) moveDirection += Vector2.right;


        // ���� ���� �����������, ������� ����
        if (moveDirection != Vector2.zero)
        {
            // Normalize() ������ ������ ��������� �����, ����� �������� �� ��������� ���� ����� ��, ��� � �� ������.
            magnifyingGlass.Move(moveDirection.normalized);
        }
    }

    // ��������������� ����� ��� ��������, ������ �� ������
    private bool IsButtonPressed(Button button)
    {
        // ������ ��������� "�������", ���� ��� ����������, ������� � �� ��� ���� ��������� ButtonState, ������� �������� � �������.
        return button != null && button.GetComponent<ButtonState>() != null && button.GetComponent<ButtonState>().isPressed;
    }

    private void OnAcceptButtonClick()
    {
        // ���� ���� ����, �������� � ��� ����� ��������
        if (magnifyingGlass != null)
        {
            magnifyingGlass.TryToFindPetroglyph();
        }
    }
}