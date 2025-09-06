using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // ��� ���������� ����� ����� � ���������� Unity.
    // ��� ����������, ��� ������ ����� ��������� ����.
    public float moveSpeed = 5.0f;

    // ������ �� ��������� Rigidbody, ������� �������� �� ������.
    private Rigidbody rb;

    // Start ���������� ���� ��� ��� ������� ����.
    void Start()
    {
        // ������� � ��������� ��������� Rigidbody, ������� ����� �� ���� �� �������.
        rb = GetComponent<Rigidbody>();
    }

    // Update ���������� ������ ����. ����� �� ����� ��������� ����.
    // �� ������� ����� � FixedUpdate ��� ���������� ������ ������.
    void FixedUpdate()
    {
        // �������� ���� � ���������� (������� ��� WASD).
        // Input.GetAxis ������ �������� �� -1 �� 1.
        float moveHorizontal = Input.GetAxis("Horizontal"); // A, D, ������� �����/������
        float moveVertical = Input.GetAxis("Vertical");     // W, S, ������� �����/����

        // ������� ������ ����������� ��������.
        // �� ��������� �� ���� X � Z (�� �������������� ���������).
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);

        // ��������� ���� � Rigidbody, ����� ������� ����.
        // �� �������� ����������� �� ��������, ����� �������������� ��������.
        rb.AddForce(movement * moveSpeed);
    }
}