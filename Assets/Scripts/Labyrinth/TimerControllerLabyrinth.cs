using UnityEngine;
using UnityEngine.UI;
using TMPro; // <<< �����: �������� ��� ������ ��� ������ � TextMeshPro

public class TimerControllerLabyrinth : MonoBehaviour
{
    // --- ������ ���������� ---
    public Image timerImage;
    public float timeLimit = 120f;
    private float timeRemaining;
    public bool timeIsUp = false;

    // --- ����� ���������� ---
    // ���� �� ��������� ��� ��������� UI-�������
    public TextMeshProUGUI timerText;

    void Start()
    {
        timeRemaining = timeLimit;
        timeIsUp = false;
    }

    void Update()
    {
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;

            // ��������� � ���������� ���...
            timerImage.fillAmount = timeRemaining / timeLimit;
            // ...� �����
            DisplayTime(timeRemaining); // <<< �����: �������� ����� ��� ���������� ������
        }
        else
        {
            // ����� �����
            timeIsUp = true;
            timeRemaining = 0;

            // ��������, ��� ��� ����������
            timerImage.fillAmount = 0;
            timerText.text = "00:00"; // <<< �����: ���������� ���� � ������
        }
    }

    // --- ����� ����� ---
    // ���� ����� ������� ����������� ����� � ������ ��:�� � ������� �� �����
    void DisplayTime(float timeToDisplay)
    {
        // ��������� 1 �������, ����� ������ �� ��������� 00:00, ����� �������� ��� ����� �������
        if (timeToDisplay < 0)
        {
            timeToDisplay = 0;
        }

        // ��������� ������ � �������
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        // ��������� �����, ��������� �������������� ������. "00" ��������, ��� ����� ������� ���� (01, 02 � �.�.)
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}