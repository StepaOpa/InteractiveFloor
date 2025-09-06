using UnityEngine;
using System.Collections;

public class CameraControllerLabyrinth : MonoBehaviour
{
    [Header("���� ��� ������")]
    public Transform closeViewTarget;
    public Transform farViewTarget;

    [Header("������")]
    public PlayerControllerLabyrinth playerController;

    [Header("��������� ������")] // ������� ��������� ��� �������
    // <<< ��� �����������: ���������� ���������� ��� �������� >>>
    [Tooltip("�������� � �������� ����� �������������� ������������ � ������ ����")]
    public float initialZoomDelay = 2.0f;

    [Tooltip("����� ����������� � ������. ��� ������ ��������, ��� ��������� ������.")]
    public float initialZoomSmoothTime = 1.5f;

    [Tooltip("����� �������� � ����. ��� ������ ��������, ��� ������� ������.")]
    public float gameplaySmoothTime = 0.2f;

    private Vector3 velocity = Vector3.zero;
    private bool isCloseView;
    private float currentSmoothTime;

    void Start()
    {
        currentSmoothTime = initialZoomSmoothTime;
        StartCoroutine(InitialZoomInSequence());
    }

    void LateUpdate()
    {
        Transform target = isCloseView ? closeViewTarget : farViewTarget;
        transform.position = Vector3.SmoothDamp(transform.position, target.position, ref velocity, currentSmoothTime);

        Quaternion smoothedRotation = Quaternion.Lerp(transform.rotation, target.rotation, 5f * Time.deltaTime);
        transform.rotation = smoothedRotation;
    }

    private IEnumerator InitialZoomInSequence()
    {
        playerController.canMove = false;
        isCloseView = false;
        transform.position = farViewTarget.position;
        transform.rotation = farViewTarget.rotation;

        // ������ ��� ������ ����� ��������, ��� ��� ���������� ����������
        yield return new WaitForSeconds(initialZoomDelay);

        isCloseView = true;

        while (Vector3.Distance(transform.position, closeViewTarget.position) > 0.1f)
        {
            yield return null;
        }

        currentSmoothTime = gameplaySmoothTime;
        playerController.canMove = true;
    }

    public void SetCloseView()
    {
        isCloseView = true;
        currentSmoothTime = gameplaySmoothTime;
    }

    public void SetFarView()
    {
        isCloseView = false;
        currentSmoothTime = initialZoomSmoothTime;
    }

    public void SwitchToFarViewImmediately()
    {
        isCloseView = false;
        currentSmoothTime = initialZoomSmoothTime;
    }
}