using UnityEngine;

public class IcebreakerCollider : MonoBehaviour
{
    [SerializeField] private IcebreakerController icebreakerController;
    [SerializeField] private float dissapearTime = 1f;

    private void OnCollisionEnter(Collision other)
    {
        // ���� ����������� � ��������� �������� ����
        if (other.gameObject.CompareTag("IceShard"))
        {
            other.gameObject.GetComponent<IceShard>().Dissapear(dissapearTime);
        }
        // ���� ����������� � ������� ������������ (���������)
        else if (other.gameObject.CompareTag("Obstacle"))
        {
            Debug.Log($"Collision with obstacle: {other.gameObject.name}");

            // �������� ����� �� ����������� ������ "Iceberg"
            Iceberg iceberg = other.gameObject.GetComponent<Iceberg>();

            // ���� ������ ������� (�� ���� ��� ��� ����������� �������)...
            if (iceberg != null)
            {
                // ...���� ��� ������� ���������!
                iceberg.BreakApart();
            }

            // ������� ���� �������� � ����� ������ ��� ������������ � Obstacle
            icebreakerController.TakeDamage(1);
        }
    }
}