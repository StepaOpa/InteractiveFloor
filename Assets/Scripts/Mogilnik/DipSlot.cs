using UnityEngine;

public class DigSpot : MonoBehaviour
{
    public GameObject hiddenItemPrefab; 
    public int tapsToReveal = 3;
    private int currentTaps = 0;

    void OnMouseDown()
    {
        currentTaps++;
        
        // --- СТРОКА НИЖЕ БЫЛА УДАЛЕНА ---
        // SoundManager.Instance.PlayDigSound(); 
        // -----------------------------------
        
        if (currentTaps >= tapsToReveal)
        {
            RevealItem();
        }
    }

    void RevealItem()
    {
        if (hiddenItemPrefab != null)
        {
            // Четвертый аргумент (transform.parent) указывает, кто будет родителем для нового объекта.
            Instantiate(hiddenItemPrefab, transform.position, Quaternion.identity, transform.parent);

            // Звук появления предмета оставляем здесь, он должен играть именно в этот момент
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.PlayItemRevealSound();
            }
        }
        
        Destroy(gameObject);
    }
}