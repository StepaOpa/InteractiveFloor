using UnityEngine;
using UnityEngine.UI; // <-- ВАЖНО: Добавили для работы с Image
using System.Collections;

public class CoinAnimator : MonoBehaviour
{
    [Header("Настройки анимации")]
    [SerializeField] private float dropDuration = 0.5f;
    [SerializeField] private float spinDuration = 0.5f; // Сделаем вращение чуть быстрее
    [SerializeField] private float bounceHeight = 30f; // <-- ВНИМАНИЕ: Значение стало больше (это пиксели)
    [SerializeField] private AnimationCurve dropCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Image coinImage;
    private RectTransform rectTransform; // <-- Работаем с RectTransform

    void Awake()
    {
        // Находим наши UI-компоненты
        coinImage = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void AnimateCoin(Vector3 startPos, Vector3 endPos, float dropDelay, float spinDelay)
    {
        StartCoroutine(AnimationSequence(startPos, endPos, dropDelay, spinDelay));
    }

    private IEnumerator AnimationSequence(Vector3 startPos, Vector3 endPos, float dropDelay, float spinDelay)
    {
        // Используем transform.position, так как Instantiate работает с мировыми координатами
        transform.position = startPos;
        
        // Прячем монетку, выключая компонент Image
        if (coinImage != null) coinImage.enabled = false;

        yield return new WaitForSeconds(dropDelay);
        
        // Показываем монетку
        if (coinImage != null) coinImage.enabled = true;

        yield return StartCoroutine(DropCoroutine(endPos)); // Передаем только конечную точку
        
        yield return new WaitForSeconds(spinDelay);
        
        StartCoroutine(SpinCoroutine());
    }

    // Корутина падения теперь работает с transform.position, как и раньше
    private IEnumerator DropCoroutine(Vector3 endPos)
    {
        float elapsedTime = 0f;
        Vector3 startPos = transform.position;

        while (elapsedTime < dropDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / dropDuration;
            transform.position = Vector3.LerpUnclamped(startPos, endPos, dropCurve.Evaluate(t));
            yield return null;
        }
        transform.position = endPos;

        // Отскок
        Vector3 bounceStartPos = endPos;
        Vector3 bouncePeakPos = endPos + Vector3.up * bounceHeight; // Отскок вверх в мировых координатах
        elapsedTime = 0f;
        float bounceDuration = dropDuration / 2.5f;

        while (elapsedTime < bounceDuration)
        {
            elapsedTime += Time.deltaTime;
            transform.position = Vector3.Lerp(bounceStartPos, bouncePeakPos, elapsedTime / bounceDuration);
            yield return null;
        }
        elapsedTime = 0f;
        while (elapsedTime < bounceDuration)
        {
            elapsedTime += Time.deltaTime;
            transform.position = Vector3.Lerp(bouncePeakPos, bounceStartPos, elapsedTime / bounceDuration);
            yield return null;
        }
        transform.position = endPos;
    }

    // --- ГЛАВНОЕ ИЗМЕНЕНИЕ: Вращение для 2D UI ---
    private IEnumerator SpinCoroutine()
    {
        float elapsedTime = 0f;
        Vector3 startAngles = rectTransform.localEulerAngles;
        // Вращаем на 360 градусов вокруг оси Z (ось, которая "смотрит" на нас)
        Vector3 endAngles = startAngles + new Vector3(0, 0, 360); 

        while (elapsedTime < spinDuration)
        {
            elapsedTime += Time.deltaTime;
            Vector3 newAngles = Vector3.Lerp(startAngles, endAngles, elapsedTime / spinDuration);
            rectTransform.localEulerAngles = newAngles;
            yield return null;
        }
        rectTransform.localEulerAngles = startAngles; // Возвращаем в исходное положение
    }
}