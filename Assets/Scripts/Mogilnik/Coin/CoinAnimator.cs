using UnityEngine;
using System.Collections;

public class CoinAnimator : MonoBehaviour
{
    [Header("Настройки анимации")]
    [SerializeField] private float dropDuration = 0.5f;
    [SerializeField] private float spinDuration = 0.7f;
    [SerializeField] private float bounceHeight = 0.2f;
    [SerializeField] private float pauseBetweenSpins = 3f;
    [SerializeField] private AnimationCurve dropCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public void AnimateCoin(Vector3 startPos, Vector3 endPos, float dropDelay, float spinDelay)
    {
        StartCoroutine(AnimationSequence(startPos, endPos, dropDelay, spinDelay));
    }

    private IEnumerator AnimationSequence(Vector3 startPos, Vector3 endPos, float dropDelay, float spinDelay)
    {
        // Устанавливаем начальную позицию. Монетка уже активна и будет видна.
        transform.position = startPos;
        transform.rotation = Quaternion.identity;

        // Запускаем падение.
        yield return StartCoroutine(DropCoroutine(startPos, endPos));
        
        // Ждем своей очереди, прежде чем начать вращаться.
        // Используем WaitForSecondsRealtime на случай, если игра все еще на паузе
        yield return new WaitForSecondsRealtime(spinDelay);
        
        // Запускаем бесконечный цикл вращений.
        StartCoroutine(SpinLoopCoroutine());
    }

    private IEnumerator DropCoroutine(Vector3 startPos, Vector3 endPos)
    {
        float elapsedTime = 0f;

        // --- Падение ---
        while (elapsedTime < dropDuration)
        {
            // Используем unscaledDeltaTime, чтобы анимация работала на паузе
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / dropDuration;
            transform.position = Vector3.LerpUnclamped(startPos, endPos, dropCurve.Evaluate(t));
            yield return null;
        }
        transform.position = endPos;

        // --- Отскок ---
        Vector3 bounceStartPos = endPos;
        Vector3 bouncePeakPos = endPos + Vector3.up * bounceHeight;
        float bounceDuration = dropDuration / 2.5f;

        // Вверх
        elapsedTime = 0f;
        while (elapsedTime < bounceDuration)
        {
            transform.position = Vector3.Lerp(bounceStartPos, bouncePeakPos, elapsedTime / bounceDuration);
            // Используем unscaledDeltaTime, чтобы анимация работала на паузе
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        // Вниз
        elapsedTime = 0f;
        while (elapsedTime < bounceDuration)
        {
            transform.position = Vector3.Lerp(bouncePeakPos, bounceStartPos, elapsedTime / bounceDuration);
            // Используем unscaledDeltaTime, чтобы анимация работала на паузе
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        transform.position = endPos;
    }

    private IEnumerator SpinLoopCoroutine()
    {
        // Бесконечный цикл
        while (true)
        {
            float elapsedTime = 0f;
            Quaternion startRotation = transform.rotation;
            Quaternion endRotation = startRotation * Quaternion.Euler(0, 360, 0); 

            while (elapsedTime < spinDuration)
            {
                // Используем unscaledDeltaTime, чтобы анимация работала на паузе
                transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsedTime / spinDuration);
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }
            transform.rotation = startRotation;

            // Используем WaitForSecondsRealtime, чтобы пауза работала даже при Time.timeScale = 0
            yield return new WaitForSecondsRealtime(pauseBetweenSpins);
        }
    }
}