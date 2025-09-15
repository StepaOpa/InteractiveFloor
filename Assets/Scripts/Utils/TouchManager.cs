using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine.UI;


public class TouchManager : MonoBehaviour
{
    public static TouchManager Instance { get; private set; }

    Thread receiveThread;
    UdpClient client;
    public int port = 8052;
    private string latestData = "";
    private bool newDataAvailable = false;
    private bool isReceiving = false;

    [SerializeField] TouchSimulator touchSimulator;

    [SerializeField] private GameObject debugCanvas;

    void Awake()
    {
        // Реализация синглтона
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            StartReceiving();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Если уже запущен, не запускаем повторно
        if (!isReceiving)
        {
            StartReceiving();
        }
    }

    private void StartReceiving()
    {
        if (isReceiving) return;

        try
        {
            receiveThread = new Thread(new ThreadStart(ReceiveData));
            receiveThread.IsBackground = true;
            receiveThread.Start();
            isReceiving = true;
            Debug.Log($"TouchManager: Начато прослушивание порта {port}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"TouchManager: Ошибка запуска потока: {e.Message}");
        }
    }

    private void ReceiveData()
    {
        try
        {
            client = new UdpClient(port);
            Debug.Log($"TouchManager: UDP клиент создан на порту {port}");

            while (isReceiving)
            {
                try
                {
                    IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, 0);
                    byte[] data = client.Receive(ref anyIP);
                    latestData = Encoding.UTF8.GetString(data);
                    newDataAvailable = true;
                }
                catch (Exception err)
                {
                    if (isReceiving) // Логируем только если еще должны работать
                    {
                        Debug.LogError($"TouchManager: Ошибка получения данных: {err.Message}");
                    }
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"TouchManager: Ошибка создания UDP клиента: {e.Message}");
        }
        finally
        {
            StopReceiving();
        }
    }

    void Update()
    {
        if (newDataAvailable)
        {
            ProcessData(latestData);
            newDataAvailable = false;
        }
    }

    void ProcessData(string data)
    {
        try
        {
            // Используем встроенный JsonUtility вместо Newtonsoft
            TouchDataWrapper wrapper = JsonUtility.FromJson<TouchDataWrapper>(data);

            foreach (TouchPoint touch in wrapper.touches)
            {
                // Правильное преобразование нормализованных координат в экранные
                Vector2 screenPosition = new Vector2(touch.x * Screen.width, (1 - touch.y) * Screen.height);
                Debug.Log($"Касание: {screenPosition}");

                // Симулируем клик только один раз
                if (touchSimulator != null)
                {
                    touchSimulator.ClickAt(screenPosition);
                }

                TouchPointDraw(screenPosition);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Ошибка обработки данных: {e}");
        }
    }

    void TouchPointDraw(Vector2 screenPosition)
    {
        if (debugCanvas == null) return;

        GameObject circle = new GameObject("DebugCircle");
        circle.transform.SetParent(debugCanvas.transform);

        Image circleImage = circle.AddComponent<Image>();
        circleImage.color = new Color(1f, 0f, 0f, 0.5f);

        RectTransform rectTransform = circle.GetComponent<RectTransform>();

        // Преобразуем экранные координаты в локальные координаты Canvas
        Vector2 localPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            debugCanvas.GetComponent<RectTransform>(),
            screenPosition,
            null,
            out localPosition
        );

        rectTransform.anchoredPosition = localPosition;
        rectTransform.sizeDelta = new Vector2(20f, 20f);

        // Удаляем объект через 0.5 секунды
        Destroy(circle, 0.5f);
    }

    private void StopReceiving()
    {
        isReceiving = false;

        if (client != null)
        {
            try
            {
                client.Close();
                client = null;
                Debug.Log("TouchManager: UDP клиент закрыт");
            }
            catch (Exception e)
            {
                Debug.LogError($"TouchManager: Ошибка закрытия UDP клиента: {e.Message}");
            }
        }

        if (receiveThread != null && receiveThread.IsAlive)
        {
            try
            {
                receiveThread.Abort();
                receiveThread = null;
                Debug.Log("TouchManager: Поток остановлен");
            }
            catch (Exception e)
            {
                Debug.LogError($"TouchManager: Ошибка остановки потока: {e.Message}");
            }
        }
    }

    void OnApplicationQuit()
    {
        StopReceiving();
    }

    void OnDestroy()
    {
        StopReceiving();
    }

    // Публичные методы для управления
    public void RestartReceiving()
    {
        Debug.Log("TouchManager: Перезапуск приема данных");
        StopReceiving();
        System.Threading.Thread.Sleep(100); // Небольшая задержка для освобождения порта
        StartReceiving();
    }

    public bool IsReceiving()
    {
        return isReceiving;
    }

    public void SetTouchSimulator(TouchSimulator simulator)
    {
        touchSimulator = simulator;
    }
}

[System.Serializable]
public class TouchDataWrapper
{
    public TouchPoint[] touches;
    public double timestamp;
}

[System.Serializable]
public class TouchPoint
{
    public float x;
    public float y;
    public string type;
}