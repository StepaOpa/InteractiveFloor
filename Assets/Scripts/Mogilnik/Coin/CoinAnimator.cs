using UnityEngine;
using System.Collections;

public class CoinAnimator : MonoBehaviour
{
    [Header("Настройки анимации")]
    [SerializeField] private float dropDuration = 0.5f;
    [SerializeField] private float spinDuration = 1f;
    [SerializeField] private float bounceHeight = 0.2f;
    [SerializeField] private float pauseBetweenSpins = 3f;
    
    [Tooltip("Ось вращения монетки. Для вас это (0,0,1)")]
    [SerializeField] private Vector3 rotationAxis = new Vector3(0, 0, 1);

    [SerializeField] private AnimationCurve dropCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public void AnimateCoin(Vector3 startPos, Vector3 endPos, float dropDelay, float spinDelay)
    {
        StartCoroutine(AnimationSequence(startPos, endPos, dropDelay, spinDelay));
    }

    private IEnumerator AnimationSequence(Vector3 startPos, Vector3 endPos, float dropDelay, float spinDelay)
    {
        transform.position = startPos;
        transform.rotation = Quaternion.identity;
        
        yield return new WaitForSecondsRealtime(dropDelay);
        yield return StartCoroutine(DropCoroutine(startPos, endPos));
        yield return new WaitForSecondsRealtime(spinDelay);
        StartCoroutine(SpinLoopCoroutine());
    }

    private IEnumerator DropCoroutine(Vector3 startPos, Vector3 endPos)
    {
        // Код падения (не трогаем, он работает)
        float elapsedTime = 0f;
        while (elapsedTime < dropDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / dropDuration;
            transform.position = Vector3.LerpUnclamped(startPos, endPos, dropCurve.Evaluate(t));
            yield return null;
        }
        transform.position = endPos;

        // Отскок (не трогаем)
        Vector3 bounceStartPos = endPos;
        Vector3 bouncePeakPos = endPos + Vector3.up * bounceHeight;
        float bounceDuration = dropDuration / 2.5f;
        elapsedTime = 0f;
        while (elapsedTime < bounceDuration)
        {
            transform.position = Vector3.Lerp(bounceStartPos, bouncePeakPos, elapsedTime / bounceDuration);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        elapsedTime = 0f;
        while (elapsedTime < bounceDuration)
        {
            transform.position = Vector3.Lerp(bouncePeakPos, endPos, elapsedTime / bounceDuration);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        transform.position = endPos;
    }
    
    // --- ВОЗВРАЩАЕМ РАБОЧИЙ МЕТОД С ИСПРАВЛЕНИЕМ ТОЧНОСТИ ---
    private IEnumerator SpinLoopCoroutine()
    {
        // Запоминаем идеальное положение перед началом всех вращений
        Quaternion initialRotation = transform.rotation;

        while (true)
        {
            float elapsedTime = 0f;
            float rotationSpeed = 360f / spinDuration;

            // Крутим монетку, как и раньше
            while (elapsedTime < spinDuration)
            {
                transform.Rotate(rotationAxis, rotationSpeed * Time.unscaledDeltaTime, Space.Self);
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }
            
            // А ТЕПЕРЬ ГЛАВНОЕ: после оборота принудительно ставим вращение в идеальное начальное положение.
            // Это исправляет любой "недоворот" или "переворот".
            transform.rotation = initialRotation;
            
            // Ждем паузу перед следующим идеальным оборотом
            yield return new WaitForSecondsRealtime(pauseBetweenSpins);
        }
    }
}