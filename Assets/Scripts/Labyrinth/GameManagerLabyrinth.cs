using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections; // <<< �����: �������� ��� ������ ��� ������ � ����������

public class GameManagerLabyrinth : MonoBehaviour
{
    [Header("������ �� ������� �����")]
    public GameObject endGamePanel;
    public PlayerControllerLabyrinth player;
    public TimerControllerLabyrinth timer;
    public CameraControllerLabyrinth cameraController; // <<< �����: ������ �� ������ ������

    [Header("�������� UI �� EndGamePanel")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI detailsText;

    private bool isGameOver = false;

    // ... (������ Start � Update �������� ��� ���������) ...
    void Start()
    {
        endGamePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    void Update()
    {
        if (isGameOver) return;
        if (timer.timeIsUp)
        {
            LoseGame("����� �����!");
        }
    }

    // --- ������ Win/Lose ������ ��������� �������� ---
    public void WinGame()
    {
        if (isGameOver) return;
        isGameOver = true;
        titleText.text = "������!";
        detailsText.text = "�� ����� ����� �� ���������!";
        StartCoroutine(EndGameSequence()); // <<< ��������
    }

    public void LoseGame(string reason)
    {
        if (isGameOver) return;
        isGameOver = true;
        titleText.text = "���������!";
        detailsText.text = reason;
        StartCoroutine(EndGameSequence()); // <<< ��������
    }

    // --- ������ ����� EndGameSequence ������� �� �������� ---
    private IEnumerator EndGameSequence()
    {
        // 1. ��������� ���������� �����
        player.enabled = false;

        // 2. ������� ������ ������������� �� ����� ����
        cameraController.SwitchToFarViewImmediately();

        // 3. ���� 1.5 �������, ���� ������ "�������"
        yield return new WaitForSeconds(1.5f);

        // 4. ������ ������ "������������" ���� � ���������� ������
        Time.timeScale = 0f;
        endGamePanel.SetActive(true);
    }

    // ... (������ RestartGame � GoToMenu �������� ��� ���������) ...
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void GoToMenu()
    {
        Debug.Log("������� � ������� ����...");
    }
}