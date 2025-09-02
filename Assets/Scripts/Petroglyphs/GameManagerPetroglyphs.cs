using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro; // �����! ������ ��� ������ ��� ������ � TextMeshPro


public class GameManagerPetroglyphs : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image petroglyphToFindImage;
    [SerializeField] private Image timerImage; // ���������� ��������-���
    [SerializeField] private TextMeshProUGUI timerText; // ��������� ������ (�����)
    [SerializeField] private GameEndPanelPetroglyphs endPanel; // ������ �� ������ ����� ����� ������

    [Header("Game Settings")]
    [SerializeField] private List<Sprite> allPetroglyphSprites;
    [SerializeField] private float timePerPetroglyph = 15f;

    private List<Sprite> availablePetroglyphs;
    private Sprite currentPetroglyph;
    private float currentTime;
    private bool isGameActive = true;

    // --- ����� ���������� ��� ���������� ---
    private int foundPetroglyphsCount = 0;
    private int totalPetroglyphsCount = 0;

    void Start()
    {
        // �������� ������ ��������� ���� � ����� ������
        endPanel.gameObject.SetActive(false);

        availablePetroglyphs = new List<Sprite>(allPetroglyphSprites);
        totalPetroglyphsCount = allPetroglyphSprites.Count; // ����������, ������� ����� ���� ��������
        foundPetroglyphsCount = 0; // ���������� ������� ���������

        SelectNextPetroglyph();
    }

    void Update()
    {
        if (!isGameActive) return;

        currentTime -= Time.deltaTime;

        // --- ���������� UI ������� (��������) ---
        // ��������� ���������� ��������-���
        timerImage.fillAmount = currentTime / timePerPetroglyph;
        // ��������� �����. Mathf.CeilToInt ��������� �� ���������� �������� ������ (14.1 -> 15)
        timerText.text = Mathf.CeilToInt(currentTime).ToString();

        if (currentTime <= 0)
        {
            // ����������, ����� ������ �� ��������� ������������� �����
            timerText.text = "0";
            LoseGame();
        }
    }

    public void SelectNextPetroglyph()
    {
        if (availablePetroglyphs.Count > 0)
        {
            ResetTimer();
            int randomIndex = Random.Range(0, availablePetroglyphs.Count);
            currentPetroglyph = availablePetroglyphs[randomIndex];
            petroglyphToFindImage.sprite = currentPetroglyph;
            availablePetroglyphs.RemoveAt(randomIndex);
            Debug.Log("����� �����: " + currentPetroglyph.name);
        }
        else
        {
            WinGame();
        }
    }

    public void CheckFoundPetroglyph(PetroglyphLocation foundLocation)
    {
        if (!isGameActive) return;

        if (foundLocation.petroglyphSprite == currentPetroglyph)
        {
            Debug.Log("���������! ������ ���roglyph: " + currentPetroglyph.name);
            foundPetroglyphsCount++; // ����������� ������� ���������
            SelectNextPetroglyph();
        }
        else
        {
            Debug.Log("�������. ��� ������ �������.");
        }
    }

    private void ResetTimer()
    {
        currentTime = timePerPetroglyph;
    }

    // --- ������ �������� (��������) ---
    private void WinGame()
    {
        isGameActive = false;
        Debug.Log("����������! �� ����� ��� ����������!");
        // �������� ����� �� ������, �������� ���, ��� ��� ������, � ����������
        endPanel.ShowPanel(true, foundPetroglyphsCount, totalPetroglyphsCount);
    }

    private void LoseGame()
    {
        isGameActive = false;
        Debug.Log("����� �����! �� ���������.");
        // �������� ����� �� ������, �������� ���, ��� ��� ���������, � ����������
        endPanel.ShowPanel(false, foundPetroglyphsCount, totalPetroglyphsCount);
    }
}