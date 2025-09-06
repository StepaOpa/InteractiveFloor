using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    private Rigidbody rb;

    // ���� ������ ����� ������� ����������� �������� �� ������.
    private Vector3 moveDirection = Vector3.zero;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        // --- �������� �� ���������� (��������� ���) ---
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        Vector3 keyboardMovement = new Vector3(moveHorizontal, 0.0f, moveVertical);

        // --- �������� �� UI ������ ---
        // ���������� �������� �� ���������� � �� ������.
        // ���� ������ � �������, � ������, �������� ����� �������.
        // ���� �����, ����� �������� ���-�� ����, ����� ����� ��������� ������.
        Vector3 totalMovement = keyboardMovement + moveDirection;

        rb.AddForce(totalMovement.normalized * moveSpeed);
    }

    // --- ����� ��������� ������ ��� ������ ---

    // ���� ����� ����� ����������, ����� ������ "������" ������
    public void OnPointerDownForward()
    {
        moveDirection.z = 1;
    }

    // ���� ����� ����� ����������, ����� ������ "������" ��������
    public void OnPointerUpForward()
    {
        moveDirection.z = 0;
    }

    public void OnPointerDownBack()
    {
        moveDirection.z = -1;
    }

    public void OnPointerUpBack()
    {
        moveDirection.z = 0;
    }

    public void OnPointerDownLeft()
    {
        moveDirection.x = -1;
    }

    public void OnPointerUpLeft()
    {
        moveDirection.x = 0;
    }

    public void OnPointerDownRight()
    {
        moveDirection.x = 1;
    }

    public void OnPointerUpRight()
    {
        moveDirection.x = 0;
    }
}