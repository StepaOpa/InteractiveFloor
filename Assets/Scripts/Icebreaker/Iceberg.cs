using UnityEngine;

public class Iceberg : MonoBehaviour
{
    [Header("����� ��������")]
    [Tooltip("������� ������ ������ ��������, ������� ����� ���������")]
    [SerializeField] private GameObject wholeModel;

    [Tooltip("������ ���������, ������� ����� ��������")]
    [SerializeField] private GameObject half_A;

    [Tooltip("������ ���������, ������� ����� ��������")]
    [SerializeField] private GameObject half_B;

    [Header("��������� ����������")]
    [SerializeField] private float separationForce = 2f;
    [SerializeField] private float sinkDelay = 1.5f;
    [SerializeField] private float sinkSpeed = 0.2f;

    private bool isBroken = false;

    public void BreakApart()
    {
        if (isBroken) return;
        isBroken = true;

        if (wholeModel != null)
        {
            wholeModel.SetActive(false);
        }
        GetComponent<Collider>().enabled = false;

        half_A.transform.SetParent(null);
        half_B.transform.SetParent(null);

        ActivateHalf(half_A, Vector3.left);
        ActivateHalf(half_B, Vector3.right);

        // ���������� ������ ������ ������ ��������, �� ������ �� �����
        Destroy(gameObject);
    }

    private void ActivateHalf(GameObject half, Vector3 direction)
    {
        half.SetActive(true);
        Rigidbody rb = half.GetComponent<Rigidbody>();

        // ========== ������ ��������� ==========
        // ������� �� ��������� ��� ����� ������
        IcebergHalf sinkingScript = half.GetComponent<IcebergHalf>();
        // ========== ����� ��������� ==========

        if (rb != null && sinkingScript != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.AddForce(direction * separationForce, ForceMode.Impulse);

            // ========== ��������� ==========
            // �� ��������� �������� �����, � ���� ������� ���������
            sinkingScript.StartSinking(sinkDelay, sinkSpeed);
            // ===============================
        }
    }
}