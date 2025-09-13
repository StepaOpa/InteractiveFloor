// WaypointEditor.cs
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

// Этот класс по-прежнему нужен для красивого отображения в инспекторе
[CustomEditor(typeof(WaypointLabyrinth))]
public class WaypointLabyrinthEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        WaypointLabyrinth waypoint = (WaypointLabyrinth)target;
        if (waypoint.type == WaypointType.Intersection)
        {
            // Убираем старый слайдер, чтобы не было путаницы
            // waypoint.intersectionNeighborCount = EditorGUILayout.IntSlider("Neighbor Count", waypoint.intersectionNeighborCount, 3, 4);
        }
    }
}

public class WaypointConnector
{
    // НАСТРОЙТЕ ЭТО ЗНАЧЕНИЕ!
    private const float MaxConnectionDistance = 5f;

    [MenuItem("Tools/Labyrinth/Connect Waypoints (Authoritative)")]
    public static void AutoConnectWaypointsAuthoritative()
    {
        WaypointLabyrinth[] allWaypoints = GameObject.FindObjectsOfType<WaypointLabyrinth>();

        // --- ФАЗА 1: Анализ кандидатов ---
        var candidates = new Dictionary<WaypointLabyrinth, List<WaypointLabyrinth>>();
        foreach (var wp in allWaypoints)
        {
            wp.neighbors.Clear(); // Очищаем сразу
            var neighbors = allWaypoints
                .Where(other => other != wp && Vector3.Distance(wp.transform.position, other.transform.position) <= MaxConnectionDistance)
                .OrderBy(other => Vector3.Distance(wp.transform.position, other.transform.position))
                .ToList();
            candidates[wp] = neighbors;
        }

        // --- ФАЗА 2: Принудительное соединение для точек-приказов ---
        // Сначала обрабатываем самые важные точки: перекрестки и тупики
        foreach (var wp in allWaypoints.Where(p => p.type == WaypointType.Intersection || p.type == WaypointType.DeadEnd))
        {
            int desiredCount = wp.GetDesiredNeighborCount();
            var bestCandidates = candidates[wp].Take(desiredCount);

            foreach (var candidate in bestCandidates)
            {
                // ПРИНУДИТЕЛЬНОЕ ВЗАИМНОЕ СОЕДИНЕНИЕ
                ConnectPair(wp, candidate);
            }
        }

        // --- ФАЗА 3: Соединение оставшихся стандартных точек ---
        // Они подбирают себе соседей из тех, у кого еще есть место
        foreach (var wp in allWaypoints.Where(p => p.type == WaypointType.Standard))
        {
            int desiredCount = wp.GetDesiredNeighborCount();
            // Ищем кандидатов, у которых еще не заполнен лимит соседей
            var availableCandidates = candidates[wp]
                .Where(c => c.neighbors.Count < c.GetDesiredNeighborCount())
                .Take(desiredCount - wp.neighbors.Count); // Берем только недостающее количество

            foreach (var candidate in availableCandidates)
            {
                ConnectPair(wp, candidate);
            }
        }

        // Сохраняем все изменения
        foreach (var wp in allWaypoints)
        {
            EditorUtility.SetDirty(wp);
        }

        Debug.Log("Авторитетное соединение завершено! Приказы выполнены.");
    }

    private static void ConnectPair(WaypointLabyrinth a, WaypointLabyrinth b)
    {
        // Проверяем, что они еще не соединены
        if (!a.neighbors.Contains(b)) a.neighbors.Add(b);
        if (!b.neighbors.Contains(a)) b.neighbors.Add(a);
    }
}