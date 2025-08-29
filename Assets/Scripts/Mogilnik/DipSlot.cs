using UnityEngine;

public class DigSpot : MonoBehaviour
{
    [Header("Объекты")]
    public GameObject hiddenItemPrefab; 
    public GameObject itemRevealEffectPrefab;

    [Header("Настройки")]
    public int tapsToReveal = 3;
    
    private int currentTaps = 0;

    void OnMouseDown()
    {
        currentTaps++;
        
        if (currentTaps >= tapsToReveal)
        {
            RevealItem();
        }
    }

    void RevealItem()
    {
        if (itemRevealEffectPrefab != null)
        {
            // --- ДИАГНОСТИЧЕСКАЯ СТРОКА ---
            Debug.Log("Попытка создать эффект появления предмета!");
            // --------------------------------
            Instantiate(itemRevealEffectPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            // --- ЭТА СТРОКА ПОМОЖЕТ, ЕСЛИ ПРИЧИНА №1 ВЕРНА ---
            Debug.LogWarning("Эффект появления предмета НЕ НАЗНАЧЕН в инспекторе DigSpot!");
            // --------------------------------------------------
        }

        if (hiddenItemPrefab != null)
        {
            Instantiate(hiddenItemPrefab, transform.position, Quaternion.identity, transform.parent);
            
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayItemRevealSound();
            }
        }
        
        Destroy(gameObject);
    }
}