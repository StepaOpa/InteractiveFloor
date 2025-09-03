using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;


public class GameManagerPetroglyphs : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject gameUiContainer;
    [SerializeField] private Image petroglyphToFindImage;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Image timerImage;
    [SerializeField] private TextMeshProUGUI foundCounterText;
    [SerializeField] private GameEndPanelPetroglyphs endPanel;

    [Header("UI Animation")]
    [SerializeField] private GameObject flyingPetroglyphPrefab;
    [SerializeField] private RectTransform targetIconTransform;
    [SerializeField] private float flyAnimationSpeed = 800f;
    [SerializeField] private float startScale = 0.1f;

    [Header("Game Settings")]
    [SerializeField] private List<Sprite> allPetroglyphSprites;
    [SerializeField] private float timePerPetroglyph = 15f;
    [SerializeField] private int coinsOnWin = 10;

    [Header("Scene References")]
    [SerializeField] private CameraController cameraController;
    [SerializeField] private Canvas mainCanvas;

    private List<Sprite> availablePetroglyphs; // <--- ¬ÓÚ Ô‡‚ËÎ¸ÌÓÂ ËÏˇ ÔÂÂÏÂÌÌÓÈ
    private Sprite currentPetroglyph;
    private float currentTime;
    private bool isGameActive = true;
    private bool isAnimating = false;
    private int foundPetroglyphsCount = 0;
    private int totalPetroglyphsCount = 0;

    void Start()
    {
        endPanel.gameObject.SetActive(false);
        gameUiContainer.SetActive(true);
        availablePetroglyphs = new List<Sprite>(allPetroglyphSprites);
        totalPetroglyphsCount = allPetroglyphSprites.Count;
        foundPetroglyphsCount = 0;
        UpdateFoundCounterText();
        SelectNextPetroglyph();
    }

    void Update()
    {
        if (!isGameActive || isAnimating) return;
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
            // --- »—œ–¿¬À≈Õ»≈ Œÿ»¡ » ¡€ÀŒ «ƒ≈—‹ ---
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
        if (!isGameActive || isAnimating) return;
        if (foundLocation.petroglyphSprite == currentPetroglyph)
        {
            StartCoroutine(AnimateFoundPetroglyph(foundLocation));
        }
    }

    private IEnumerator AnimateFoundPetroglyph(PetroglyphLocation foundLocation)
    {
        isAnimating = true;

        GameObject flyingIcon = Instantiate(flyingPetroglyphPrefab, mainCanvas.transform);
        flyingIcon.GetComponent<Image>().sprite = foundLocation.petroglyphSprite;

        Vector3 startPosition = Camera.main.WorldToScreenPoint(foundLocation.transform.position);
        Vector3 endPosition = targetIconTransform.position;
        flyingIcon.transform.position = startPosition;

        Vector3 initialScale = Vector3.one * startScale;
        Vector3 finalScale = Vector3.one;

        float distance = Vector3.Distance(startPosition, endPosition);
        float duration = (flyAnimationSpeed > 0) ? distance / flyAnimationSpeed : 0;

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float smoothedT = 1f - Mathf.Cos(t * Mathf.PI * 0.5f);

            flyingIcon.transform.position = Vector3.Lerp(startPosition, endPosition, smoothedT);
            flyingIcon.transform.localScale = Vector3.Lerp(initialScale, finalScale, smoothedT);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(flyingIcon);

        foundPetroglyphsCount++;
        UpdateFoundCounterText();
        SelectNextPetroglyph();

        isAnimating = false;
    }

    private void UpdateFoundCounterText()
    {
        if (foundCounterText != null)
        {
            foundCounterText.text = $"Õ‡È‰ÂÌÓ ËÒÛÌÍÓ‚: {foundPetroglyphsCount}/{totalPetroglyphsCount}";
        }
    }

    private void ResetTimer()
    {
        currentTime = timePerPetroglyph;
    }

    private IEnumerator WinGame()
    {
        isGameActive = false;
        gameUiContainer.SetActive(false);
        yield return cameraController.MoveCameraToDefaultPosition();
        endPanel.ShowPanel(true, foundPetroglyphsCount, totalPetroglyphsCount, coinsOnWin);
    }

    private IEnumerator LoseGame()
    {
        isGameActive = false;
        gameUiContainer.SetActive(false);
        yield return cameraController.MoveCameraToDefaultPosition();
        endPanel.ShowPanel(false, foundPetroglyphsCount, totalPetroglyphsCount, 0);
    }
}