using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.Collections;


/// <summary>
/// Окно редактора для пошагового соединения точек лабиринта.
/// Использует "волновой" алгоритм для построения логичных коридоров.
/// </summary>
public class WaypointConnectorEditorWindow : EditorWindow
{
    private IEnumerator _connectorCoroutine;

    [MenuItem("Tools/Labyrinth/Step-by-Step Connector")]
    public static void ShowWindow()
    {
        GetWindow<WaypointConnectorEditorWindow>("Labyrinth Connector");
    }

    private void OnGUI()
    {
        GUILayout.Label("Управление авто-соединением", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (_connectorCoroutine == null)
        {
            if (GUILayout.Button("Начать пошаговое соединение", GUILayout.Height(30)))
            {
                _connectorCoroutine = AutoConnectWaypoints();
                _connectorCoroutine.MoveNext();
            }
        }
        else
        {
            if (GUILayout.Button("Следующий шаг", GUILayout.Height(30)))
            {
                if (!_connectorCoroutine.MoveNext())
                {
                    Debug.Log("Пошаговое соединение завершено!");
                    _connectorCoroutine = null;
                }
            }

            GUILayout.Space(10);
            if (GUILayout.Button("Сбросить и начать заново"))
            {
                _connectorCoroutine = null;
                ClearAllConnections();
            }
        }

        GUILayout.FlexibleSpace();
        EditorGUILayout.HelpBox("Нажимайте 'Следующий шаг', чтобы соединить следующую пару точек и увидеть, как алгоритм строит коридоры.", MessageType.Info);
    }

    /// <summary>
    /// Новый "волновой" алгоритм соединения.
    /// </summary>
    private IEnumerator AutoConnectWaypoints()
    {
        Debug.Log("Начинаем процесс соединения...");

        WaypointLabyrinth[] allWaypoints = GameObject.FindObjectsOfType<WaypointLabyrinth>();

        // --- Шаг 0: Очистка ---
        foreach (var wp in allWaypoints)
        {
            if (wp.neighbors != null) wp.neighbors.Clear();
            EditorUtility.SetDirty(wp);
        }
        Debug.Log("Все старые соединения очищены.");
        SceneView.RepaintAll();
        yield return null;

        // --- Шаг 1: Поиск кандидатов ---
        var candidates = new Dictionary<WaypointLabyrinth, List<WaypointLabyrinth>>();
        const float MaxConnectionDistance = 5f;

        foreach (var wp in allWaypoints)
        {
            var potentialNeighbors = allWaypoints
                .Where(other => other != wp && Vector3.Distance(wp.transform.position, other.transform.position) <= MaxConnectionDistance)
                .OrderBy(other => Vector3.Distance(wp.transform.position, other.transform.position))
                .ToList();
            candidates[wp] = potentialNeighbors;
        }
        Debug.Log("Потенциальные соседи для всех точек найдены.");
        yield return null;

        // --- Шаг 2: Соединение "якорей" (перекрестков и тупиков) ---
        Debug.Log("--- ФАЗА 1: Соединяем перекрестки и тупики ---");
        var priorityWaypoints = allWaypoints
            .Where(p => p.Type == WaypointType.Intersection || p.Type == WaypointType.DeadEnd);

        foreach (var wp in priorityWaypoints)
        {
            int desiredCount = wp.GetDesiredNeighborCount();
            var bestCandidates = candidates[wp]
                .Where(c => c.neighbors.Count < c.GetDesiredNeighborCount())
                .Take(desiredCount - wp.neighbors.Count);

            foreach (var candidate in bestCandidates)
            {
                Debug.Log($"[Якорь] Соединяем {wp.name} <-> {candidate.name}");
                ConnectPair(wp, candidate);
                SceneView.RepaintAll();
                yield return null;
            }
        }

        // --- Шаг 3: "Волновое" построение коридоров ---
        Debug.Log("--- ФАЗА 2: Начинаем построение коридоров от якорей ---");

        // Очередь для точек, от которых нужно "распространяться"
        var processingQueue = new Queue<WaypointLabyrinth>();

        // Находим стартовые точки - те, что примыкают к якорям
        foreach (var wp in allWaypoints.Where(p => p.Type == WaypointType.Standard && p.neighbors.Count == 1))
        {
            processingQueue.Enqueue(wp);
        }
        Debug.Log($"Найдено {processingQueue.Count} стартовых точек для коридоров.");
        yield return null;

        while (processingQueue.Count > 0)
        {
            var currentWp = processingQueue.Dequeue();

            // Если у точки уже 2 соседа, она завершена, пропускаем
            if (currentWp.neighbors.Count >= currentWp.GetDesiredNeighborCount())
            {
                continue;
            }

            // Ищем лучшего соседа для продолжения пути:
            // - Это должна быть Standard точка
            // - У нее должно быть меньше 2-х соседей
            // - Она не должна быть уже соединена с текущей точкой
            var nextNeighbor = candidates[currentWp]
                .FirstOrDefault(c =>
                    c.Type == WaypointType.Standard &&
                    c.neighbors.Count < c.GetDesiredNeighborCount() &&
                    !currentWp.neighbors.Contains(c)
                );

            if (nextNeighbor != null)
            {
                Debug.Log($"[Коридор] Продолжаем путь: {currentWp.name} -> {nextNeighbor.name}");
                ConnectPair(currentWp, nextNeighbor);

                // Добавляем новую точку в очередь, чтобы продолжить путь от нее
                processingQueue.Enqueue(nextNeighbor);

                SceneView.RepaintAll();
                yield return null;
            }
            else
            {
                // Это может произойти, если коридор уперся в уже заполненный перекресток
                // или другой коридор. Это нормальная ситуация.
                Debug.Log($"[Коридор] Точка {currentWp.name} не нашла продолжения. Путь завершен.");
                yield return null;
            }
        }

        Debug.Log("Алгоритм завершил свою работу.");

        // Финальное сохранение изменений
        foreach (var wp in allWaypoints)
        {
            EditorUtility.SetDirty(wp);
        }
    }

    private static void ConnectPair(WaypointLabyrinth a, WaypointLabyrinth b)
    {
        if (a == null || b == null) return;
        if (!a.neighbors.Contains(b)) a.neighbors.Add(b);
        if (!b.neighbors.Contains(a)) b.neighbors.Add(a);
        EditorUtility.SetDirty(a);
        EditorUtility.SetDirty(b);
    }

    private void ClearAllConnections()
    {
        WaypointLabyrinth[] allWaypoints = GameObject.FindObjectsOfType<WaypointLabyrinth>();
        foreach (var wp in allWaypoints)
        {
            if (wp.neighbors != null)
            {
                wp.neighbors.Clear();
                EditorUtility.SetDirty(wp);
            }
        }
        SceneView.RepaintAll();
        Debug.Log("Все соединения были сброшены.");
    }
}