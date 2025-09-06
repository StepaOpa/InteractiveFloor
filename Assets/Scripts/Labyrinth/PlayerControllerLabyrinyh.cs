using UnityEngine;

public class PlayerControllerLabyrinth : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    private Rigidbody rb;
    private Vector3 moveDirection = Vector3.zero;
    private GameManagerLabyrinth gameManager;

    // --- �����: ��� "�����������" �������� ---
    public bool canMove = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        gameManager = FindObjectOfType<GameManagerLabyrinth>();
    }

    void FixedUpdate()
    {
        // --- �����: ���� ��������� ������, ������� �� ������ ---
        if (!canMove)
        {
            // ����� �������� ����, ����� ���� ����� ������������
            rb.linearVelocity = Vector3.zero;
            return;
        }

        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");
        Vector3 keyboardMovement = new Vector3(moveHorizontal, 0.0f, moveVertical);

        Vector3 totalMovement = keyboardMovement + moveDirection;
        rb.AddForce(totalMovement.normalized * moveSpeed);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("WinZone")) { gameManager.WinGame(); }
        if (other.CompareTag("Trap")) { gameManager.LoseGame("�� �������� � ����!"); }
    }

    // --- �����: ��������� �������� � ������ ����� ��� ������ ---
    public void OnPointerDownForward() { if (!canMove) return; moveDirection.z = 1; }
    public void OnPointerDownBack() { if (!canMove) return; moveDirection.z = -1; }
    public void OnPointerDownLeft() { if (!canMove) return; moveDirection.x = -1; }
    public void OnPointerDownRight() { if (!canMove) return; moveDirection.x = 1; }

    public void OnPointerUpForward() { moveDirection.z = 0; }
    public void OnPointerUpBack() { moveDirection.z = 0; }
    public void OnPointerUpLeft() { moveDirection.x = 0; }
    public void OnPointerUpRight() { moveDirection.x = 0; }
}