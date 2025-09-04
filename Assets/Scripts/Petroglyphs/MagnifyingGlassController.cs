using UnityEngine;

public class MagnifyingGlassController : MonoBehaviour
{
    [Header("��������� �����������")]
    [SerializeField] private float moveSpeed = 5f; // �������� ����������� ����
    [SerializeField] private RectTransform moveArea; // ������������� (��������, ������), � �������� �������� ���� ����� ���������

    private RectTransform rectTransform; // ��������� ����� ����

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    // ����� ��� ����������� ����, ������� ����� ���������� �� ������
    public void Move(Vector2 direction)
    {
        // ��������� ����� �������
        Vector3 newPosition = rectTransform.anchoredPosition + direction * moveSpeed * Time.deltaTime;

        // ������������ �������� � �������� �������� �������
        if (moveArea != null)
        {
            Rect areaRect = moveArea.rect;
            // ���������� Clamp ��� ����������� ���������
            newPosition.x = Mathf.Clamp(newPosition.x, areaRect.xMin, areaRect.xMax);
            newPosition.y = Mathf.Clamp(newPosition.y, areaRect.yMin, areaRect.yMax);
        }

        rectTransform.anchoredPosition = newPosition;
    }

    // ��������� ����� ��� �������� ����������
    // GameManager ����� �������� ��� ��� ������� �� ������ "�������"
    public void TryToFindPetroglyph()
    {
        // ������� ��� �� ������� ������� ���� �� ������
        Ray ray = Camera.main.ScreenPointToRay(transform.position);
        RaycastHit hit;

        // ������� ��� � ���������, ����� �� �� �� ���-��
        if (Physics.Raycast(ray, out hit))
        {
            // �������� �������� ��������� PetroglyphLocation �� �������, � ������� ������
            PetroglyphLocation petroglyphLocation = hit.collider.GetComponent<PetroglyphLocation>();

            // ���� ��������� ������, ������ �� ������ � ���������
            if (petroglyphLocation != null)
            {
                // ������� GameManager �� ����� � �������� � ���� ����� ��������
                // ��� �� ����� ���������������� ������, �� ��� �������� ��������.
                // � ������� �������� ����� ������������ ������ ������.
                FindObjectOfType<GameManagerPetroglyphs>().CheckFoundPetroglyph(petroglyphLocation);
            }
        }
    }
}