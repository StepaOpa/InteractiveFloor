using UnityEngine;
using UnityEngine.SceneManagement; // ��� ������ ����� ��� ������������ �����
using TMPro; // ��� ������ ����� ��� ������ � �������

public class GameManagerLabyrinth : MonoBehaviour
{
    [Header("������ �� ������� �����")]
    public GameObject endGamePanel;          // ���� ��������� ������ ����� ����
    public PlayerControllerLabyrinth player; // ���� ��������� ������ ����
    public TimerControllerLabyrinth timer;   // ���� ��������� ������ �������

    [Header("�������� UI �� EndGamePanel")]
    public TextMeshProUGUI titleText;        // ����� "������" / "���������"
    public TextMeshProUGUI detailsText;      // ��������� ����� (�������)

    private bool isGameOver = false; // ����, ������� ����������, ��� ���� ��� �����������

    void Start()
    {
        // � ������ ���� ������ ������ ������ ����� ����
        endGamePanel.SetActive(false);
        // �������� ����� (����� ��� �����������)
        Time.timeScale = 1f;
    }

    void Update()
    {
        // ���� ���� ��� �����������, �� ������ �� ������
        if (isGameOver) return;

        // ��������� ���������, �� ����� �� ����� �� �������
        if (timer.timeIsUp)
        {
            // ���� ����� �����, �������� ������� ���������
            LoseGame("����� �����!");
        }
    }

    // ���� ����� ����� ����������, ����� ����� �������
    public void WinGame()
    {
        if (isGameOver) return; // �������������� ��������, ����� �� �������� ������
        isGameOver = true;

        titleText.text = "������!";
        detailsText.text = "�� ����� ����� �� ���������!";

        EndGameSequence();
    }

    // ���� ����� ����� ����������, ����� ����� ��������
    public void LoseGame(string reason)
    {
        if (isGameOver) return;
        isGameOver = true;

        titleText.text = "���������!";
        detailsText.text = reason; // ���������� �������: "����� �����!" ��� "�������� � ����!"

        EndGameSequence();
    }

    // ����� ������������������ �������� � ����� ����
    private void EndGameSequence()
    {
        // "������������" ����
        Time.timeScale = 0f;
        // ��������� ������ ���������� �����, ����� �� ������ ���� �������
        player.enabled = false;
        // ���������� ������
        endGamePanel.SetActive(true);
    }

    // --- ������ ��� ������ �� EndGamePanel ---

    public void RestartGame()
    {
        // ������������� ������� �����
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMenu()
    {
        // � ������� ����� ����� ��� ��� �������� � ������� ����
        // ���� ������ ������� ��������� � �������
        Debug.Log("������� � ������� ����...");
        // SceneManager.LoadScene("MainMenuScene"); // ������, ��� ��� ����� ���������
    }
}