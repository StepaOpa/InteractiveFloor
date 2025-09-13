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
        // Сначала рисуем стандартный инспектор.
        DrawDefaultInspector();

        // Получаем наш компонент.
        WaypointLabyrinth waypoint = (WaypointLabyrinth)target;

        // <<< ИСПРАВЛЕНИЕ ЗДЕСЬ: Используем свойство .Type с большой буквы >>>
        // Проверяем, совпадает ли реальное количество соседей с тем, что должно быть.
        if (waypoint.neighbors.Count != waypoint.GetDesiredNeighborCount())
        {
            // Формируем понятное сообщение об ошибке.
            string message = $"Эта точка имеет тип '{waypoint.Type}' и должна иметь {waypoint.GetDesiredNeighborCount()} соседа(ей), но сейчас у нее {waypoint.neighbors.Count}.";

            // Отображаем красивое поле с предупреждением.
            EditorGUILayout.HelpBox(message, MessageType.Warning);
        }
    }

    /// <summary>
    /// Рисует графические элементы в окне сцены для удобства.
    /// </summary>
    [DrawGizmo(GizmoType.NonSelected | GizmoType.Selected | GizmoType.Pickable)]
    public static void OnDrawSceneGizmo(WaypointLabyrinth waypoint, GizmoType gizmoType)
    {
        // <<< ИСПРАВЛЕНИЕ ЗДЕСЬ: Используем свойство .Type с большой буквы >>>
        // Устанавливаем цвет в зависимости от типа точки.
        switch (waypoint.Type)
        {
            case WaypointType.Standard:
                Gizmos.color = new Color(0, 1, 1, 0.7f); // Голубой
                break;
            case WaypointType.Intersection:
                Gizmos.color = new Color(0, 1, 0, 0.7f); // Зеленый
                break;
            case WaypointType.DeadEnd:
                Gizmos.color = new Color(1, 0, 0, 0.7f); // Красный
                break;
        }

        // Рисуем на месте точки сферу, чтобы ее было видно.
        Gizmos.DrawSphere(waypoint.transform.position, 0.2f);

        // Рисуем линии к соседям.
        Gizmos.color = new Color(1, 1, 0, 0.5f); // Желтый
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
    // НАСТРОЙТЕ ЭТО ЗНАЧЕНИЕ!
    // Максимальное расстояние, на котором точки могут соединиться.
    private const float MaxConnectionDistance = 5f;

    /// <summary>
    /// Меню для запуска автоматического соединения вейпоинтов.
    /// </summary>
    [MenuItem("Tools/Labyrinth/Auto-Connect Waypoints")]
    public static void AutoConnectWaypoints()
    {
        WaypointLabyrinth[] allWaypoints = GameObject.FindObjectsOfType<WaypointLabyrinth>();

        // --- ФАЗА 1: Анализ кандидатов ---
        var candidates = new Dictionary<WaypointLabyrinth, List<WaypointLabyrinth>>();
        foreach (var wp in allWaypoints)
        {
            wp.neighbors.Clear(); // Сразу очищаем все старые соединения.
            var potentialNeighbors = allWaypoints
                .Where(other => other != wp && Vector3.Distance(wp.transform.position, other.transform.position) <= MaxConnectionDistance)
                .OrderBy(other => Vector3.Distance(wp.transform.position, other.transform.position))
                .ToList();
            candidates[wp] = potentialNeighbors;
        }

        // --- ФАЗА 2: Принудительное соединение для "командных" точек ---
        // Сначала обрабатываем самые важные точки: перекрестки и тупики.
        // <<< ИСПРАВЛЕНИЕ ЗДЕСЬ: Используем свойство .Type с большой буквы >>>
        foreach (var wp in allWaypoints.Where(p => p.Type == WaypointType.Intersection || p.Type == WaypointType.DeadEnd))
        {
            int desiredCount = wp.GetDesiredNeighborCount();
            var bestCandidates = candidates[wp].Take(desiredCount);

            foreach (var candidate in bestCandidates)
            {
                ConnectPair(wp, candidate); // Принудительно соединяем их.
            }
        }

        // --- ФАЗА 3: Соединение оставшихся стандартных точек ---
        // Они подбирают себе соседей из тех, у кого еще есть свободные "слоты".
        // <<< ИСПРАВЛЕНИЕ ЗДЕСЬ: Используем свойство .Type с большой буквы >>>
        foreach (var wp in allWaypoints.Where(p => p.Type == WaypointType.Standard))
        {
            int desiredCount = wp.GetDesiredNeighborCount();
            var availableCandidates = candidates[wp]
                .Where(c => c.neighbors.Count < c.GetDesiredNeighborCount()) // Ищем только тех, кто еще "не занят".
                .Take(desiredCount - wp.neighbors.Count); // Берем только недостающее количество.

            foreach (var candidate in availableCandidates)
            {
                ConnectPair(wp, candidate);
            }
        }

        // Сохраняем все изменения, чтобы Unity их не потерял.
        foreach (var wp in allWaypoints)
        {
            EditorUtility.SetDirty(wp);
        }

        Debug.Log($"Авто-соединение завершено! Обработано {allWaypoints.Length} точек.");
    }

    /// <summary>
    /// Создает двустороннюю связь между двумя точками.
    /// </summary>
    private static void ConnectPair(WaypointLabyrinth a, WaypointLabyrinth b)
    {
        if (a == null || b == null) return;

        // Добавляем друг друга в соседи, если еще не добавлены.
        if (!a.neighbors.Contains(b)) a.neighbors.Add(b);
        if (!b.neighbors.Contains(a)) b.neighbors.Add(a);
    }
}