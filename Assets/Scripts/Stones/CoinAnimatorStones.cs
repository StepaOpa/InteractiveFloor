using UnityEngine;
using System.Collections;

public class CoinAnimatorStones : MonoBehaviour
{
    // === ВОТ ЭТОТ БЛОК ПЕРЕМЕННЫХ БЫЛ ПРОПУЩЕН ===
    [Header("Настройки анимации")]
    [SerializeField] private float dropDuration = 0.5f;
    [SerializeField] private float spinDuration = 1f;
    [SerializeField] private float bounceHeight = 0.2f;
    [SerializeField] private float pauseBetweenSpins = 3f;
    [SerializeField] private Vector3 rotationAxis = new Vector3(0, 1, 0);
    [SerializeField] private AnimationCurve dropCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    // ===============================================

    public void AnimateCoin(Vector3 localStartPos, Vector3 localEndPos, float dropDelay, float spinDelay, Quaternion initialRotation)
    {
        transform.localRotation = initialRotation;
        StartCoroutine(AnimationSequence(localStartPos, localEndPos, dropDelay, spinDelay));
    }

    private IEnumerator AnimationSequence(Vector3 localStartPos, Vector3 localEndPos, float dropDelay, float spinDelay)
    {
        transform.localPosition = localStartPos;

        yield return new WaitForSecondsRealtime(dropDelay);
        yield return StartCoroutine(DropCoroutine(localStartPos, localEndPos));
        yield return new WaitForSecondsRealtime(spinDelay);
        StartCoroutine(SpinLoopCoroutine());
    }

    private IEnumerator DropCoroutine(Vector3 localStartPos, Vector3 localEndPos)
    {
        float elapsedTime = 0f;
        while (elapsedTime < dropDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float t = elapsedTime / dropDuration;
            transform.localPosition = Vector3.LerpUnclamped(localStartPos, localEndPos, dropCurve.Evaluate(t));
            yield return null;
        }
        transform.localPosition = localEndPos;

        Vector3 bounceStartPos = localEndPos;
        Vector3 bouncePeakPos = localEndPos + Vector3.up * bounceHeight;
        float bounceDuration = dropDuration / 2.5f;
        elapsedTime = 0f;
        while (elapsedTime < bounceDuration)
        {
            transform.localPosition = Vector3.Lerp(bounceStartPos, bouncePeakPos, elapsedTime / bounceDuration);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        elapsedTime = 0f;
        while (elapsedTime < bounceDuration)
        {
            transform.localPosition = Vector3.Lerp(bouncePeakPos, localEndPos, elapsedTime / bounceDuration);
            elapsedTime += Time.unscaledDeltaTime;
            yield return null;
        }
        transform.localPosition = localEndPos;
    }

    // === И ПОЛНЫЙ КОД ЭТОГО МЕТОДА ===
    private IEnumerator SpinLoopCoroutine()
    {
        Quaternion initialLoopRotation = transform.localRotation;
        while (true)
        {
            float elapsedTime = 0f;
            float rotationSpeed = 360f / spinDuration;
            while (elapsedTime < spinDuration)
            {
                transform.Rotate(rotationAxis, rotationSpeed * Time.unscaledDeltaTime, Space.Self);
                elapsedTime += Time.unscaledDeltaTime;
                yield return null; // Эта строка важна для корутины
            }
            transform.localRotation = initialLoopRotation;
            yield return new WaitForSecondsRealtime(pauseBetweenSpins);
        }
    }
}