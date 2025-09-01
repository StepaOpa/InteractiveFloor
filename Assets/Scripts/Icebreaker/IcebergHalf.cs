using UnityEngine;
using System.Collections;

public class IcebergHalf : MonoBehaviour
{
    // ��� ������� ����� ��������� ������� "������ ������" �����
    public void StartSinking(float delay, float speed)
    {
        // ��������� �������� �� ���� �� ������� (���������)
        StartCoroutine(SinkCoroutine(delay, speed));
    }

    private IEnumerator SinkCoroutine(float sinkDelay, float sinkSpeed)
    {
        // ���� �������
        yield return new WaitForSeconds(sinkDelay);

        // ������ ����� ������
        while (this != null) // ���������� 'this' ��� ������� ����������
        {
            transform.Translate(Vector3.down * sinkSpeed * Time.deltaTime, Space.World);
            yield return null;
        }
    }
}