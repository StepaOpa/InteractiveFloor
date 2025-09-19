using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// Финальная версия инструмента для авто-соединения точек лабиринта.
/// Использует "волновой" алгоритм, основанный на поиске ближайшего соседа (как в отладочной версии).
/// </summary>
[InitializeOnLoad]
public class WaypointEditor
{
    private const float MaxConnectionDistance = 5f;

    [MenuItem("Tools/Labyrinth/Auto-Connect Waypoints (Final)")]
    public static void AutoConnectWaypoints()
    {
        Debug.Log("Запуск финального алгоритма авто-соединения...");

        // Находим все существующие точки на сцене
        WaypointLabyrinth[] allWaypoints = GameObject.FindObjectsOfType<WaypointLabyrinth>()
                                                     .Where(wp => wp != null).ToArray();

        if (allWaypoints.Length == 0)
        {
            Debug.LogWarning("На сцене не найдено ни одной существующей точки (WaypointLabyrinth).");
            return;
        }

        // Шаг 0: Очистка старых соединений
        foreach (var wp in allWaypoints)
        {
            if (wp.neighbors != null) wp.neighbors.Clear();
        }

        // Шаг 1: Поиск и кэширование всех потенциальных соседей по дистанции
        var candidates = new Dictionary<WaypointLabyrinth, List<WaypointLabyrinth>>();
        foreach (var wp in allWaypoints)
        {
            var potentialNeighbors = allWaypoints
                .Where(other => other != null && other != wp && Vector3.Distance(wp.transform.position, other.transform.position) <= MaxConnectionDistance)
                .OrderBy(other => Vector3.Distance(wp.transform.position, other.transform.position))
                .ToList();
            candidates[wp] = potentialNeighbors;
        }

        // Шаг 2: Соединение "якорей" (перекрестков и тупиков)
        var priorityWaypoints = allWaypoints
            .Where(p => p != null && (p.Type == WaypointType.Intersection || p.Type == WaypointType.DeadEnd))
            .OrderBy(p => p.Type);

        foreach (var wp in priorityWaypoints)
        {
            int desiredCount = wp.GetDesiredNeighborCount();
            var bestCandidates = candidates[wp]
                .Where(c => c != null && c.neighbors.Count < c.GetDesiredNeighborCount())
                .Take(desiredCount - wp.neighbors.Count);

            foreach (var candidate in bestCandidates)
            {
                ConnectPair(wp, candidate);
            }
        }

        // Шаг 3: "Волновое" построение коридоров (логика из отладки)
        var processingQueue = new Queue<WaypointLabyrinth>();

        // Находим стартовые точки для коридоров
        foreach (var wp in allWaypoints.Where(p => p != null && p.Type == WaypointType.Standard && p.neighbors.Count == 1))
        {
            processingQueue.Enqueue(wp);
        }

        while (processingQueue.Count > 0)
        {
            var currentWp = processingQueue.Dequeue();

            if (currentWp == null || currentWp.neighbors.Count >= currentWp.GetDesiredNeighborCount())
            {
                continue;
            }

            // --- ВОЗВРАЩЕННАЯ ЛОГИКА ---
            // Ищем первого доступного соседа из списка, отсортированного по дистанции.
            var nextNeighbor = candidates[currentWp]
                .FirstOrDefault(c =>
                    c != null &&
                    c.Type == WaypointType.Standard &&
                    c.neighbors.Count < c.GetDesiredNeighborCount() &&
                    !currentWp.neighbors.Contains(c)
                );

            if (nextNeighbor != null)
            {
                ConnectPair(currentWp, nextNeighbor);
                // Если новая точка еще не заполнена, добавляем ее в очередь
                if (nextNeighbor.neighbors.Count < nextNeighbor.GetDesiredNeighborCount())
                {
                    processingQueue.Enqueue(nextNeighbor);
                }
            }
        }

        // Финальный шаг: Сохранение изменений
        foreach (var wp in allWaypoints)
        {
            if (wp != null)
            {
                EditorUtility.SetDirty(wp);
            }
        }

        Debug.Log($"Авто-соединение завершено! Обработано {allWaypoints.Length} точек. Алгоритм: Wave.");
    }

    private static void ConnectPair(WaypointLabyrinth a, WaypointLabyrinth b)
    {
        if (a == null || b == null) return;
        if (!a.neighbors.Contains(b)) a.neighbors.Add(b);
        if (!b.neighbors.Contains(a)) b.neighbors.Add(a);
    }
}