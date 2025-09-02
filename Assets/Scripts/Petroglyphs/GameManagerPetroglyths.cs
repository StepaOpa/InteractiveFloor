using UnityEngine;
using UnityEngine.UI; // ����������� ������ ��� ������ ��� ������ � UI
using System.Collections.Generic; // ��� ������ �� ��������


public class GameManagerPetroglyths : MonoBehaviour // <-- ��� ������ ��������
{
    [Header("UI Elements")]
    [SerializeField] private Image petroglyphToFindImage; // ���� ��������� ���� ������

    [Header("Game Settings")]
    [SerializeField] private List<Sprite> allPetroglyphSprites; // ������ ���� ��������� �������� �����������

    private List<Sprite> availablePetroglyphs; // ������ �����������, ������� ��� �� ���� �������
    private Sprite currentPetroglyph; // ������� ���������, ������� ����� �����

    void Start()
    {
        // �������� ��� ������� � ������ ��������� ��� ������
        availablePetroglyphs = new List<Sprite>(allPetroglyphSprites);
        // �������� ��������� ��������� ��� ������
        SelectNextPetroglyph();
    }

    public void SelectNextPetroglyph()
    {
        if (availablePetroglyphs.Count > 0)
        {
            // �������� ��������� ������ �� ������ ��������� �����������
            int randomIndex = Random.Range(0, availablePetroglyphs.Count);
            currentPetroglyph = availablePetroglyphs[randomIndex];

            // ������������� ������ � ��� UI �������
            petroglyphToFindImage.sprite = currentPetroglyph;

            // ������� ��������� ��������� �� ������, ����� �� �� ����������
            availablePetroglyphs.RemoveAt(randomIndex);

            Debug.Log("����� �����: " + currentPetroglyph.name);
        }
        else
        {
            // ��� ���������� �������!
            Debug.Log("����������! �� ����� ��� ����������!");
            // ����� ����� ����� ������ ������ � ����
        }
    }
}