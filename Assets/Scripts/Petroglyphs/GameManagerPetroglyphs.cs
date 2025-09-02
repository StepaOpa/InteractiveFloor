using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;


public class GameManagerPetroglyphs : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject gameUiContainer; // --- ИЗМЕНЕНО: Общий контейнер для всего игрового UI
    [SerializeField] private Image petroglyphToFindImage;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image timerImage; // Вернул ссылку на таймер, так как она нужна в Update
    [SerializeField] private TextMeshProUGUI foundCounterText;
    [SerializeField] private GameEndPanelPetroglyphs endPanel;

    [Header("Game Settings")]
    [SerializeField] private List<Sprite> allPetroglyphSprites;
    [SerializeField] private float timePerPetroglyph = 15f;
    [SerializeField] private int coinsOnWin = 10;

    [Header("Scene References")]
    [SerializeField] private CameraController cameraController;

    private List<Sprite> availablePetroglyphs;
    private Sprite currentPetroglyph;
    private float currentTime;
    private bool isGameActive = true;
    private int foundPetroglyphsCount = 0;
    private int totalPetroglyphsCount = 0;

    void Start()
    {
        endPanel.gameObject.SetActive(false);
        gameUiContainer.SetActive(true); // --- НОВАЯ СТРОКА ---
        availablePetroglyphs = new List<Sprite>(allPetroglyphSprites);
        totalPetroglyphsCount = allPetroglyphSprites.Count;
        foundPetroglyphsCount = 0;
        UpdateFoundCounterText();
        SelectNextPetroglyph();
    }

    void Update()
    {
        if (!isGameActive) return;
        currentTime -= Time.deltaTime;
        timerImage.fillAmount = currentTime / timePerPetroglyph;
        timerText.text = Mathf.CeilToInt(currentTime).ToString();

        if (currentTime <= 0)
        {
            timerText.text = "0";
            StartCoroutine(LoseGame());
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
        }
        else
        {
            StartCoroutine(WinGame());
        }
    }

    public void CheckFoundPetroglyph(PetroglyphLocation foundLocation)
    {
        if (!isGameActive) return;
        if (foundLocation.petroglyphSprite == currentPetroglyph)
        {
            foundPetroglyphsCount++;
            UpdateFoundCounterText();
            SelectNextPetroglyph();
        }
    }

    private void UpdateFoundCounterText()
    {
        if (foundCounterText != null)
        {
            foundCounterText.text = $"Найдено рисунков: {foundPetroglyphsCount}/{totalPetroglyphsCount}";
        }
    }

    private void ResetTimer()
    {
        currentTime = timePerPetroglyph;
    }

    private IEnumerator WinGame()
    {
        isGameActive = false;

        // --- ИЗМЕНЕНО: Скрываем весь игровой UI одним махом ---
        gameUiContainer.SetActive(false);

        yield return cameraController.MoveCameraToDefaultPosition();
        endPanel.ShowPanel(true, foundPetroglyphsCount, totalPetroglyphsCount, coinsOnWin);
    }

    private IEnumerator LoseGame()
    {
        isGameActive = false;

        // --- ИЗМЕНЕНО: Скрываем весь игровой UI одним махом ---
        gameUiContainer.SetActive(false);

        yield return cameraController.MoveCameraToDefaultPosition();
        endPanel.ShowPanel(false, foundPetroglyphsCount, totalPetroglyphsCount, 0);
    }
}