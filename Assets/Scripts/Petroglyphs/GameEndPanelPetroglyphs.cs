using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameEndPanelPetroglyphs : MonoBehaviour
{
    [Header("UI ����������")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI detailsText;
    [SerializeField] private Button restartButton;
    [SerializeField] private Button mainMenuButton;

    // --- ����� ������ ---
    [Header("������� ������")]
    [SerializeField] private CoinRewardController coinRewardController; // ������ �� ���������� �������

    void Start()
    {
        restartButton.onClick.AddListener(OnRestartButtonClick);
        mainMenuButton.onClick.AddListener(OnMainMenuButtonClick);
    }

    // --- ����� ������� ---
    // �������� �������� coinsAwarded ��� �������� ���������� �����
    public void ShowPanel(bool isWin, int foundCount, int totalCount, int coinsAwarded)
    {
        gameObject.SetActive(true);

        if (isWin)
        {
            titleText.text = "������!";
            detailsText.text = $"�� ����� ��� �������!\n���������� �������: {coinsAwarded}";

            // --- ����� ������ ---
            // ��������� �������� ������� �������
            if (coinRewardController != null)
            {
                coinRewardController.StartRewardSequence(coinsAwarded);
            }
        }
        else
        {
            titleText.text = "���������";
            detailsText.text = $"����� �����.\n������� ��������: {foundCount} �� {totalCount}";
        }
    }

    private void OnRestartButtonClick()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void OnMainMenuButtonClick()
    {
        Debug.Log("������� � ������� ���� (�� �����������)");
    }

    private void OnDestroy()
    {
        restartButton.onClick.RemoveListener(OnRestartButtonClick);
        mainMenuButton.onClick.RemoveListener(OnMainMenuButtonClick);
    }
}