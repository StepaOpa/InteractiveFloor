using UnityEngine;
using UnityEngine.UI;

public class InspectionUI : MonoBehaviour
{
    [Header("UI Кнопки вращения")]
    [SerializeField] private Button upButton;
    [SerializeField] private Button downButton;
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;

    [Header("Дополнительные кнопки")]
    [SerializeField] private Button collectButton;
    [SerializeField] private Button cancelButton;

    [Header("Панель UI")]
    [SerializeField] private GameObject inspectionPanel;

    [Header("Эффекты")]
    [SerializeField] private GameObject dustCloudEffectPrefab;

    private CollectableItem currentItem;
    private GameObject currentDustEffectInstance;
    private ParticleSystem dustParticleSystem;
    public static InspectionUI Instance { get; private set; }

    // --- НАШИ ИЗМЕНЕНИЯ ЗДЕСЬ ---
    private float lastClickTime = -1f; // Время последнего клика. -1f чтобы первый клик всегда срабатывал.
    private const float clickCooldown = 1.0f; // Задержка в 1 секунду.
    // ----------------------------

    void Awake()
    {
        if (Instance == null) { Instance = this; }
        else { Destroy(gameObject); return; }
    }

    void Start()
    {
        InitializeButtons();
        HideInspectionUI();
    }

    // Возвращаемся к вашему оригинальному методу инициализации
    private void InitializeButtons()
    {
        if (upButton != null) upButton.onClick.AddListener(() => RotateCurrentItem("up"));
        if (downButton != null) downButton.onClick.AddListener(() => RotateCurrentItem("down"));
        if (leftButton != null) leftButton.onClick.AddListener(() => RotateCurrentItem("left"));
        if (rightButton != null) rightButton.onClick.AddListener(() => RotateCurrentItem("right"));
        if (collectButton != null) collectButton.onClick.AddListener(CollectCurrentItem);
        if (cancelButton != null) cancelButton.onClick.AddListener(CancelInspection);
    }

    // --- И ЗДЕСЬ МЕНЯЕМ ЛОГИКУ ---
    private void RotateCurrentItem(string direction)
    {
        // Проверяем, прошло ли достаточно времени с последнего клика
        if (Time.time - lastClickTime < clickCooldown)
        {
            return; // Если не прошло, выходим из функции
        }

        if (currentItem != null && currentItem.IsBeingInspected())
        {
            // Обновляем время последнего клика на текущее
            lastClickTime = Time.time;

            // Выполняем вращение
            currentItem.RotateByButton(direction);
        }
    }
    // ----------------------------

    public void ShowInspectionUI(CollectableItem item)
    {
        currentItem = item;
        if (inspectionPanel != null)
        {
            inspectionPanel.SetActive(true);
        }
    }

    public void HideInspectionUI()
    {
        DestroyDustEffect();
        currentItem = null;
        if (inspectionPanel != null)
        {
            inspectionPanel.SetActive(false);
        }
    }

    public void CreateDustEffect(Transform parentTransform, Vector3 spawnPosition)
    {
        if (dustCloudEffectPrefab != null && currentDustEffectInstance == null)
        {
            currentDustEffectInstance = Instantiate(dustCloudEffectPrefab, spawnPosition, Quaternion.identity);
            currentDustEffectInstance.transform.SetParent(parentTransform);
            dustParticleSystem = currentDustEffectInstance.GetComponent<ParticleSystem>();
        }
    }

    public void PlayDustEffect()
    {
        if (dustParticleSystem != null && !dustParticleSystem.isPlaying)
        {
            dustParticleSystem.Play();
        }
    }

    public void StopDustEffect()
    {
        if (dustParticleSystem != null && dustParticleSystem.isPlaying)
        {
            dustParticleSystem.Stop();
        }
    }

    public void DestroyDustEffect()
    {
        if (currentDustEffectInstance != null)
        {
            Destroy(currentDustEffectInstance);
            currentDustEffectInstance = null;
            dustParticleSystem = null;
        }
    }

    private void CollectCurrentItem()
    {
        if (currentItem != null && currentItem.IsBeingInspected())
        {
            currentItem.CollectItemPublic();
        }
    }

    private void CancelInspection()
    {
        if (currentItem != null && currentItem.IsBeingInspected())
        {
            currentItem.ExitInspectionPublic();
        }
    }

    public bool IsInspectionUIActive()
    {
        return currentItem != null && inspectionPanel != null && inspectionPanel.activeInHierarchy;
    }

    public void ForceHide()
    {
        if (inspectionPanel != null && inspectionPanel.activeSelf)
        {
            HideInspectionUI();
        }
    }
}