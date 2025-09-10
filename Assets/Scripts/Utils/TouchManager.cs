using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine.UI;

public class TouchManager : MonoBehaviour
{
    Thread receiveThread;
    UdpClient client;
    public int port = 8052;
    private string latestData = "";
    private bool newDataAvailable = false;

    [SerializeField] private GameObject debugCanvas;

    void Start()
    {
        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    private void ReceiveData()
    {
        client = new UdpClient(port);
        while (true)
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
                Debug.Log(err.ToString());
            }
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
                Vector2 position = new Vector2(touch.x * Screen.width - Screen.width / 2, (1 - touch.y) * Screen.height - Screen.height / 2);
                Debug.Log($"Касание: {position}");

                // Ваша логика обработки касаний здесь
                TouchPointDraw(position);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Ошибка обработки данных: {e}");
        }
    }

    void TouchPointDraw(Vector2 position)
    {
        GameObject circle = new GameObject("DebugCircle");
        circle.transform.SetParent(debugCanvas.transform);

        Image circleImage = circle.AddComponent<Image>();
        circleImage.color = new Color(1f, 0f, 0f, 0.5f);

        RectTransform rectTransform = circle.GetComponent<RectTransform>();
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = new Vector2(20f, 20f);
    }

    void OnApplicationQuit()
    {
        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Abort();
        }
        if (client != null)
        {
            client.Close();
        }
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