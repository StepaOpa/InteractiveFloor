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
    [SerializeField] private float delayAfterFinding = 0.5f; // --- НОВАЯ СТРОКА ---

    [Header("Game Settings")]
    [SerializeField] private List<Sprite> allPetroglyphSprites;
    [SerializeField] private float timePerPetroglyph = 15f;
    [SerializeField] private int coinsOnWin = 10;

    [Header("Scene References")]
    [SerializeField] private CameraController cameraController;
    [SerializeField] private Canvas mainCanvas;

    private List<Sprite> availablePetroglyphs;
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

    public void CheckFoundPetroglyph(PetroglyphLocation foundLocation)
    {
        if (!isGameActive || isAnimating) return;
        if (foundLocation.petroglyphSprite == currentPetroglyph)
        {
            isAnimating = true; // Блокируем игру на время анимации

            // Создаем объект из префаба
            GameObject flyingIconObject = Instantiate(flyingPetroglyphPrefab, mainCanvas.transform);

            // Получаем его аниматор
            FlyingIconAnimator animator = flyingIconObject.GetComponent<FlyingIconAnimator>();

            Vector3 startPosition = Camera.main.WorldToScreenPoint(foundLocation.transform.position);
            Vector3 endPosition = targetIconTransform.position;

            // Запускаем анимацию и передаем ей метод OnAnimationComplete, который выполнится по завершении
            animator.StartAnimation(foundLocation.petroglyphSprite, startPosition, endPosition, flyAnimationSpeed, startScale, OnAnimationComplete);
        }
    }

    // --- НОВЫЙ МЕТОД, ВЫПОЛНЯЕТСЯ ПОСЛЕ АНИМАЦИИ ---
    private void OnAnimationComplete()
    {
        StartCoroutine(OnAnimationCompleteCoroutine());
    }

    private IEnumerator OnAnimationCompleteCoroutine()
    {
        // Ждем небольшую паузу, чтобы насладиться моментом
        yield return new WaitForSeconds(delayAfterFinding);

        // Теперь обновляем игру
        foundPetroglyphsCount++;
        UpdateFoundCounterText();
        SelectNextPetroglyph();

        isAnimating = false; // Снимаем блокировку
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

    private void UpdateFoundCounterText()
    {
        if (foundCounterText != null)
        {
            foundCounterText.text = $"Найдено рисунков: {foundPetroglyphsCount}/{totalPetroglyphsCount}";
        }
    }

    // ... (остальные методы без изменений) ...

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