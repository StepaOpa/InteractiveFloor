// Файл: CoinAnimatorIcebreaker.cs

using UnityEngine;
using System.Collections;

public class CoinAnimatorIcebreaker : MonoBehaviour
{
    [Header("Настройки анимации")]
    [SerializeField] private float dropDuration = 0.5f;
    [SerializeField] private float spinDuration = 1f;
    [SerializeField] private float bounceHeight = 0.2f;
    [SerializeField] private float pauseBetweenSpins = 3f;
    [SerializeField] private Vector3 rotationAxis = new Vector3(0, 0, 1);
    [SerializeField] private AnimationCurve dropCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public void AnimateCoin(Vector3 localStartPos, Vector3 localEndPos, float dropDelay, float spinDelay)
    {
        StartCoroutine(AnimationSequence(localStartPos, localEndPos, dropDelay, spinDelay));
    }

    private IEnumerator AnimationSequence(Vector3 startPos, Vector3 endPos, float dropDelay, float spinDelay)
    {
        transform.localPosition = startPos;
        transform.localRotation = Quaternion.identity;

        yield return new WaitForSecondsRealtime(dropDelay);
        yield return StartCoroutine(DropCoroutine(startPos, endPos));
        yield return new WaitForSecondsRealtime(spinDelay);
        StartCoroutine(SpinLoopCoroutine());
    }

    private IEnumerator DropCoroutine(Vector3 startPos, Vector3 endPos)
    {
        // Падение
        float elapsedTime = 0f;
        while (elapsedTime < dropDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / dropDuration;
            transform.localPosition = Vector3.LerpUnclamped(startPos, endPos, dropCurve.Evaluate(t));
            yield return null;
        }
        transform.localPosition = endPos;

        // Отскок в ЛОКАЛЬНОМ пространстве. Это правильно, монета будет отскакивать
        // "вверх" относительно своего родителя (контейнера).
        Vector3 bounceStartPos = endPos;
        Vector3 bouncePeakPos = endPos + Vector3.up * bounceHeight;
        float bounceDuration = dropDuration / 2.5f;

        // Вверх
        elapsedTime = 0f;
        while (elapsedTime < bounceDuration)
        {
            transform.localPosition = Vector3.Lerp(bounceStartPos, bouncePeakPos, elapsedTime / bounceDuration);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }

        // Вниз
        elapsedTime = 0f;
        while (elapsedTime < bounceDuration)
        {
            transform.localPosition = Vector3.Lerp(bouncePeakPos, endPos, elapsedTime / bounceDuration);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        transform.localPosition = endPos;
    }

    private IEnumerator SpinLoopCoroutine()
    {
        Quaternion initialRotation = transform.localRotation;
        while (true)
        {
            float elapsedTime = 0f;
            float rotationSpeed = 360f / spinDuration;
            while (elapsedTime < spinDuration)
            {
                transform.Rotate(rotationAxis, rotationSpeed * Time.unscaledDeltaTime, Space.Self);
                elapsedTime += Time.unscaledDeltaTime;
                yield return null;
            }
            transform.localRotation = initialRotation;
            yield return new WaitForSecondsRealtime(pauseBetweenSpins);
        }
    }
}