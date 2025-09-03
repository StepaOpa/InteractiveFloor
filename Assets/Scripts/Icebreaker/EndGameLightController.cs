using UnityEngine;
using System.Collections;

public class EndGameLightController : MonoBehaviour
{
    [Tooltip("������-����, ����������� �������� ������� � ������� �����.")]
    [SerializeField] private Transform endGameTarget;

    [Tooltip("��� ����� (� ��������) ���� ����� ������������ � �������� �����.")]
    [SerializeField] private float transitionDuration = 2.5f;

    [Tooltip("�������� �����, ������� ����� ���������. ���� �����, ������ ������ ������� Directional Light.")]
    [SerializeField] private Light lightToMove;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Awake()
    {
        // ���� ���� �� ������ �������, ���� ��� �� �����
        if (lightToMove == null)
        {
            lightToMove = FindObjectOfType<Light>();
            if (lightToMove == null)
            {
                Debug.LogError("�� ����� �� ������ �������� ����� ��� ����������!", this);
            }
        }
    }

    /// <summary>
    /// ��������� ������� ����������� ����� � �������� �������.
    /// </summary>
    public void StartTransition()
    {
        if (endGameTarget == null || lightToMove == null)
        {
            Debug.LogWarning("���� ��� ����� (EndGameTarget) ��� ��� �������� ����� �� ���������!", this);
            return;
        }

        StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        // ���������� ��������� ������� � �������
        initialPosition = lightToMove.transform.position;
        initialRotation = lightToMove.transform.rotation;

        float elapsedTime = 0f;

        // ���� ����� ��������, ���� �� ������� ����� transitionDuration
        while (elapsedTime < transitionDuration)
        {
            // ��� ��� Time.timeScale ����� 0, �� ���������� Time.unscaledDeltaTime,
            // ����� �������� ������������ ���� �� "�����".
            elapsedTime += Time.unscaledDeltaTime;

            // ��������� �������� �� 0 �� 1
            float progress = Mathf.Clamp01(elapsedTime / transitionDuration);

            // ������ ������������� ������� � �������
            lightToMove.transform.position = Vector3.Lerp(initialPosition, endGameTarget.position, progress);
            lightToMove.transform.rotation = Quaternion.Slerp(initialRotation, endGameTarget.rotation, progress);

            yield return null; // ���� ���������� �����
        }

        // �������������� ������������� ��������� ��������
        lightToMove.transform.position = endGameTarget.position;
        lightToMove.transform.rotation = endGameTarget.rotation;
    }
}