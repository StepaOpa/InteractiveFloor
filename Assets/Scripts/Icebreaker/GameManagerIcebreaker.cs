using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManagerIcebreaker : MonoBehaviour // <-- ��� ������ ������ ��������� � ������ �����
{
    // ������� ������ ���� public, ����� Unity �� ������
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}