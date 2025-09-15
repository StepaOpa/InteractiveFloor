using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Определяет функциональный тип точки в лабиринте.
/// </summary>
public enum WaypointType
{
    /// <summary>
    /// Обычная точка на прямом участке пути. Должна иметь ровно 2 соседа.
    /// </summary>
    Standard,

    /// <summary>
    /// Перекресток или поворот, где игрок должен принимать решение. Имеет 3 или 4 соседа.
    /// </summary>
    Intersection,

    /// <summary>
    /// Тупик. Конечная точка пути, имеет только 1 соседа.
    /// </summary>
    DeadEnd
}

/// <summary>
/// Компонент, представляющий собой узел (точку) в навигационной сетке лабиринта.
/// </summary>
public class WaypointLabyrinth : MonoBehaviour
{
    [Header("Настройки точки")]

    [Tooltip("Функциональный тип этой точки (прямой участок, перекресток или тупик).")]
    [SerializeField]
    private WaypointType _type = WaypointType.Standard;
    public WaypointType Type => _type;


    [Tooltip("ДЛЯ ПЕРЕКРЕСТКОВ: Укажите точное количество выходов (3 или 4).")]
    [Range(3, 4)]
    [SerializeField]
    private int _intersectionNeighborCount = 3;


    [Header("Соединения")]

    [Tooltip("Список соседних точек, к которым можно двигаться из этой. Заполняется вручную или редактором.")]
    public List<WaypointLabyrinth> neighbors = new List<WaypointLabyrinth>();

    /// <summary>
    /// Возвращает ожидаемое количество соседей для точки в зависимости от ее типа.
    /// </summary>
    public int GetDesiredNeighborCount()
    {
        switch (_type)
        {
            case WaypointType.Standard:
                return 2;

            case WaypointType.Intersection:
                return _intersectionNeighborCount;

            case WaypointType.DeadEnd:
                return 1;

            default:
                return 2;
        }
    }

    // <<< ВОТ ОНО, ГЛАВНОЕ ДОБАВЛЕНИЕ >>>
    /// <summary>
    /// Этот специальный метод Unity вызывается автоматически для отрисовки графики в окне редактора.
    /// Он не работает в самой игре, только во вьюпорте.
    /// </summary>
    private void OnDrawGizmos()
    {
        // --- Шаг 1: Рисуем саму точку ---

        // Устанавливаем цвет в зависимости от типа точки
        switch (Type)
        {
            case WaypointType.Standard:
                Gizmos.color = new Color(0, 0.8f, 1f, 0.7f); // Ярко-голубой для обычных точек
                break;
            case WaypointType.Intersection:
                Gizmos.color = new Color(0, 1f, 0, 0.7f);   // Зеленый для перекрестков
                break;
            case WaypointType.DeadEnd:
                Gizmos.color = new Color(1f, 0, 0, 0.7f);     // Красный для тупиков
                break;
        }

        // Рисуем на месте точки полупрозрачную сферу, чтобы ее было хорошо видно.
        Gizmos.DrawSphere(transform.position, 0.05f);

        // --- Шаг 2: Рисуем линии к соседям ---

        // Устанавливаем желтый цвет для линий
        Gizmos.color = new Color(1f, 0.9f, 0, 0.5f);

        // Проходим по списку всех соседей
        if (neighbors != null)
        {
            foreach (var neighbor in neighbors)
            {
                // Если сосед не пустой (на случай, если вы случайно оставили пустое поле в списке)
                if (neighbor != null)
                {
                    // Рисуем линию от этой точки к соседу
                    Gizmos.DrawLine(transform.position, neighbor.transform.position);
                }
            }
        }
    }
}