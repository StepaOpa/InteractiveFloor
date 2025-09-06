using UnityEngine;

public class PlayerControllerLabyrinth : MonoBehaviour
{
    // --- ���������� ��� �������� ---
    public float moveSpeed = 5.0f;
    private Rigidbody rb;
    private Vector3 moveDirection = Vector3.zero;

    // --- ������ �� ������� ����������� ������ ---
    private GameManagerLabyrinth gameManager;

    void Start()
    {
        // �������� ��������� Rigidbody ��� ���������� �������
        rb = GetComponent<Rigidbody>();
        // ������� �� ����� ������ �� �������� GameManagerLabyrinth � "����������" ���
        gameManager = FindObjectOfType<GameManagerLabyrinth>();
    }

    void FixedUpdate()
    {
        // ��������� ���� � ���������� (������� ��� WASD)
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        Vector3 keyboardMovement = new Vector3(moveHorizontal, 0.0f, moveVertical);

        // ���������� �������� �� ���������� � �� UI-������
        Vector3 totalMovement = keyboardMovement + moveDirection;

        // ��������� ���� � �������, ����� �� ��������.
        // .normalized �����, ����� �������� �� ��������� �� ���� ����
        rb.AddForce(totalMovement.normalized * moveSpeed);
    }

    // --- ����� ����� ---
    // ���� ����� ������������� ���������� Unity, ����� ��� ������ ������ � �������
    private void OnTriggerEnter(Collider other)
    {
        // ��������� ��� �������, � ������� �� �����
        if (other.CompareTag("WinZone"))
        {
            // ���� ��� ���� ������, �������� GameManager'�
            gameManager.WinGame();
        }

        if (other.CompareTag("Trap"))
        {
            // ���� ��� �������, �������� GameManager'�
            gameManager.LoseGame("�� �������� � ����!");
        }
    }

    // --- ������ ��� UI-������ (�������� ��� ���������) ---
    // ����������, ����� ������ ������
    public void OnPointerDownForward() { moveDirection.z = 1; }
    public void OnPointerDownBack() { moveDirection.z = -1; }
    public void OnPointerDownLeft() { moveDirection.x = -1; }
    public void OnPointerDownRight() { moveDirection.x = 1; }

    // ����������, ����� ������ ��������
    public void OnPointerUpForward() { moveDirection.z = 0; }
    public void OnPointerUpBack() { moveDirection.z = 0; }
    public void OnPointerUpLeft() { moveDirection.x = 0; }
    public void OnPointerUpRight() { moveDirection.x = 0; }
}