using UnityEngine;
using System.Collections;

public class CameraControllerLabyrinth : MonoBehaviour
{
    [Header("Цели для камеры")]
    public Transform closeViewTarget;
    public Transform farViewTarget;

    [Header("Ссылки")]
    public PlayerControllerLabyrinth playerController;

    [Header("Настройки камеры")] // Изменил заголовок для ясности
    // <<< ВОТ ИСПРАВЛЕНИЕ: Возвращаем переменную для задержки >>>
    [Tooltip("Задержка в секундах перед автоматическим приближением в начале игры")]
    public float initialZoomDelay = 2.0f;

    [Tooltip("Время приближения в начале. Чем больше значение, тем медленнее камера.")]
    public float initialZoomSmoothTime = 1.5f;

    [Tooltip("Время слежения в игре. Чем меньше значение, тем быстрее камера.")]
    public float gameplaySmoothTime = 0.2f;

    private Vector3 velocity = Vector3.zero;
    private bool isCloseView;
    private float currentSmoothTime;

    void Start()
    {
        currentSmoothTime = initialZoomSmoothTime;
        StartCoroutine(InitialZoomInSequence());
    }

    void LateUpdate()
    {
        Transform target = isCloseView ? closeViewTarget : farViewTarget;
        transform.position = Vector3.SmoothDamp(transform.position, target.position, ref velocity, currentSmoothTime);

        Quaternion smoothedRotation = Quaternion.Lerp(transform.rotation, target.rotation, 5f * Time.deltaTime);
        transform.rotation = smoothedRotation;
    }

    private IEnumerator InitialZoomInSequence()
    {
        playerController.canMove = false;
        isCloseView = false;
        transform.position = farViewTarget.position;
        transform.rotation = farViewTarget.rotation;

        // Теперь эта строка снова работает, так как переменная существует
        yield return new WaitForSeconds(initialZoomDelay);

        isCloseView = true;

        while (Vector3.Distance(transform.position, closeViewTarget.position) > 0.1f)
        {
            yield return null;
        }

        currentSmoothTime = gameplaySmoothTime;
        playerController.canMove = true;
    }

    public void SetCloseView()
    {
        isCloseView = true;
        currentSmoothTime = gameplaySmoothTime;
    }

    public void SetFarView()
    {
        isCloseView = false;
        currentSmoothTime = initialZoomSmoothTime;
    }

    public void SwitchToFarViewImmediately()
    {
        isCloseView = false;
        currentSmoothTime = initialZoomSmoothTime;
    }
}