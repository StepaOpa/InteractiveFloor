using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Reflection;

public class TouchSimulator : MonoBehaviour
{
    [SerializeField] private Camera eventCamera;

    private void Start()
    {
        if (eventCamera == null)
            eventCamera = Camera.main;
    }

    public void ClickAt(Vector2 screenPosition)
    {
        // Проверяем наличие EventSystem
        if (EventSystem.current == null)
        {
            Debug.LogWarning("EventSystem не найден!");
            return;
        }

        // Обработка UI
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = screenPosition;
        pointerData.pointerId = -1; // Используем отрицательный ID для симулированных касаний

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        if (results.Count > 0)
        {
            GameObject target = results[0].gameObject;
            Debug.Log($"UI клик по: {target.name}");

            // Универсальная обработка UI элементов
            HandleUIElement(target, pointerData);

            // Также проверяем родительские объекты на наличие кнопок
            Transform parent = target.transform.parent;
            while (parent != null)
            {
                Button parentButton = parent.GetComponent<Button>();
                if (parentButton != null && parentButton.interactable)
                {
                    Debug.Log($"Нажатие родительской кнопки: {parent.name}");
                    parentButton.onClick.Invoke();
                    break; // Останавливаемся на первой найденной родительской кнопке
                }
                parent = parent.parent;
            }
        }
        else
        {
            Debug.Log("UI объекты не найдены, проверяем 3D объекты");

            // Обработка 3D объектов
            if (eventCamera != null)
            {
                Ray ray = eventCamera.ScreenPointToRay(screenPosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit))
                {
                    Debug.Log($"3D клик по: {hit.collider.gameObject.name}");

                    // Вызываем OnMouseDown() если он есть
                    MonoBehaviour[] components = hit.collider.gameObject.GetComponents<MonoBehaviour>();
                    foreach (MonoBehaviour component in components)
                    {
                        MethodInfo onMouseDownMethod = component.GetType().GetMethod("OnMouseDown",
                            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                        if (onMouseDownMethod != null)
                        {
                            Debug.Log($"Вызываем OnMouseDown() для {component.GetType().Name}");
                            onMouseDownMethod.Invoke(component, null);
                        }
                    }

                    // Также выполняем события UI системы
                    ExecuteEvents.Execute(hit.collider.gameObject, pointerData, ExecuteEvents.pointerDownHandler);
                    ExecuteEvents.Execute(hit.collider.gameObject, pointerData, ExecuteEvents.pointerClickHandler);
                    ExecuteEvents.Execute(hit.collider.gameObject, pointerData, ExecuteEvents.pointerUpHandler);
                }
                else
                {
                    Debug.Log("3D объекты не найдены");
                }
            }
        }
    }

    /// <summary>
    /// Универсальный метод для обработки UI элементов
    /// </summary>
    private void HandleUIElement(GameObject target, PointerEventData pointerData)
    {
        // Проверяем различные типы UI элементов
        Button button = target.GetComponent<Button>();
        Toggle toggle = target.GetComponent<Toggle>();
        Slider slider = target.GetComponent<Slider>();
        InputField inputField = target.GetComponent<InputField>();
        Dropdown dropdown = target.GetComponent<Dropdown>();

        if (button != null && button.interactable)
        {
            Debug.Log($"Нажатие кнопки: {target.name}");
            button.onClick.Invoke();
        }
        else if (toggle != null && toggle.interactable)
        {
            Debug.Log($"Переключение Toggle: {target.name}");
            toggle.isOn = !toggle.isOn;
        }
        else if (slider != null && slider.interactable)
        {
            Debug.Log($"Взаимодействие со Slider: {target.name}");
            // Для слайдера можно добавить логику изменения значения
        }
        else if (inputField != null && inputField.interactable)
        {
            Debug.Log($"Фокус на InputField: {target.name}");
            inputField.Select();
        }
        else if (dropdown != null && dropdown.interactable)
        {
            Debug.Log($"Открытие Dropdown: {target.name}");
            dropdown.Show();
        }
        else
        {
            // Стандартная обработка через EventSystem
            ExecuteEvents.Execute(target, pointerData, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(target, pointerData, ExecuteEvents.pointerClickHandler);
            ExecuteEvents.Execute(target, pointerData, ExecuteEvents.pointerUpHandler);
        }
    }

}