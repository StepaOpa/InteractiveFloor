using UnityEngine;

public class Iceberg : MonoBehaviour
{
    [Header("Части айсберга")]
    [Tooltip("Видимая модель целого айсберга, которая будет выключена")]
    [SerializeField] private GameObject wholeModel;

    [Tooltip("Первая половинка, которая будет включена")]
    [SerializeField] private GameObject half_A;

    [Tooltip("Вторая половинка, которая будет включена")]
    [SerializeField] private GameObject half_B;

    [Header("Настройки разрушения")]
    [SerializeField] private float separationForce = 2f;
    [SerializeField] private float sinkDelay = 1.5f;
    [SerializeField] private float sinkSpeed = 0.2f;

    private bool isBroken = false;

    public void BreakApart()
    {
        if (isBroken) return;
        isBroken = true;

        if (wholeModel != null)
        {
            wholeModel.SetActive(false);
        }
        GetComponent<Collider>().enabled = false;

        half_A.transform.SetParent(null);
        half_B.transform.SetParent(null);

        ActivateHalf(half_A, Vector3.left);
        ActivateHalf(half_B, Vector3.right);

        // Уничтожаем старый пустой объект айсберга, он больше не нужен
        Destroy(gameObject);
    }

    private void ActivateHalf(GameObject half, Vector3 direction)
    {
        half.SetActive(true);
        Rigidbody rb = half.GetComponent<Rigidbody>();

        // ========== НАЧАЛО ИЗМЕНЕНИЙ ==========
        // Находим на половинке наш новый скрипт
        IcebergHalf sinkingScript = half.GetComponent<IcebergHalf>();
        // ========== КОНЕЦ ИЗМЕНЕНИЙ ==========

        if (rb != null && sinkingScript != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.AddForce(direction * separationForce, ForceMode.Impulse);

            // ========== ИЗМЕНЕНИЕ ==========
            // НЕ запускаем корутину здесь, а ДАЕМ КОМАНДУ половинке
            sinkingScript.StartSinking(sinkDelay, sinkSpeed);
            // ===============================
        }
    }
}