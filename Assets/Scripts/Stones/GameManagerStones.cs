using UnityEngine;
using UnityEngine.UI; // Для работы с Image (радиальный бар)
using TMPro;          // Для работы с TextMeshPro

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

    [Header("System & Controllers")]
    public SpawnerStones spawner; // Ссылка на спаунер камней
    public GameObject gameplayUiContainer;
    public EndGamePanelStones endGamePanel;
    public CoinRewardControllerStones coinRewardController;

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
        coinText.text = $"Собрано ценных камней: {coins}/{targetCoins}";
    }

    void UpdateTimeText()
    {
        timerText.text = Mathf.Ceil(gameTime).ToString();
    }

    void UpdateRadialBar()
    {
        timerRadialBar.fillAmount = gameTime / totalGameTime;
    }

    void WinGame()
    {
        isGameActive = false;

        // Останавливаем появление новых камней
        if (spawner != null)
        {
            spawner.StopSpawning();
        }

        gameplayUiContainer.SetActive(false);
        endGamePanel.ShowPanel(true);

        // Запускаем анимацию победных монеток
        if (coinRewardController != null)
        {
            StartCoroutine(coinRewardController.GetRewardSequenceCoroutine(targetCoins));
        }
    }

    void LoseGame()
    {
        isGameActive = false;

        // Останавливаем появление новых камней
        if (spawner != null)
        {
            spawner.StopSpawning();
        }

        gameplayUiContainer.SetActive(false);
        endGamePanel.ShowPanel(false);
    }
}