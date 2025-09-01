using UnityEngine;

public class IcebreakerCollider : MonoBehaviour
{
    [SerializeField] private IcebreakerController icebreakerController;
    [SerializeField] private float dissapearTime = 1f;

    private void OnCollisionEnter(Collision other)
    {
        // Если столкнулись с маленьким осколком льда
        if (other.gameObject.CompareTag("IceShard"))
        {
            other.gameObject.GetComponent<IceShard>().Dissapear(dissapearTime);
        }
        // Если столкнулись с большим препятствием (айсбергом)
        else if (other.gameObject.CompareTag("Obstacle"))
        {
            Debug.Log($"Collision with obstacle: {other.gameObject.name}");

            // Пытаемся найти на препятствии скрипт "Iceberg"
            Iceberg iceberg = other.gameObject.GetComponent<Iceberg>();

            // Если скрипт нашелся (то есть это наш разрушаемый айсберг)...
            if (iceberg != null)
            {
                // ...даем ему команду сломаться!
                iceberg.BreakApart();
            }

            // Наносим урон ледоколу в любом случае при столкновении с Obstacle
            icebreakerController.TakeDamage(1);
        }
    }
}