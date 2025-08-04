using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Intel.RealSense;

public class InteractiveFloorController : MonoBehaviour
{
    [Header("RealSense Settings")]
    public RsProcessingPipe processingPipe;
    
    [Header("Floor Calibration")]
    public float calibrationTime = 3.0f;
    public bool isCalibrated = false;
    public float floorDistance = 0f;
    
    [Header("Coordinate Mapping")]
    [Tooltip("Размер интерактивной зоны в метрах (ширина)")]
    public float interactiveZoneWidth = 3.0f;
    [Tooltip("Размер интерактивной зоны в метрах (глубина)")]
    public float interactiveZoneDepth = 2.0f;
    [Tooltip("Высота камеры над полом в метрах")]
    public float cameraHeight = 2.5f;
    [Tooltip("Смещение центра интерактивной зоны")]
    public Vector3 zoneCenter = Vector3.zero;

    [Header("Object Detection")]
    [Tooltip("Минимальная высота объекта над полом (в мм) для обнаружения")]
    public float objectHeightThreshold = 50f;
    [Tooltip("Область сканирования (в процентах от размера изображения)")]
    [Range(0.1f, 1.0f)]
    public float scanAreaPercentage = 0.8f;
    [Tooltip("Минимальный размер группы пикселей для считывания объектом")]
    public int minObjectPixelSize = 100;
    
    [Header("Debug Info")]
    public bool showDebugInfo = true;
    public int detectedObjectsCount = 0;
    
    // Данные калибровки
    private List<float> calibrationSamples = new List<float>();
    private float calibrationTimer = 0f;
    private bool isCalibrating = false;
    
    // Данные кадра
    private ushort[] depthData;
    private int frameWidth;
    private int frameHeight;
    
    // Флаги для обработки в главном потоке
    private bool hasNewFrameData = false;
    private readonly object dataLock = new object();
    
    // Обнаруженные объекты
    public struct DetectedObject
    {
        public Vector2 center;      // Центр объекта в координатах изображения
        public Vector2 worldPos;    // Позиция в мировых координатах
        public int pixelCount;      // Количество пикселей объекта
        public float minDepth;      // Минимальная глубина объекта
    }
    
    private List<DetectedObject> detectedObjects = new List<DetectedObject>();
    
    // События для подписки других компонентов
    public System.Action<List<DetectedObject>> OnObjectsDetected;
    public System.Action OnCalibrationComplete;

    void Start()
    {
        if (processingPipe == null)
        {
            Debug.LogError("ProcessingPipe не назначен!");
            return;
        }
        
        Debug.Log("Инициализация InteractiveFloorController...");
        processingPipe.OnStart += OnPipelineStart;
        processingPipe.OnNewSample += OnNewFrame;
        
        StartCalibration();
    }
    
    void OnDestroy()
    {
        if (processingPipe != null)
        {
            processingPipe.OnStart -= OnPipelineStart;
            processingPipe.OnNewSample -= OnNewFrame;
        }
    }
    
    void Update()
    {
        bool processFrame = false;
        
        lock (dataLock)
        {
            if (hasNewFrameData)
            {
                hasNewFrameData = false;
                processFrame = true;
            }
        }
        
        if (processFrame)
        {
            if (isCalibrating)
            {
                ProcessCalibration();
            }
            else if (isCalibrated)
            {
                DetectObjects();
            }
        }
    }
    
    void OnPipelineStart(PipelineProfile profile)
    {
        Debug.Log("RealSense pipeline запущен!");
    }
    
    void OnNewFrame(Frame frame)
    {
        if (frame.IsComposite)
        {
            using (var frameSet = frame.As<FrameSet>())
            {
                using (var depthFrame = frameSet.DepthFrame)
                {
                    if (depthFrame != null)
                    {
                        lock (dataLock)
                        {
                            UpdateDepthData(depthFrame);
                            hasNewFrameData = true;
                        }
                    }
                }
            }
        }
    }
    
    void UpdateDepthData(DepthFrame depthFrame)
    {
        frameWidth = depthFrame.Width;
        frameHeight = depthFrame.Height;
        
        if (depthData == null || depthData.Length != frameWidth * frameHeight)
        {
            depthData = new ushort[frameWidth * frameHeight];
        }
        
        depthFrame.CopyTo(depthData);
    }
    
    void StartCalibration()
    {
        Debug.Log("Начинаем калибровку пола...");
        calibrationSamples.Clear();
        calibrationTimer = 0f;
        isCalibrating = true;
        isCalibrated = false;
    }
    
    void ProcessCalibration()
    {
        calibrationTimer += Time.deltaTime;
        
        if (depthData != null && frameWidth > 0 && frameHeight > 0)
        {
            // Собираем образцы из центральной области
            int centerX = frameWidth / 2;
            int centerY = frameHeight / 2;
            int sampleSize = 50; // 100x100 пикселей вокруг центра
            
            List<float> frameSamples = new List<float>();
            
            for (int y = centerY - sampleSize; y <= centerY + sampleSize; y++)
            {
                for (int x = centerX - sampleSize; x <= centerX + sampleSize; x++)
                {
                    if (x >= 0 && x < frameWidth && y >= 0 && y < frameHeight)
                    {
                        int index = y * frameWidth + x;
                        ushort depth = depthData[index];
                        
                        if (depth > 0) // Игнорируем нулевые значения
                        {
                            frameSamples.Add(depth);
                        }
                    }
                }
            }
            
            if (frameSamples.Count > 0)
            {
                // Добавляем медианное значение кадра в общие образцы
                frameSamples.Sort();
                float medianValue = frameSamples[frameSamples.Count / 2];
                calibrationSamples.Add(medianValue);
            }
        }
        
        if (calibrationTimer >= calibrationTime)
        {
            CompleteCalibration();
        }
    }
    
    void CompleteCalibration()
    {
        if (calibrationSamples.Count > 0)
        {
            // Вычисляем среднее значение
            float sum = 0f;
            foreach (float sample in calibrationSamples)
            {
                sum += sample;
            }
            floorDistance = sum / calibrationSamples.Count;
            
            isCalibrating = false;
            isCalibrated = true;
            
            Debug.Log($"Калибровка завершена! Расстояние до пола: {floorDistance:F1} мм");
            Debug.Log($"Использовано образцов: {calibrationSamples.Count}");
            
            OnCalibrationComplete?.Invoke();
        }
        else
        {
            Debug.LogWarning("Не удалось собрать образцы для калибровки. Повторяем...");
            StartCalibration();
        }
    }
    
    void DetectObjects()
    {
        if (depthData == null || frameWidth == 0 || frameHeight == 0)
            return;
            
        detectedObjects.Clear();
        
        // Определяем область сканирования
        int scanWidth = Mathf.RoundToInt(frameWidth * scanAreaPercentage);
        int scanHeight = Mathf.RoundToInt(frameHeight * scanAreaPercentage);
        int startX = (frameWidth - scanWidth) / 2;
        int startY = (frameHeight - scanHeight) / 2;
        
        // Создаем карту обнаруженных пикселей
        bool[,] objectPixels = new bool[frameWidth, frameHeight];
        
        // Первый проход: находим все пиксели объектов
        for (int y = startY; y < startY + scanHeight; y++)
        {
            for (int x = startX; x < startX + scanWidth; x++)
            {
                int index = y * frameWidth + x;
                ushort depth = depthData[index];
                
                if (depth > 0)
                {
                    float heightAboveFloor = floorDistance - depth;
                    
                    if (heightAboveFloor > objectHeightThreshold)
                    {
                        objectPixels[x, y] = true;
                    }
                }
            }
        }
        
        // Второй проход: группируем пиксели в объекты
        bool[,] visited = new bool[frameWidth, frameHeight];
        
        for (int y = startY; y < startY + scanHeight; y++)
        {
            for (int x = startX; x < startX + scanWidth; x++)
            {
                if (objectPixels[x, y] && !visited[x, y])
                {
                    var objectPixelsList = new List<Vector2Int>();
                    FloodFill(objectPixels, visited, x, y, objectPixelsList);
                    
                    if (objectPixelsList.Count >= minObjectPixelSize)
                    {
                        CreateDetectedObject(objectPixelsList);
                    }
                }
            }
        }
        
        detectedObjectsCount = detectedObjects.Count;
        
        if (detectedObjects.Count > 0)
        {
            OnObjectsDetected?.Invoke(detectedObjects);
            
            if (showDebugInfo)
            {
                Debug.Log($"Обнаружено объектов: {detectedObjects.Count}");
            }
        }
    }
    
    void FloodFill(bool[,] objectPixels, bool[,] visited, int startX, int startY, List<Vector2Int> result)
    {
        Stack<Vector2Int> stack = new Stack<Vector2Int>();
        stack.Push(new Vector2Int(startX, startY));
        
        while (stack.Count > 0)
        {
            Vector2Int current = stack.Pop();
            int x = current.x;
            int y = current.y;
            
            if (x < 0 || x >= frameWidth || y < 0 || y >= frameHeight || 
                visited[x, y] || !objectPixels[x, y])
                continue;
                
            visited[x, y] = true;
            result.Add(current);
            
            // Проверяем соседние пиксели (4-связность)
            stack.Push(new Vector2Int(x + 1, y));
            stack.Push(new Vector2Int(x - 1, y));
            stack.Push(new Vector2Int(x, y + 1));
            stack.Push(new Vector2Int(x, y - 1));
        }
    }
    
    void CreateDetectedObject(List<Vector2Int> pixels)
    {
        if (pixels.Count == 0) return;
        
        // Вычисляем центр объекта
        float centerX = 0, centerY = 0;
        ushort minDepth = ushort.MaxValue;
        
        foreach (var pixel in pixels)
        {
            centerX += pixel.x;
            centerY += pixel.y;
            
            int index = pixel.y * frameWidth + pixel.x;
            ushort depth = depthData[index];
            if (depth > 0 && depth < minDepth)
            {
                minDepth = depth;
            }
        }
        
        centerX /= pixels.Count;
        centerY /= pixels.Count;
        
        // Преобразуем пиксельные координаты в мировые
        Vector3 worldPos = CameraToWorldCoordinates(new Vector2(centerX, centerY));
        
        DetectedObject obj = new DetectedObject
        {
            center = new Vector2(centerX, centerY),
            worldPos = new Vector2(worldPos.x, worldPos.z),
            pixelCount = pixels.Count,
            minDepth = minDepth
        };
        
        detectedObjects.Add(obj);
    }
    
    // Публичные методы для внешнего управления
    public void RecalibrateFloor()
    {
        StartCalibration();
    }
    
    public List<DetectedObject> GetDetectedObjects()
    {
        return new List<DetectedObject>(detectedObjects);
    }
    
    public bool IsSystemReady()
    {
        return isCalibrated && !isCalibrating;
    }
    
    /// <summary>
    /// Преобразует координаты пикселей камеры в мировые координаты Unity
    /// </summary>
    public Vector3 CameraToWorldCoordinates(Vector2 pixelPos)
    {
        if (frameWidth == 0 || frameHeight == 0)
            return Vector3.zero;
            
        // Нормализуем координаты (0-1)
        float normalizedX = pixelPos.x / frameWidth;
        float normalizedY = pixelPos.y / frameHeight;
        
        // Преобразуем в мировые координаты
        // Камера смотрит вниз, поэтому Y камеры становится Z мира
        float worldX = (normalizedX - 0.5f) * interactiveZoneWidth + zoneCenter.x;
        float worldZ = (normalizedY - 0.5f) * interactiveZoneDepth + zoneCenter.z;
        float worldY = zoneCenter.y; // Объекты на уровне пола
        
        return new Vector3(worldX, worldY, worldZ);
    }
    
    /// <summary>
    /// Получает высоту объекта над полом в метрах
    /// </summary>
    public float GetObjectHeightAboveFloor(ushort depth)
    {
        if (!isCalibrated || depth == 0)
            return 0f;
            
        // Преобразуем миллиметры в метры
        float heightMM = floorDistance - depth;
        return heightMM / 1000f; // конвертируем в метры
    }
    
    void OnGUI()
    {
        if (!showDebugInfo) return;
        
        GUILayout.BeginArea(new Rect(10, 10, 300, 200));
        GUILayout.Label($"Статус: {(isCalibrating ? "Калибровка..." : (isCalibrated ? "Готов" : "Не готов"))}");
        
        if (isCalibrated)
        {
            GUILayout.Label($"Расстояние до пола: {floorDistance:F1} мм");
            GUILayout.Label($"Порог высоты: {objectHeightThreshold:F1} мм");
            GUILayout.Label($"Обнаружено объектов: {detectedObjectsCount}");
            
            if (GUILayout.Button("Повторить калибровку"))
            {
                RecalibrateFloor();
            }
        }
        
        if (isCalibrating)
        {
            float progress = calibrationTimer / calibrationTime;
            GUILayout.Label($"Прогресс калибровки: {progress * 100:F0}%");
        }
        
        GUILayout.EndArea();
    }
} 