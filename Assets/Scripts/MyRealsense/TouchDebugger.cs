using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Простой отладочный скрипт для проверки работы системы касаний.
/// Выводит подробную информацию о касаниях в консоль.
/// </summary>
public class TouchDebugger : MonoBehaviour
{
    [Header("Настройки")]
    public TouchProcessor touchProcessor;
    public bool showDetailedLogs = true;
    public bool showOnScreenInfo = true;

    private int totalTouchesCount = 0;

    void Start()
    {
        if (touchProcessor == null)
        {
            Debug.LogError("TouchProcessor не назначен в TouchDebugger!", this);
            enabled = false;
            return;
        }

        // Подписываемся на все события касаний
        touchProcessor.OnTouchBegan += OnTouchBegan;
        touchProcessor.OnTouchMoved += OnTouchMoved;
        touchProcessor.OnTouchEnded += OnTouchEnded;

        Debug.Log("🔍 TouchDebugger запущен. Готов к отладке касаний.");
    }

    void OnDestroy()
    {
        if (touchProcessor != null)
        {
            touchProcessor.OnTouchBegan -= OnTouchBegan;
            touchProcessor.OnTouchMoved -= OnTouchMoved;
            touchProcessor.OnTouchEnded -= OnTouchEnded;
        }
    }

    private void OnTouchBegan(TouchInfo touch)
    {
        totalTouchesCount++;

        if (showDetailedLogs)
        {
            Debug.Log($"🟢 <color=green><b>КАСАНИЕ НАЧАЛОСЬ</b></color>\n" +
                     $"   ID: {touch.TouchId}\n" +
                     $"   Позиция в камере: ({touch.PositionInCamera.x:F1}, {touch.PositionInCamera.y:F1})\n" +
                     $"   Позиция в проекции: ({touch.PositionInProjection.x:F3}, {touch.PositionInProjection.y:F3})\n" +
                     $"   Пикселей: {touch.PixelCount}\n" +
                     $"   Время: {touch.StartTime:F2}");
        }
    }

    private void OnTouchMoved(TouchInfo touch)
    {
        if (showDetailedLogs)
        {
            Debug.Log($"🟡 <color=yellow>КАСАНИЕ ДВИЖЕТСЯ</color>\n" +
                     $"   ID: {touch.TouchId}\n" +
                     $"   Новая позиция: ({touch.PositionInProjection.x:F3}, {touch.PositionInProjection.y:F3})\n" +
                     $"   Смещение: {touch.DeltaPosition.magnitude:F2} пикселей\n" +
                     $"   Направление: ({touch.DeltaPosition.x:F1}, {touch.DeltaPosition.y:F1})");
        }
    }

    private void OnTouchEnded(TouchInfo touch)
    {
        if (showDetailedLogs)
        {
            Debug.Log($"🔴 <color=red><b>КАСАНИЕ ЗАКОНЧИЛОСЬ</b></color>\n" +
                     $"   ID: {touch.TouchId}\n" +
                     $"   Длительность: {touch.Duration:F2} секунд\n" +
                     $"   Финальная позиция: ({touch.PositionInProjection.x:F3}, {touch.PositionInProjection.y:F3})");
        }
    }

    void Update()
    {
        // Выводим краткую статистику каждую секунду
        if (Time.frameCount % 60 == 0) // Примерно раз в секунду при 60 FPS
        {
            Debug.Log($"📊 Статистика: Активных касаний: {touchProcessor.ActiveTouches.Count}, Всего касаний: {totalTouchesCount}");
        }
    }

    void OnGUI()
    {
        if (!showOnScreenInfo) return;

        // Отладочная информация на экране
        GUI.color = Color.white;
        GUILayout.BeginArea(new Rect(10, 10, 400, 300));

        GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.fontSize = 16;
        headerStyle.fontStyle = FontStyle.Bold;

        GUILayout.Label("🔍 ОТЛАДКА СИСТЕМЫ КАСАНИЙ", headerStyle);
        GUILayout.Space(10);

        GUILayout.Label($"Всего касаний: {totalTouchesCount}");
        GUILayout.Label($"Активных касаний: {touchProcessor.ActiveTouches.Count}");
        GUILayout.Label($"Точек касания: {touchProcessor.TouchPoints.Count}");

        if (touchProcessor.depthData != null)
        {
            GUILayout.Label($"Разрешение камеры: {touchProcessor.depthWidth}x{touchProcessor.depthHeight}");
        }

        GUILayout.Space(10);

        if (touchProcessor.ActiveTouches.Count > 0)
        {
            GUILayout.Label("Активные касания:", headerStyle);
            foreach (var touch in touchProcessor.ActiveTouches)
            {
                string stateColor = touch.State == TouchState.Began ? "green" :
                                   touch.State == TouchState.Moved ? "yellow" : "white";

                GUILayout.Label($"<color={stateColor}>ID {touch.TouchId}: {touch.State} " +
                               $"({touch.PositionInProjection.x:F2}, {touch.PositionInProjection.y:F2}) " +
                               $"[{touch.PixelCount}px, {touch.Duration:F1}s]</color>");
            }
        }

        GUILayout.EndArea();
    }
}
