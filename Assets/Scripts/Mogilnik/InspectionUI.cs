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

    private void InitializeButtons()
    {
        if (upButton != null) upButton.onClick.AddListener(() => RotateCurrentItem("up"));
        if (downButton != null) downButton.onClick.AddListener(() => RotateCurrentItem("down"));
        if (leftButton != null) leftButton.onClick.AddListener(() => RotateCurrentItem("left"));
        if (rightButton != null) rightButton.onClick.AddListener(() => RotateCurrentItem("right"));
        if (collectButton != null) collectButton.onClick.AddListener(CollectCurrentItem);
        if (cancelButton != null) cancelButton.onClick.AddListener(CancelInspection);
    }

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

    private void RotateCurrentItem(string direction)
    {
        if (currentItem != null && currentItem.IsBeingInspected())
        {
            currentItem.RotateByButton(direction);
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