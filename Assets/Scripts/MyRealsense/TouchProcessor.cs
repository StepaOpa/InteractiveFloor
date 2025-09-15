using UnityEngine;
using Intel.RealSense;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using UnityEngine.UI;

// Режимы визуализации данных глубины
public enum VisualizationMode
{
    HeatMap,        // Стандартная тепловая карта (красный-синий градиент)
    ContourBands,   // Полосы равной глубины (как топографическая карта)
    LocalContrast,  // Выделение контуров и перепадов глубины
    Combined        // Комбинация полос и контрастов
}

// Состояние касания
public enum TouchState
{
    Began,      // Касание только началось
    Moved,      // Касание движется
    Stationary, // Касание неподвижно
    Ended       // Касание закончилось
}

// Вспомогательный класс для группировки точек касания
public class TouchCluster
{
    public List<Vector2> Points = new List<Vector2>();
}

// Класс для представления одного касания
[Serializable]
public class TouchInfo
{
    public int TouchId;                    // Уникальный ID касания
    public Vector2 PositionInCamera;       // Позиция в координатах камеры (пиксели)
    public Vector2 PositionInProjection;   // Позиция в координатах проекции (0-1)
    public Vector2 PositionInScreen;       // Позиция в экранных координатах Unity
    public TouchState State;               // Текущее состояние касания
    public float StartTime;                // Время начала касания
    public float LastUpdateTime;           // Время последнего обновления
    public Vector2 DeltaPosition;          // Смещение с последнего кадра
    public int PixelCount;                 // Количество пикселей в этом касании (размер области)

    public float Duration => LastUpdateTime - StartTime;
    public bool IsActive => State != TouchState.Ended;
}

// Класс для хранения данных калибровки
[Serializable]
public class CalibrationData
{
    public Vector2[] CornerPoints = new Vector2[4]; // 0:TL, 1:TR, 2:BL, 3:BR
    public float AverageFloorDepth = 0;
}

public class TouchProcessor : MonoBehaviour
{
    [Header("RealSense")]
    public RsFrameProvider Source;

    [Header("Калибровка")]
    public RectTransform[] CalibrationMarkers; // 0:TL, 1:TR, 2:BL, 3:BR
    public GameObject CalibrationUI;
    public float TouchThresholdMm = 50; // Насколько объект должен "подняться" над полом, чтобы считаться касанием (в мм)

    [Header("Визуализация (необязательно)")]
    public RawImage DebugImage; // UI RawImage для отображения карты глубины и касаний
    public float MaxColorDepthMm = 4000f; // Максимальная глубина для цветового градиента (в мм)

    [Header("Режимы визуализации")]
    public VisualizationMode visualizationMode = VisualizationMode.ContourBands;
    public float BandWidthMm = 10f; // Ширина полосы в мм для режима ContourBands
    public float ContrastSensitivity = 5f; // Чувствительность к перепадам глубины в мм для режима LocalContrast

    [Header("Настройки системы касаний")]
    public float TouchClusterRadius = 30f; // Радиус группировки точек касания в пикселях камеры
    public int MinTouchPixels = 5; // Минимальное количество пикселей для регистрации касания
    public float TouchTimeoutSeconds = 0.5f; // Время в секундах, после которого касание считается завершенным
    public float MovementThreshold = 5f; // Минимальное смещение в пикселях для регистрации движения

    // События для уведомления других скриптов
    public System.Action<TouchInfo> OnTouchBegan;
    public System.Action<TouchInfo> OnTouchMoved;
    public System.Action<TouchInfo> OnTouchEnded;

    // Публичные свойства для доступа из других скриптов
    public ushort[] depthData { get; private set; }
    public int depthWidth { get; private set; }
    public int depthHeight { get; private set; }
    public List<Vector2> TouchPoints { get; private set; } = new List<Vector2>();
    public List<TouchInfo> ActiveTouches { get; private set; } = new List<TouchInfo>();

    private bool _isCalibrating = false;
    private CalibrationData _calibrationData = new CalibrationData();
    private const string CALIBRATION_KEY = "InteractiveFloorCalibration";

    // Поля для отладочной текстуры
    private Texture2D _debugTexture;
    private byte[] _debugTextureBytes;
    private volatile bool _textureNeedsUpdate = false; // volatile важен для многопоточности
    private readonly object _textureLock = new object();

    // Приватные поля для системы касаний
    private int _nextTouchId = 1;
    private List<TouchInfo> _previousTouches = new List<TouchInfo>();
    private readonly object _touchLock = new object();
    private float _currentFrameTime; // Время текущего кадра, передаваемое из основного потока

    void Start()
    {
        if (Source == null)
        {
            Debug.LogError("RsFrameProvider source is not set.", this);
            enabled = false;
            return;
        }

        LoadCalibration();
        CalibrationUI.SetActive(false);
    }

    void OnEnable()
    {
        if (Source != null) Source.OnNewSample += OnNewSample;
    }

    void OnDisable()
    {
        if (Source != null) Source.OnNewSample -= OnNewSample;
    }

    // Выполняется в фоновом потоке
    private void OnNewSample(Frame frame)
    {
        try
        {
            if (frame.IsComposite)
            {
                using (var frames = frame.As<FrameSet>())
                using (var depthFrame = frames.DepthFrame)
                {
                    if (depthFrame != null)
                    {
                        ProcessDepthFrame(depthFrame);
                    }
                }
            }
            else if (frame.Is(Extension.DepthFrame))
            {
                using (var depthFrame = frame.As<DepthFrame>())
                {
                    ProcessDepthFrame(depthFrame);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    // Выполняется в фоновом потоке
    private void ProcessDepthFrame(DepthFrame depthFrame)
    {
        // 1. Получаем данные глубины
        bool sizeChanged = false;
        if (depthData == null || depthWidth != depthFrame.Width || depthHeight != depthFrame.Height)
        {
            depthWidth = depthFrame.Width;
            depthHeight = depthFrame.Height;
            depthData = new ushort[depthWidth * depthHeight];
            sizeChanged = true;
        }
        depthFrame.CopyTo(depthData);

        // 2. Ищем точки касания
        FindTouchPoints();

        // 3. Обрабатываем касания (группировка, отслеживание, события)
        ProcessTouches();

        // 4. Если включена отладка, готовим массив байтов для текстуры
        if (DebugImage != null)
        {
            lock (_textureLock)
            {
                if (sizeChanged || _debugTextureBytes == null)
                {
                    _debugTextureBytes = new byte[depthWidth * depthHeight * 3];
                }

                // Обрабатываем каждый пиксель в зависимости от выбранного режима
                for (int i = 0; i < depthData.Length; i++)
                {
                    ushort depth = depthData[i];
                    int x = i % depthWidth;
                    int y = i / depthWidth;

                    byte r, g, b;
                    GetVisualizationColor(depth, x, y, out r, out g, out b);

                    _debugTextureBytes[i * 3] = r;
                    _debugTextureBytes[i * 3 + 1] = g;
                    _debugTextureBytes[i * 3 + 2] = b;
                }

                // Отмечаем точки касания белым цветом, чтобы они выделялись
                foreach (var touch in TouchPoints)
                {
                    int x = (int)touch.x;
                    int y = depthHeight - 1 - (int)touch.y;
                    if (x >= 0 && x < depthWidth && y >= 0 && y < depthHeight)
                    {
                        int index = (y * depthWidth + x) * 3;
                        _debugTextureBytes[index] = 255;
                        _debugTextureBytes[index + 1] = 255;
                        _debugTextureBytes[index + 2] = 255;
                    }
                }
            }
            _textureNeedsUpdate = true; // Устанавливаем флаг для основного потока
        }
    }

    // Выполняется в фоновом потоке
    private void FindTouchPoints()
    {
        TouchPoints.Clear();
        if (_calibrationData.AverageFloorDepth <= 0 || depthData == null) return;

        float minX = Mathf.Min(_calibrationData.CornerPoints[0].x, _calibrationData.CornerPoints[2].x);
        float maxX = Mathf.Max(_calibrationData.CornerPoints[1].x, _calibrationData.CornerPoints[3].x);
        float minY = Mathf.Min(_calibrationData.CornerPoints[0].y, _calibrationData.CornerPoints[1].y);
        float maxY = Mathf.Max(_calibrationData.CornerPoints[2].y, _calibrationData.CornerPoints[3].y);

        for (int y = (int)minY; y < (int)maxY; y++)
        {
            for (int x = (int)minX; x < (int)maxX; x++)
            {
                int index = y * depthWidth + x;
                ushort depth = depthData[index];

                if (depth > 0 && depth < (_calibrationData.AverageFloorDepth - TouchThresholdMm))
                {
                    TouchPoints.Add(new Vector2(x, y));
                }
            }
        }
    }

    // Выполняется в основном потоке Unity
    void Update()
    {
        // Обновляем время для фонового потока (потокобезопасно)
        _currentFrameTime = Time.time;

        if (_isCalibrating)
        {
            // Логика перетаскивания маркеров (остается без изменений)
        }

        // Обновление отладочной текстуры, если фоновый поток подготовил данные
        if (_textureNeedsUpdate)
        {
            _textureNeedsUpdate = false; // Сбрасываем флаг

            // Проверяем и создаем/пересоздаем текстуру, если нужно. ЭТО БЕЗОПАСНО, т.к. мы в Update().
            if (_debugTexture == null || _debugTexture.width != depthWidth || _debugTexture.height != depthHeight)
            {
                _debugTexture = new Texture2D(depthWidth, depthHeight, TextureFormat.RGB24, false);
                DebugImage.texture = _debugTexture;
            }

            // Загружаем данные в текстуру и применяем
            lock (_textureLock)
            {
                _debugTexture.LoadRawTextureData(_debugTextureBytes);
            }
            _debugTexture.Apply();
        }
    }

    // --- Методы для кнопок UI и калибровки (остаются без изменений) ---

    public void StartCalibration()
    {
        _isCalibrating = true;
        CalibrationUI.SetActive(true);
        Debug.Log("Режим калибровки начат. Перетащите маркеры по углам проекции.");
    }

    public void CancelCalibration()
    {
        _isCalibrating = false;
        CalibrationUI.SetActive(false);
        UpdateMarkersFromData();
        Debug.Log("Калибровка отменена.");
    }

    public void SaveCalibration()
    {
        _isCalibrating = false;
        CalibrationUI.SetActive(false);

        for (int i = 0; i < 4; i++)
        {
            _calibrationData.CornerPoints[i] = ScreenToDepthCoordinates(CalibrationMarkers[i].position);
        }

        long totalDepth = 0;
        int pointCount = 0;
        foreach (var corner in _calibrationData.CornerPoints)
        {
            for (int y = (int)corner.y - 2; y <= (int)corner.y + 2; y++)
            {
                for (int x = (int)corner.x - 2; x <= (int)corner.x + 2; x++)
                {
                    if (x >= 0 && x < depthWidth && y >= 0 && y < depthHeight)
                    {
                        ushort depth = depthData[y * depthWidth + x];
                        if (depth > 0)
                        {
                            totalDepth += depth;
                            pointCount++;
                        }
                    }
                }
            }
        }

        if (pointCount > 0)
        {
            _calibrationData.AverageFloorDepth = (float)totalDepth / pointCount;
            Debug.Log($"Средняя глубина пола: {_calibrationData.AverageFloorDepth} мм.");
        }
        else
        {
            Debug.LogError("Не удалось определить глубину пола. Убедитесь, что маркеры находятся в пределах видимости камеры.");
            return;
        }

        string json = JsonUtility.ToJson(_calibrationData);
        PlayerPrefs.SetString(CALIBRATION_KEY, json);
        PlayerPrefs.Save();
        Debug.Log("Калибровка сохранена!");
    }

    private void LoadCalibration()
    {
        if (PlayerPrefs.HasKey(CALIBRATION_KEY))
        {
            string json = PlayerPrefs.GetString(CALIBRATION_KEY);
            _calibrationData = JsonUtility.FromJson<CalibrationData>(json);
            UpdateMarkersFromData();
            Debug.Log("Калибровка загружена.");
        }
        else
        {
            Debug.Log("Данные калибровки не найдены.");
        }
    }

    private void UpdateMarkersFromData()
    {
        for (int i = 0; i < 4; i++)
        {
            CalibrationMarkers[i].position = DepthToScreenCoordinates(_calibrationData.CornerPoints[i]);
        }
    }

    private Vector2 ScreenToDepthCoordinates(Vector2 screenPos)
    {
        float x = (screenPos.x / Screen.width) * depthWidth;
        float y = (screenPos.y / Screen.height) * depthHeight;
        return new Vector2(x, y);
    }

    private Vector2 DepthToScreenCoordinates(Vector2 depthPos)
    {
        float x = (depthPos.x / depthWidth) * Screen.width;
        float y = (depthPos.y / depthHeight) * Screen.height;
        return new Vector2(x, y);
    }

    /// <summary>
    /// Основной метод обработки касаний - группирует точки, отслеживает их во времени и генерирует события.
    /// Выполняется в фоновом потоке.
    /// </summary>
    private void ProcessTouches()
    {
        // 1. Группируем близкие точки касания в кластеры
        List<TouchCluster> clusters = GroupTouchPointsIntoClusters();

        // 2. Преобразуем кластеры в касания
        List<TouchInfo> currentTouches = new List<TouchInfo>();
        foreach (var cluster in clusters)
        {
            if (cluster.Points.Count >= MinTouchPixels)
            {
                TouchInfo touch = CreateTouchFromCluster(cluster);
                currentTouches.Add(touch);
            }
        }

        // 3. Сопоставляем текущие касания с предыдущими и обновляем состояния
        UpdateTouchStates(currentTouches);

        // 4. Обновляем список активных касаний (потокобезопасно)
        lock (_touchLock)
        {
            ActiveTouches.Clear();
            ActiveTouches.AddRange(currentTouches.Where(t => t.IsActive));
        }
    }

    /// <summary>
    /// Группирует близкие точки касания в кластеры.
    /// </summary>
    private List<TouchCluster> GroupTouchPointsIntoClusters()
    {
        List<TouchCluster> clusters = new List<TouchCluster>();
        List<Vector2> unprocessedPoints = new List<Vector2>(TouchPoints);

        while (unprocessedPoints.Count > 0)
        {
            // Начинаем новый кластер с первой необработанной точки
            TouchCluster cluster = new TouchCluster();
            cluster.Points.Add(unprocessedPoints[0]);
            unprocessedPoints.RemoveAt(0);

            // Ищем все точки в радиусе от центра кластера
            bool foundNewPoints = true;
            while (foundNewPoints)
            {
                foundNewPoints = false;
                Vector2 clusterCenter = GetClusterCenter(cluster);

                for (int i = unprocessedPoints.Count - 1; i >= 0; i--)
                {
                    float distance = Vector2.Distance(clusterCenter, unprocessedPoints[i]);
                    if (distance <= TouchClusterRadius)
                    {
                        cluster.Points.Add(unprocessedPoints[i]);
                        unprocessedPoints.RemoveAt(i);
                        foundNewPoints = true;
                    }
                }
            }

            clusters.Add(cluster);
        }

        return clusters;
    }

    /// <summary>
    /// Создает TouchInfo из кластера точек.
    /// </summary>
    private TouchInfo CreateTouchFromCluster(TouchCluster cluster)
    {
        Vector2 centerInCamera = GetClusterCenter(cluster);

        TouchInfo touch = new TouchInfo
        {
            PositionInCamera = centerInCamera,
            PositionInProjection = CameraToProjectionCoordinates(centerInCamera),
            PositionInScreen = CameraToScreenCoordinates(centerInCamera),
            PixelCount = cluster.Points.Count,
            LastUpdateTime = _currentFrameTime
        };

        return touch;
    }

    /// <summary>
    /// Обновляет состояния касаний, сопоставляя их с предыдущим кадром.
    /// </summary>
    private void UpdateTouchStates(List<TouchInfo> currentTouches)
    {
        float currentTime = _currentFrameTime;

        // Сначала пытаемся сопоставить текущие касания с предыдущими
        foreach (var currentTouch in currentTouches)
        {
            TouchInfo matchedPreviousTouch = null;
            float minDistance = float.MaxValue;

            // Ищем ближайшее предыдущее касание
            foreach (var prevTouch in _previousTouches)
            {
                if (!prevTouch.IsActive) continue;

                float distance = Vector2.Distance(currentTouch.PositionInCamera, prevTouch.PositionInCamera);
                if (distance < minDistance && distance <= TouchClusterRadius)
                {
                    minDistance = distance;
                    matchedPreviousTouch = prevTouch;
                }
            }

            if (matchedPreviousTouch != null)
            {
                // Обновляем существующее касание
                currentTouch.TouchId = matchedPreviousTouch.TouchId;
                currentTouch.StartTime = matchedPreviousTouch.StartTime;
                currentTouch.DeltaPosition = currentTouch.PositionInCamera - matchedPreviousTouch.PositionInCamera;

                // Определяем состояние
                if (currentTouch.DeltaPosition.magnitude > MovementThreshold)
                {
                    currentTouch.State = TouchState.Moved;
                }
                else
                {
                    currentTouch.State = TouchState.Stationary;
                }

                // Удаляем из предыдущих, чтобы не использовать дважды
                _previousTouches.Remove(matchedPreviousTouch);
            }
            else
            {
                // Новое касание
                currentTouch.TouchId = _nextTouchId++;
                currentTouch.StartTime = currentTime;
                currentTouch.State = TouchState.Began;
                currentTouch.DeltaPosition = Vector2.zero;
            }
        }

        // Отмечаем оставшиеся предыдущие касания как завершенные
        foreach (var prevTouch in _previousTouches)
        {
            if (prevTouch.IsActive && (currentTime - prevTouch.LastUpdateTime) > TouchTimeoutSeconds)
            {
                prevTouch.State = TouchState.Ended;
                currentTouches.Add(prevTouch);
            }
        }

        // Обновляем список предыдущих касаний
        _previousTouches.Clear();
        _previousTouches.AddRange(currentTouches.Where(t => t.IsActive));

        // Генерируем события (в основном потоке это будет обработано)
        foreach (var touch in currentTouches)
        {
            switch (touch.State)
            {
                case TouchState.Began:
                    OnTouchBegan?.Invoke(touch);
                    break;
                case TouchState.Moved:
                    OnTouchMoved?.Invoke(touch);
                    break;
                case TouchState.Ended:
                    OnTouchEnded?.Invoke(touch);
                    break;
            }
        }
    }

    /// <summary>
    /// Вычисляет центр кластера точек.
    /// </summary>
    private Vector2 GetClusterCenter(TouchCluster cluster)
    {
        Vector2 sum = Vector2.zero;
        foreach (var point in cluster.Points)
        {
            sum += point;
        }
        return sum / cluster.Points.Count;
    }

    /// <summary>
    /// Преобразует координаты из пикселей камеры в координаты проекции (0-1).
    /// </summary>
    private Vector2 CameraToProjectionCoordinates(Vector2 cameraPos)
    {
        // Используем билинейную интерполяцию для преобразования координат из четырехугольника калибровки
        // в прямоугольник проекции (0,0) - (1,1)

        if (_calibrationData.CornerPoints == null || _calibrationData.CornerPoints.Length < 4)
            return Vector2.zero;

        // TL = Top-Left, TR = Top-Right, BL = Bottom-Left, BR = Bottom-Right
        Vector2 tl = _calibrationData.CornerPoints[0];
        Vector2 tr = _calibrationData.CornerPoints[1];
        Vector2 bl = _calibrationData.CornerPoints[2];
        Vector2 br = _calibrationData.CornerPoints[3];

        // Простое приближение: используем bounding box для быстрого преобразования
        float minX = Mathf.Min(tl.x, bl.x);
        float maxX = Mathf.Max(tr.x, br.x);
        float minY = Mathf.Min(tl.y, tr.y);
        float maxY = Mathf.Max(bl.y, br.y);

        float normalizedX = Mathf.InverseLerp(minX, maxX, cameraPos.x);
        float normalizedY = Mathf.InverseLerp(minY, maxY, cameraPos.y);

        return new Vector2(normalizedX, normalizedY);
    }

    /// <summary>
    /// Преобразует координаты из пикселей камеры в экранные координаты Unity.
    /// </summary>
    private Vector2 CameraToScreenCoordinates(Vector2 cameraPos)
    {
        Vector2 projectionPos = CameraToProjectionCoordinates(cameraPos);
        return new Vector2(projectionPos.x * Screen.width, projectionPos.y * Screen.height);
    }

    /// <summary>
    /// Определяет цвет пикселя в зависимости от выбранного режима визуализации.
    /// </summary>
    private void GetVisualizationColor(ushort depth, int x, int y, out byte r, out byte g, out byte b)
    {
        if (depth == 0)
        {
            // Пиксели без данных делаем черными
            r = g = b = 0;
            return;
        }

        switch (visualizationMode)
        {
            case VisualizationMode.HeatMap:
                GetHeatMapColor(depth, out r, out g, out b);
                break;
            case VisualizationMode.ContourBands:
                GetContourBandsColor(depth, out r, out g, out b);
                break;
            case VisualizationMode.LocalContrast:
                GetLocalContrastColor(depth, x, y, out r, out g, out b);
                break;
            case VisualizationMode.Combined:
                GetCombinedColor(depth, x, y, out r, out g, out b);
                break;
            default:
                GetHeatMapColor(depth, out r, out g, out b);
                break;
        }
    }

    /// <summary>
    /// Стандартная тепловая карта (красный-синий градиент).
    /// </summary>
    private void GetHeatMapColor(ushort depth, out byte r, out byte g, out byte b)
    {
        float normalizedDepth = Mathf.Clamp01((float)depth / MaxColorDepthMm);
        float hue = normalizedDepth * 0.66f; // От красного (0) до синего (0.66)
        HsvToRgb(hue, 1, 1, out r, out g, out b);
    }

    /// <summary>
    /// Режим контурных полос - создает "топографическую карту" с резкими переходами цвета.
    /// </summary>
    private void GetContourBandsColor(ushort depth, out byte r, out byte g, out byte b)
    {
        // Вычисляем, в какую "полосу" попадает текущая глубина
        int bandIndex = Mathf.FloorToInt((float)depth / BandWidthMm);

        // Создаем циклический градиент - каждая полоса имеет свой цвет
        float hue = (bandIndex % 12) / 12f; // 12 различных оттенков

        // Делаем полосы более контрастными
        float saturation = 0.8f;
        float value = 0.9f;

        HsvToRgb(hue, saturation, value, out r, out g, out b);
    }

    /// <summary>
    /// Режим локального контраста - выделяет места с резкими перепадами глубины.
    /// </summary>
    private void GetLocalContrastColor(ushort depth, int x, int y, out byte r, out byte g, out byte b)
    {
        // Анализируем разность глубины с соседними пикселями
        float maxDifference = 0f;

        // Проверяем 4 направления: вверх, вниз, влево, вправо
        int[] dx = { -1, 1, 0, 0 };
        int[] dy = { 0, 0, -1, 1 };

        for (int i = 0; i < 4; i++)
        {
            int nx = x + dx[i];
            int ny = y + dy[i];

            if (nx >= 0 && nx < depthWidth && ny >= 0 && ny < depthHeight)
            {
                int neighborIndex = ny * depthWidth + nx;
                ushort neighborDepth = depthData[neighborIndex];

                if (neighborDepth > 0)
                {
                    float difference = Mathf.Abs((float)depth - neighborDepth);
                    maxDifference = Mathf.Max(maxDifference, difference);
                }
            }
        }

        // Если разность большая - выделяем ярким цветом, иначе - серым
        if (maxDifference > ContrastSensitivity)
        {
            // Ярко-желтый для контуров
            r = 255; g = 255; b = 0;
        }
        else
        {
            // Серый градиент для ровных областей
            float normalizedDepth = Mathf.Clamp01((float)depth / MaxColorDepthMm);
            byte grayValue = (byte)(normalizedDepth * 200 + 50); // От 50 до 250
            r = g = b = grayValue;
        }
    }

    /// <summary>
    /// Комбинированный режим - сочетает контурные полосы с выделением контрастов.
    /// </summary>
    private void GetCombinedColor(ushort depth, int x, int y, out byte r, out byte g, out byte b)
    {
        // Сначала получаем цвет контурных полос
        GetContourBandsColor(depth, out byte bandR, out byte bandG, out byte bandB);

        // Затем проверяем локальный контраст
        GetLocalContrastColor(depth, x, y, out byte contrastR, out byte contrastG, out byte contrastB);

        // Если есть контраст (контурная линия), используем контрастный цвет
        if (contrastR == 255 && contrastG == 255 && contrastB == 0) // Желтый = контур
        {
            r = contrastR; g = contrastG; b = contrastB;
        }
        else
        {
            // Иначе используем цвет полос, но с уменьшенной яркостью
            r = (byte)(bandR * 0.7f);
            g = (byte)(bandG * 0.7f);
            b = (byte)(bandB * 0.7f);
        }
    }

    /// <summary>
    /// Конвертирует цвет из HSV в RGB.
    /// </summary>
    /// <param name="h">Hue (от 0 до 1)</param>
    /// <param name="s">Saturation (от 0 до 1)</param>
    /// <param name="v">Value (от 0 до 1)</param>
    /// <param name="r">Выходной Red (от 0 до 255)</param>
    /// <param name="g">Выходной Green (от 0 до 255)</param>
    /// <param name="b">Выходной Blue (от 0 до 255)</param>
    private void HsvToRgb(float h, float s, float v, out byte r, out byte g, out byte b)
    {
        int i = Mathf.FloorToInt(h * 6);
        float f = h * 6 - i;
        float p = v * (1 - s);
        float q = v * (1 - f * s);
        float t = v * (1 - (1 - f) * s);

        float rf = 0, gf = 0, bf = 0;
        switch (i % 6)
        {
            case 0: rf = v; gf = t; bf = p; break;
            case 1: rf = q; gf = v; bf = p; break;
            case 2: rf = p; gf = v; bf = t; break;
            case 3: rf = p; gf = q; bf = v; break;
            case 4: rf = t; gf = p; bf = v; break;
            case 5: rf = v; gf = p; bf = q; break;
        }

        r = (byte)(rf * 255);
        g = (byte)(gf * 255);
        b = (byte)(bf * 255);
    }
}
