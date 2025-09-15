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
        // --- ВОТ РЕШЕНИЕ ---
        // Перед тем как что-либо делать, проверяем, не активен ли режим осмотра.
        // Если да - полностью игнорируем этот клик.
        if (InspectionUI.Instance != null && InspectionUI.Instance.IsInspectionUIActive())
        {
            return;
        }
        // --------------------

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
            Instantiate(itemRevealEffectPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("Эффект появления предмета НЕ НАЗНАЧЕН в инспекторе DigSpot!");
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