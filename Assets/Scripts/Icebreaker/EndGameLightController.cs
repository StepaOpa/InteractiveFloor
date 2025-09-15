using UnityEngine;
using System.Collections;

public class EndGameLightController : MonoBehaviour
{
    [Tooltip("Объект-цель, указывающий конечную позицию и поворот света.")]
    [SerializeField] private Transform endGameTarget;

    [Tooltip("Как долго (в секундах) свет будет перемещаться в конечную точку.")]
    [SerializeField] private float transitionDuration = 2.5f;

    [Tooltip("Источник света, которым нужно управлять. Если пусто, скрипт найдет главный Directional Light.")]
    [SerializeField] private Light lightToMove;

    private Vector3 initialPosition;
    private Quaternion initialRotation;

    void Awake()
    {
        // Если свет не указан вручную, ищем его на сцене
        if (lightToMove == null)
        {
            lightToMove = FindObjectOfType<Light>();
            if (lightToMove == null)
            {
                Debug.LogError("На сцене не найден источник света для управления!", this);
            }
        }
    }

    /// <summary>
    /// Запускает плавное перемещение света в конечную позицию.
    /// </summary>
    public void StartTransition()
    {
        if (endGameTarget == null || lightToMove == null)
        {
            Debug.LogWarning("Цель для света (EndGameTarget) или сам источник света не назначены!", this);
            return;
        }

        StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        // Запоминаем начальные позицию и поворот
        initialPosition = lightToMove.transform.position;
        initialRotation = lightToMove.transform.rotation;

        float elapsedTime = 0f;

        // Цикл будет работать, пока не пройдет время transitionDuration
        while (elapsedTime < transitionDuration)
        {
            // Так как Time.timeScale будет 0, мы используем Time.unscaledDeltaTime,
            // чтобы анимация продолжалась даже на "паузе".
            elapsedTime += Time.unscaledDeltaTime;

            // Вычисляем прогресс от 0 до 1
            float progress = Mathf.Clamp01(elapsedTime / transitionDuration);

            // Плавно интерполируем позицию и поворот
            lightToMove.transform.position = Vector3.Lerp(initialPosition, endGameTarget.position, progress);
            lightToMove.transform.rotation = Quaternion.Slerp(initialRotation, endGameTarget.rotation, progress);

            yield return null; // Ждем следующего кадра
        }

        // Гарантированно устанавливаем финальные значения
        lightToMove.transform.position = endGameTarget.position;
        lightToMove.transform.rotation = endGameTarget.rotation;
    }
}