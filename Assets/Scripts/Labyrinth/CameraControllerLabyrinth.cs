using UnityEngine;

public class CameraControllerLabyrinth : MonoBehaviour
{
    [Header("���� ��� ������")]
    public Transform closeViewTarget; // "�����", ������� �������� � ����
    public Transform farViewTarget;   // "�����" ��� ������ �����

    [Header("���������")]
    // ����: public float smoothSpeed = 5f;
    // �����: �����, �� ������� ������ ������� ����. ������ = �������!
    public float smoothTime = 0.2f;

    // ��� ���������� ����� ��� ������ SmoothDamp, ��� ������ ������� �������� ������
    private Vector3 velocity = Vector3.zero;

    private bool isCloseView = true; // �������� � ������������� ����

    void LateUpdate()
    {
        Transform target = isCloseView ? closeViewTarget : farViewTarget;

        // --- ��� ������ ���������� ---
        // ����: Vector3.Lerp(...)
        // �����: Vector3.SmoothDamp(...)
        transform.position = Vector3.SmoothDamp(transform.position, target.position, ref velocity, smoothTime);

        // ������� ����� �������� � Lerp, �� �������� ������ ��� ��������
        Quaternion smoothedRotation = Quaternion.Lerp(transform.rotation, target.rotation, 5f * Time.deltaTime);
        transform.rotation = smoothedRotation;
    }

    // --- ��������� ������ �������� ��� ��������� ---

    public void SetCloseView()
    {
        isCloseView = true;
    }

    public void SetFarView()
    {
        isCloseView = false;
    }

    public void SwitchToFarViewImmediately()
    {
        isCloseView = false;
    }
}