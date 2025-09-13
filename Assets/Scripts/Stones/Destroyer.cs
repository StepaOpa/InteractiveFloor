using UnityEngine;

public class Destroyer : MonoBehaviour
{
    // Ётот метод вызываетс€ автоматически, когда другой объект
    // с компонентом Collider входит в триггер
    private void OnTriggerEnter(Collider other)
    {
        // ”ничтожаем игровой объект, который вошел в триггер (т.е. камень)
        Destroy(other.gameObject);
    }
}