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
    [Range(3, 4)] // Ограничиваем ввод, чтобы избежать ошибок
    [SerializeField]
    private int _intersectionNeighborCount = 3;


    [Header("Соединения")]

    [Tooltip("Список соседних точек, к которым можно двигаться из этой. Заполняется вручную или редактором.")]
    public List<WaypointLabyrinth> neighbors = new List<WaypointLabyrinth>();

    /// <summary>
    /// Возвращает ожидаемое количество соседей для точки в зависимости от ее типа.
    /// </summary>
    /// <returns>Целое число (1, 2, 3 или 4).</returns>
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
                // На случай, если появится новый тип, возвращаем стандартное значение
                return 2;
        }
    }
}