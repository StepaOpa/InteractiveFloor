using UnityEngine;

// ���� ��������� �������� �� 3D-������ "����" �� �����,
// ������� �������� �������� �������� ��� CollectableItem.
[RequireComponent(typeof(Renderer))]
public class DirtCoat : MonoBehaviour
{
    [Header("��������� �������")]
    [Tooltip("������� ����� �����, ����� ��������� �������� �������")]
    [SerializeField] private int tapsToClean = 3;

    private Renderer coatRenderer;
    private int currentTaps = 0;
    private bool isFullyCleaned = false;

    // ��������, ����� CollectableItem ��� ������� ������, ������ �� ����
    public bool IsFullyClean => isFullyCleaned;

    void Awake()
    {
        // �������� ��������� �������, ����� ��������� ����������
        coatRenderer = GetComponent<Renderer>();

        // �����: ������� ����� ���������, ����� ��������� ������������
        // �� ������ �� ��� ��������� ������� � ���� �� ����������.
        Material materialInstance = coatRenderer.material;

        // ������������� ����� �������, ������� ������������ ������������.
        // ��� ����� �����-����� �������� �� �����.
        // ���������� ����������� ����� ������� ������� Unity
        materialInstance.SetFloat("_Mode", 3); // 3 = Transparent
        materialInstance.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        materialInstance.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        materialInstance.SetInt("_ZWrite", 0);
        materialInstance.DisableKeyword("_ALPHATEST_ON");
        materialInstance.EnableKeyword("_ALPHABLEND_ON");
        materialInstance.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        materialInstance.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        coatRenderer.material = materialInstance;

        // ����� ������������� ��������� ��������������
        Color initialColor = coatRenderer.material.color;
        initialColor.a = 1f;
        coatRenderer.material.color = initialColor;
    }

    /// <summary>
    /// ���� ����� ����� ���������� �� CollectableItem ��� ������ ����.
    /// </summary>
    /// <returns>���������� true, ���� ���� ����� ������� ��� ������ ���������.</returns>
    public bool RegisterTap()
    {
        if (isFullyCleaned)
        {
            return false; // ��� �����, ������ �� ������
        }

        currentTaps++;

        float newAlpha = (float)(tapsToClean - currentTaps) / tapsToClean;

        Color currentColor = coatRenderer.material.color;
        currentColor.a = newAlpha;
        coatRenderer.material.color = currentColor;

        if (currentTaps >= tapsToClean)
        {
            isFullyCleaned = true;
            gameObject.SetActive(false);
            // ����������: ���� ���������, ����� Debug ������������
            UnityEngine.Debug.Log("������� ��������� ������!");
            return true;
        }

        return false;
    }
}