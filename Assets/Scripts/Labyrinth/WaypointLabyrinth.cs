// WaypointLabyrinth.cs
using UnityEngine;
using System.Collections.Generic;

// Создаем четкие типы для наших точек
public enum WaypointType
{
    Standard,     // Обычная точка в коридоре (всегда 2 соседа)
    Intersection, // Перекресток (3 или 4 соседа)
    DeadEnd       // Тупик (всегда 1 сосед)
}

public class WaypointLabyrinth : MonoBehaviour
{
    [Header("Настройки точки")]
    [Tooltip("Выберите тип этой точки из списка")]
    public WaypointType type = WaypointType.Standard;

    [Tooltip("ДЛЯ ПЕРЕКРЕСТКОВ: Укажите точное количество выходов (3 или 4)")]
    [Range(3, 4)] // Ограничиваем ввод, чтобы избежать ошибок
    public int intersectionNeighborCount = 3;

    [Header("Соединения")]
    [Tooltip("Список соседей. Заполняется автоматически.")]
    public List<WaypointLabyrinth> neighbors = new List<WaypointLabyrinth>();

    // Вспомогательный метод, чтобы узнать, сколько соседей должно быть у точки
    public int GetDesiredNeighborCount()
    {
        switch (type)
        {
            case WaypointType.Standard:
                return 2;
            case WaypointType.Intersection:
                return intersectionNeighborCount;
            case WaypointType.DeadEnd:
                return 1;
            default:
                return 2;
        }
    }
}