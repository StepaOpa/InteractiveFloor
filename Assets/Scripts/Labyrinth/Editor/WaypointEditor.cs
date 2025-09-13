using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;


/// <summary>
/// Кастомный редактор для компонента WaypointLabyrinth.
/// Добавляет визуальные подсказки в инспекторе и в окне сцены.
/// </summary>
[CustomEditor(typeof(WaypointLabyrinth))]
public class WaypointLabyrinthEditor : Editor
{
    /// <summary>
    /// Рисует кастомный интерфейс в инспекторе.
    /// </summary>
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        WaypointLabyrinth waypoint = (WaypointLabyrinth)target;

        if (waypoint.neighbors.Count != waypoint.GetDesiredNeighborCount())
        {
            string message = $"Эта точка имеет тип '{waypoint.Type}' и должна иметь {waypoint.GetDesiredNeighborCount()} соседа(ей), но сейчас у нее {waypoint.neighbors.Count}.";
            EditorGUILayout.HelpBox(message, MessageType.Warning);
        }
    }

    /// <summary>
    /// Этот метод отвечает за отрисовку Гизмо в окне сцены для WaypointLabyrinth.
    /// </summary>
    [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.Pickable)]
    public static void OnDrawSceneGizmo(WaypointLabyrinth waypoint, GizmoType gizmoType)
    {
        // Устанавливаем цвет в зависимости от типа точки
        switch (waypoint.Type)
        {
            case WaypointType.Standard:
                Gizmos.color = new Color(0, 0.8f, 1f, 0.7f); // Голубой
                break;
            case WaypointType.Intersection:
                Gizmos.color = new Color(0, 1f, 0, 0.7f);   // Зеленый
                break;
            case WaypointType.DeadEnd:
                Gizmos.color = new Color(1f, 0, 0, 0.7f);     // Красный
                break;
        }

        // <<< ИЗМЕНЕНИЕ ЗДЕСЬ: Уменьшаем радиус сферы >>>
        // Раньше было 0.2f, теперь 0.05f. Можете поставить любое другое значение.
        Gizmos.DrawSphere(waypoint.transform.position, 0.05f);

        // Рисуем линии к соседям
        Gizmos.color = new Color(1f, 0.9f, 0, 0.5f); // Желтый
        if (waypoint.neighbors != null)
        {
            foreach (var neighbor in waypoint.neighbors)
            {
                if (neighbor != null)
                {
                    Gizmos.DrawLine(waypoint.transform.position, neighbor.transform.position);
                }
            }
        }
    }
}


/// <summary>
/// Класс-инструмент для автоматического соединения точек лабиринта в редакторе.
/// </summary>
public class WaypointConnector
{
    private const float MaxConnectionDistance = 5f;

    [MenuItem("Tools/Labyrinth/Auto-Connect Waypoints")]
    public static void AutoConnectWaypoints()
    {
        WaypointLabyrinth[] allWaypoints = GameObject.FindObjectsOfType<WaypointLabyrinth>();
        var candidates = new Dictionary<WaypointLabyrinth, List<WaypointLabyrinth>>();

        foreach (var wp in allWaypoints)
        {
            wp.neighbors.Clear();
            var potentialNeighbors = allWaypoints
                .Where(other => other != wp && Vector3.Distance(wp.transform.position, other.transform.position) <= MaxConnectionDistance)
                .OrderBy(other => Vector3.Distance(wp.transform.position, other.transform.position))
                .ToList();
            candidates[wp] = potentialNeighbors;
        }

        foreach (var wp in allWaypoints.Where(p => p.Type == WaypointType.Intersection || p.Type == WaypointType.DeadEnd))
        {
            int desiredCount = wp.GetDesiredNeighborCount();
            var bestCandidates = candidates[wp].Take(desiredCount);
            foreach (var candidate in bestCandidates)
            {
                ConnectPair(wp, candidate);
            }
        }

        foreach (var wp in allWaypoints.Where(p => p.Type == WaypointType.Standard))
        {
            int desiredCount = wp.GetDesiredNeighborCount();
            var availableCandidates = candidates[wp]
                .Where(c => c.neighbors.Count < c.GetDesiredNeighborCount())
                .Take(desiredCount - wp.neighbors.Count);
            foreach (var candidate in availableCandidates)
            {
                ConnectPair(wp, candidate);
            }
        }

        foreach (var wp in allWaypoints)
        {
            EditorUtility.SetDirty(wp);
        }
        Debug.Log($"Авто-соединение завершено! Обработано {allWaypoints.Length} точек.");
    }

    private static void ConnectPair(WaypointLabyrinth a, WaypointLabyrinth b)
    {
        if (a == null || b == null) return;
        if (!a.neighbors.Contains(b)) a.neighbors.Add(b);
        if (!b.neighbors.Contains(a)) b.neighbors.Add(a);
    }
}