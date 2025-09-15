using UnityEngine;

// Название класса теперь DestroyerStones
public class DestroyerStones : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Destroy(other.gameObject);
    }
}