using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManagerStones : MonoBehaviour
{
    [Header("Game Logic")]
    public int coins = 0;
    public int targetCoins = 10;
    public float gameTime = 30f;

    [Header("UI Elements")]
    public TextMeshProUGUI coinText;
    public TextMeshProUGUI timerText;
    public Image timerRadialBar;

    [Header("Containers & Panels")]
    public GameObject gameplayUiContainer; // Ссылка на наш контейнер "ForClear"
    public EndGamePanelStones endGamePanel; // Ссылка на скрипт нашей панели

    private float totalGameTime;
    private bool isGameActive = true;

    void Start()
    {
        totalGameTime = gameTime;
        UpdateCoinText();
        UpdateTimeText();
    }

    void Update()
    {
        if (!isGameActive) return;

        gameTime -= Time.deltaTime;
        UpdateTimeText();
        UpdateRadialBar();

        if (gameTime <= 0)
        {
            gameTime = 0;
            UpdateTimeText();
            UpdateRadialBar();
            LoseGame();
        }

        if (Input.GetMouseButtonDown(0))
        {
            HandleClick();
        }
    }

    void HandleClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.collider.CompareTag("ValuableStone"))
            {
                coins++;
                UpdateCoinText();
                Destroy(hit.collider.gameObject);

                if (coins >= targetCoins)
                {
                    WinGame();
                }
            }
        }
    }

    void UpdateCoinText()
    {
        coinText.text = $"собрано ценных камней: {coins}/{targetCoins}";
    }

    void UpdateTimeText()
    {
        timerText.text = Mathf.Ceil(gameTime).ToString();
    }

    void UpdateRadialBar()
    {
        timerRadialBar.fillAmount = gameTime / totalGameTime;
    }

    // --- ОБНОВЛЕННЫЕ МЕТОДЫ ПОБЕДЫ И ПОРАЖЕНИЯ ---
    void WinGame()
    {
        isGameActive = false;
        gameplayUiContainer.SetActive(false); // Прячем игровой интерфейс
        endGamePanel.ShowPanel(true); // Показываем панель с сообщением о победе
    }

    void LoseGame()
    {
        isGameActive = false;
        gameplayUiContainer.SetActive(false); // Прячем игровой интерфейс
        endGamePanel.ShowPanel(false); // Показываем панель с сообщением о поражении
    }
}