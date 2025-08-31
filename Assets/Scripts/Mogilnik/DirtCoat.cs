using UnityEngine;

// Этот компонент вешается на 3D-объект "шубы" из грязи,
// который является дочерним объектом для CollectableItem.
[RequireComponent(typeof(Renderer))]
public class DirtCoat : MonoBehaviour
{
    [Header("Настройки очистки")]
    [Tooltip("Сколько тапов нужно, чтобы полностью очистить предмет")]
    [SerializeField] private int tapsToClean = 3;

    private Renderer coatRenderer;
    private int currentTaps = 0;
    private bool isFullyCleaned = false;

    // Свойство, чтобы CollectableItem мог снаружи узнать, чистая ли шуба
    public bool IsFullyClean => isFullyCleaned;

    void Awake()
    {
        // Получаем компонент рендера, чтобы управлять материалом
        coatRenderer = GetComponent<Renderer>();

        // ВАЖНО: Создаем копию материала, чтобы изменение прозрачности
        // не влияло на все остальные объекты с этим же материалом.
        Material materialInstance = coatRenderer.material;

        // Устанавливаем режим рендера, который поддерживает прозрачность.
        // Без этого альфа-канал работать не будет.
        // Используем стандартные имена свойств шейдера Unity
        materialInstance.SetFloat("_Mode", 3); // 3 = Transparent
        materialInstance.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        materialInstance.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        materialInstance.SetInt("_ZWrite", 0);
        materialInstance.DisableKeyword("_ALPHATEST_ON");
        materialInstance.EnableKeyword("_ALPHABLEND_ON");
        materialInstance.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        materialInstance.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;

        coatRenderer.material = materialInstance;

        // Сразу устанавливаем начальную непрозрачность
        Color initialColor = coatRenderer.material.color;
        initialColor.a = 1f;
        coatRenderer.material.color = initialColor;
    }

    /// <summary>
    /// Этот метод будет вызываться из CollectableItem при каждом тапе.
    /// </summary>
    /// <returns>Возвращает true, если этим тапом предмет был очищен полностью.</returns>
    public bool RegisterTap()
    {
        if (isFullyCleaned)
        {
            return false; // Уже чисто, ничего не делаем
        }

        currentTaps++;

        float newAlpha = (float)(tapsToClean - currentTaps) / tapsToClean;

        Color currentColor = coatRenderer.material.color;
        currentColor.a = newAlpha;
        coatRenderer.material.color = currentColor;

        if (currentTaps >= tapsToClean)
        {
            isFullyCleaned = true;
            gameObject.SetActive(false);
            // ИСПРАВЛЕНО: Явно указываем, какой Debug использовать
            UnityEngine.Debug.Log("Предмет полностью очищен!");
            return true;
        }

        return false;
    }
}