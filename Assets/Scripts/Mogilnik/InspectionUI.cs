using UnityEngine;
using UnityEngine.UI;

public class InspectionUI : MonoBehaviour
{
    // --- ВОЗВРАЩАЕМ КНОПКИ ---
    [Header("UI Кнопки вращения")]
    [SerializeField] private Button upButton;
    [SerializeField] private Button downButton;
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    // -------------------------

    [Header("Дополнительные кнопки")]
    [SerializeField] private Button collectButton;
    [SerializeField] private Button cancelButton;

    [Header("Панель UI")]
    [SerializeField] private GameObject inspectionPanel;
    
    private CollectableItem currentItem;
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
        Debug.Log("[InspectionUI] Инициализация кнопок управления...");

        // --- ВОЗВРАЩАЕМ ЛОГИКУ КНОПОК ---
        if (upButton != null)
            upButton.onClick.AddListener(() => RotateCurrentItem("up"));
        else
            Debug.LogWarning("[InspectionUI] Up Button не назначена!");

        if (downButton != null)
            downButton.onClick.AddListener(() => RotateCurrentItem("down"));
        else
            Debug.LogWarning("[InspectionUI] Down Button не назначена!");

        if (leftButton != null)
            leftButton.onClick.AddListener(() => RotateCurrentItem("left"));
        else
            Debug.LogWarning("[InspectionUI] Left Button не назначена!");

        if (rightButton != null)
            rightButton.onClick.AddListener(() => RotateCurrentItem("right"));
        else
            Debug.LogWarning("[InspectionUI] Right Button не назначена!");
        // -------------------------------

        if (collectButton != null)
            collectButton.onClick.AddListener(CollectCurrentItem);
        else
            Debug.LogWarning("[InspectionUI] Collect Button не назначена!");

        if (cancelButton != null)
            cancelButton.onClick.AddListener(CancelInspection);
        else
            Debug.LogWarning("[InspectionUI] Cancel Button не назначена!");
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
        currentItem = null;
        if (inspectionPanel != null)
        {
            inspectionPanel.SetActive(false);
        }
    }

    // --- ВОЗВРАЩАЕМ МЕТОД ВРАЩЕНИЯ ---
    private void RotateCurrentItem(string direction)
    {
        if (currentItem != null && currentItem.IsBeingInspected())
        {
            currentItem.RotateByButton(direction);
        }
        else
        {
            Debug.LogWarning("[InspectionUI] Нет предмета для вращения!");
        }
    }
    // ---------------------------------

    private void CollectCurrentItem() { if (currentItem != null && currentItem.IsBeingInspected()) { currentItem.CollectItemPublic(); } }
    private void CancelInspection() { if (currentItem != null && currentItem.IsBeingInspected()) { currentItem.ExitInspectionPublic(); } }
    public bool IsInspectionUIActive() { return currentItem != null && inspectionPanel != null && inspectionPanel.activeInHierarchy; }
    public void ForceHide() { if (inspectionPanel != null && inspectionPanel.activeSelf) { HideInspectionUI(); } }
}