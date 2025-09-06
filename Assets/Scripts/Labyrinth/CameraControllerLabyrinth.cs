using UnityEngine;

public class CameraControllerLabyrinth : MonoBehaviour
{
    [Header("���� ��� ������")]
    public Transform closeViewTarget; // "�����", ������� �������� � ����
    public Transform farViewTarget;   // "�����" ��� ������ �����

    [Header("���������")]
    public float smoothSpeed = 5f; // ��������, � ������� ������ ����� ���������

    private bool isCloseView = true; // �������� � ������������� ����

    // LateUpdate ���������� ����� ���� Update. �������� ��� �����,
    // ����� �������� ��������, ����� ������ � ����� �������� � ����� �����.
    void LateUpdate()
    {
        Transform target = isCloseView ? closeViewTarget : farViewTarget;

        // ������ ���������� ������ � ������� ����
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, target.position, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;

        // ������ ������������ ������ � ������� ����
        Quaternion smoothedRotation = Quaternion.Lerp(transform.rotation, target.rotation, smoothSpeed * Time.deltaTime);
        transform.rotation = smoothedRotation;
    }

    // --- ������ ��� ������ UI ---

    public void SetCloseView()
    {
        isCloseView = true;
    }

    public void SetFarView()
    {
        isCloseView = false;
    }

    // --- ����� ��� GameManager'� ---
    // �� ����� ������������� ����������� �� ����� ���� ����� ������ ����
    public void SwitchToFarViewImmediately()
    {
        isCloseView = false;
    }
}