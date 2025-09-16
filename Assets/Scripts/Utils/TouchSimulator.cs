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

        // Создаем PointerEventData с правильными параметрами
        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = screenPosition;
        pointerData.pointerId = 0; // Используем положительный ID
        pointerData.button = PointerEventData.InputButton.Left;
        pointerData.delta = Vector2.zero;
        pointerData.scrollDelta = Vector2.zero;
        pointerData.pressPosition = screenPosition;
        pointerData.clickTime = Time.time;
        pointerData.clickCount = 1;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        if (results.Count > 0)
        {
            GameObject target = results[0].gameObject;
            Debug.Log($"UI клик по: {target.name}");

            // Выполняем полную последовательность событий для корректной работы кнопок
            ExecutePointerEvents(target, pointerData);

            // Также проверяем родительские объекты на наличие кнопок
            Transform parent = target.transform.parent;
            while (parent != null)
            {
                Button parentButton = parent.GetComponent<Button>();
                if (parentButton != null && parentButton.interactable)
                {
                    Debug.Log($"Нажатие родительской кнопки: {parent.name}");
                    ExecutePointerEvents(parent.gameObject, pointerData);
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
    /// Выполняет полную последовательность событий указателя для корректной работы UI элементов
    /// </summary>
    private void ExecutePointerEvents(GameObject target, PointerEventData pointerData)
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

            // Выполняем полную последовательность событий для кнопки
            ExecuteEvents.Execute(target, pointerData, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(target, pointerData, ExecuteEvents.pointerClickHandler);
            ExecuteEvents.Execute(target, pointerData, ExecuteEvents.pointerUpHandler);

            // Дополнительно вызываем onClick напрямую для гарантии
            button.onClick.Invoke();
        }
        else if (toggle != null && toggle.interactable)
        {
            Debug.Log($"Переключение Toggle: {target.name}");
            ExecuteEvents.Execute(target, pointerData, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(target, pointerData, ExecuteEvents.pointerClickHandler);
            ExecuteEvents.Execute(target, pointerData, ExecuteEvents.pointerUpHandler);
        }
        else if (slider != null && slider.interactable)
        {
            Debug.Log($"Взаимодействие со Slider: {target.name}");
            ExecuteEvents.Execute(target, pointerData, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(target, pointerData, ExecuteEvents.pointerClickHandler);
            ExecuteEvents.Execute(target, pointerData, ExecuteEvents.pointerUpHandler);
        }
        else if (inputField != null && inputField.interactable)
        {
            Debug.Log($"Фокус на InputField: {target.name}");
            ExecuteEvents.Execute(target, pointerData, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(target, pointerData, ExecuteEvents.pointerClickHandler);
            ExecuteEvents.Execute(target, pointerData, ExecuteEvents.pointerUpHandler);
        }
        else if (dropdown != null && dropdown.interactable)
        {
            Debug.Log($"Открытие Dropdown: {target.name}");
            ExecuteEvents.Execute(target, pointerData, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(target, pointerData, ExecuteEvents.pointerClickHandler);
            ExecuteEvents.Execute(target, pointerData, ExecuteEvents.pointerUpHandler);
        }
        else
        {
            // Стандартная обработка через EventSystem
            ExecuteEvents.Execute(target, pointerData, ExecuteEvents.pointerDownHandler);
            ExecuteEvents.Execute(target, pointerData, ExecuteEvents.pointerClickHandler);
            ExecuteEvents.Execute(target, pointerData, ExecuteEvents.pointerUpHandler);
        }
    }

    /// <summary>
    /// Универсальный метод для обработки UI элементов (устаревший, используется ExecutePointerEvents)
    /// </summary>
    private void HandleUIElement(GameObject target, PointerEventData pointerData)
    {
        ExecutePointerEvents(target, pointerData);
    }

    /// <summary>
    /// Специальный метод для вызова методов OnPointerDown* из PlayerControllerLabyrinth
    /// </summary>
    public void SimulateButtonClick(string buttonName)
    {
        // Ищем PlayerControllerLabyrinth в сцене
        PlayerControllerLabyrinth playerController = FindFirstObjectByType<PlayerControllerLabyrinth>();
        if (playerController == null)
        {
            Debug.LogWarning("PlayerControllerLabyrinth не найден в сцене!");
            return;
        }

        // Вызываем соответствующий метод в зависимости от имени кнопки
        switch (buttonName.ToLower())
        {
            case "forward":
            case "вперед":
                playerController.OnPointerDownForward();
                Debug.Log("Симулировано нажатие кнопки 'Вперед'");
                break;
            case "back":
            case "назад":
                playerController.OnPointerDownBack();
                Debug.Log("Симулировано нажатие кнопки 'Назад'");
                break;
            case "left":
            case "влево":
                playerController.OnPointerDownLeft();
                Debug.Log("Симулировано нажатие кнопки 'Влево'");
                break;
            case "right":
            case "вправо":
                playerController.OnPointerDownRight();
                Debug.Log("Симулировано нажатие кнопки 'Вправо'");
                break;
            default:
                Debug.LogWarning($"Неизвестное имя кнопки: {buttonName}");
                break;
        }
    }

    /// <summary>
    /// Улучшенный метод ClickAt с автоматическим распознаванием кнопок управления
    /// </summary>
    public void ClickAtWithSmartDetection(Vector2 screenPosition)
    {
        // Сначала пробуем стандартный метод
        ClickAt(screenPosition);

        // Дополнительно проверяем, не является ли это кнопкой управления
        if (EventSystem.current == null) return;

        PointerEventData pointerData = new PointerEventData(EventSystem.current);
        pointerData.position = screenPosition;
        pointerData.pointerId = 0;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        if (results.Count > 0)
        {
            GameObject target = results[0].gameObject;
            string objectName = target.name.ToLower();

            // Проверяем, содержит ли имя объекта ключевые слова для кнопок управления
            if (objectName.Contains("forward") || objectName.Contains("вперед") || objectName.Contains("up"))
            {
                SimulateButtonClick("forward");
            }
            else if (objectName.Contains("back") || objectName.Contains("назад") || objectName.Contains("down"))
            {
                SimulateButtonClick("back");
            }
            else if (objectName.Contains("left") || objectName.Contains("влево"))
            {
                SimulateButtonClick("left");
            }
            else if (objectName.Contains("right") || objectName.Contains("вправо"))
            {
                SimulateButtonClick("right");
            }
        }
    }

}