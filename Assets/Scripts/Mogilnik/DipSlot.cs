using UnityEngine;

public class DigSpot : MonoBehaviour
{
    public GameObject hiddenItemPrefab; 
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
        if (hiddenItemPrefab != null)
        {
            // --- ВОТ ИСПРАВЛЕНИЕ ---
            // Четвертый аргумент (transform.parent) указывает, кто будет родителем для нового объекта.
            Instantiate(hiddenItemPrefab, transform.position, Quaternion.identity, transform.parent);
        }
        
        Destroy(gameObject);
    }
}