using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// Умный инструмент для автоматического соединения точек лабиринта.
/// Находится в меню "Tools/Labyrinth/Auto-Connect Waypoints (Advanced)".
/// </summary>
[InitializeOnLoad] // Этот атрибут нужен, чтобы класс был активен в редакторе
public class WaypointEditor
{
    private const float MaxConnectionDistance = 5f;

    [MenuItem("Tools/Labyrinth/Auto-Connect Waypoints (Advanced)")]
    public static void AutoConnectWaypoints()
    {
        Debug.Log("Запуск улучшенного алгоритма авто-соединения...");

        WaypointLabyrinth[] allWaypoints = GameObject.FindObjectsOfType<WaypointLabyrinth>();
        if (allWaypoints.Length == 0)
        {
            Debug.LogWarning("На сцене не найдено ни одной точки (WaypointLabyrinth).");
            return;
        }

        // Шаг 0: Очистка
        foreach (var wp in allWaypoints)
        {
            if (wp.neighbors != null) wp.neighbors.Clear();
        }

        // Шаг 1: Поиск всех потенциальных соседей
        var candidates = new Dictionary<WaypointLabyrinth, List<WaypointLabyrinth>>();
        foreach (var wp in allWaypoints)
        {
            var potentialNeighbors = allWaypoints
                .Where(other => other != wp && Vector3.Distance(wp.transform.position, other.transform.position) <= MaxConnectionDistance)
                .OrderBy(other => Vector3.Distance(wp.transform.position, other.transform.position))
                .ToList();
            candidates[wp] = potentialNeighbors;
        }

        // Шаг 2: Соединение "якорей" (сначала тупики, потом перекрестки для стабильности)
        var priorityWaypoints = allWaypoints
            .Where(p => p.Type == WaypointType.Intersection || p.Type == WaypointType.DeadEnd)
            .OrderBy(p => p.Type); // Сначала DeadEnd, потом Intersection - это важно!

        foreach (var wp in priorityWaypoints)
        {
            int desiredCount = wp.GetDesiredNeighborCount();
            var bestCandidates = candidates[wp]
                .Where(c => c.neighbors.Count < c.GetDesiredNeighborCount())
                .Take(desiredCount - wp.neighbors.Count);

            foreach (var candidate in bestCandidates)
            {
                ConnectPair(wp, candidate);
            }
        }

        // Шаг 3: "Волновое" построение коридоров с учетом направления
        var processingQueue = new Queue<WaypointLabyrinth>();

        // Находим стартовые точки
        foreach (var wp in allWaypoints.Where(p => p.Type == WaypointType.Standard && p.neighbors.Count == 1))
        {
            processingQueue.Enqueue(wp);
        }

        while (processingQueue.Count > 0)
        {
            var currentWp = processingQueue.Dequeue();

            if (currentWp.neighbors.Count >= currentWp.GetDesiredNeighborCount())
            {
                continue;
            }

            // --- КЛЮЧЕВОЕ УЛУЧШЕНИЕ ЗДЕСЬ ---
            // Определяем направление, откуда мы пришли
            var previousWp = currentWp.neighbors[0];
            Vector3 forwardDirection = (currentWp.transform.position - previousWp.transform.position).normalized;

            // Ищем лучшего соседа, который продолжает это направление
            var nextNeighbor = candidates[currentWp]
                .Where(c =>
                    c.Type == WaypointType.Standard &&
                    c.neighbors.Count < c.GetDesiredNeighborCount() &&
                    !currentWp.neighbors.Contains(c)
                )
                // Сортируем кандидатов: чем ближе направление к "прямо", тем лучше
                .OrderByDescending(c => Vector3.Dot(forwardDirection, (c.transform.position - currentWp.transform.position).normalized))
                .FirstOrDefault();

            if (nextNeighbor != null)
            {
                ConnectPair(currentWp, nextNeighbor);
                // Если новая точка еще не заполнена, добавляем ее в очередь для продолжения коридора
                if (nextNeighbor.neighbors.Count < nextNeighbor.GetDesiredNeighborCount())
                {
                    processingQueue.Enqueue(nextNeighbor);
                }
            }
        }

        // Финальный шаг: Сохранение
        foreach (var wp in allWaypoints)
        {
            EditorUtility.SetDirty(wp);
        }

        Debug.Log($"Авто-соединение завершено! Обработано {allWaypoints.Length} точек. Алгоритм: Advanced.");
    }

    private static void ConnectPair(WaypointLabyrinth a, WaypointLabyrinth b)
    {
        if (a == null || b == null) return;
        if (!a.neighbors.Contains(b)) a.neighbors.Add(b);
        if (!b.neighbors.Contains(a)) b.neighbors.Add(a);
    }
}