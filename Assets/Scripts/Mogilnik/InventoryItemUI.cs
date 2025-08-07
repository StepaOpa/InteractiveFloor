using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InventoryItemUI : MonoBehaviour
{
    [SerializeField] private Image itemImage;
    [SerializeField] private float animationDuration = 0.3f;
    [SerializeField] private AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    void Awake()
    {
        if (itemImage == null)
            itemImage = GetComponent<Image>();
    }

    void Start()
    {
        PlaySpawnAnimation();
    }

    public void SetSprite(Sprite sprite)
    {
        if (itemImage == null)
            itemImage = GetComponent<Image>();

        if (itemImage != null)
        {
            itemImage.sprite = sprite;
            itemImage.preserveAspect = true;
            Debug.Log($"[InventoryItemUI] Спрайт установлен: {sprite?.name ?? "null"}");
        }
        else
        {
            Debug.LogError("[InventoryItemUI] Image компонент не найден!");
        }
    }

    private void PlaySpawnAnimation()
    {
        transform.localScale = Vector3.zero;
        StartCoroutine(ScaleAnimation());
    }

    private IEnumerator ScaleAnimation()
    {
        float elapsedTime = 0f;

        while (elapsedTime < animationDuration)
        {
            elapsedTime += Time.deltaTime;
            float progress = elapsedTime / animationDuration;
            float scaleValue = scaleCurve.Evaluate(progress);
            transform.localScale = Vector3.one * scaleValue;
            yield return null;
        }

        transform.localScale = Vector3.one;
    }
}
