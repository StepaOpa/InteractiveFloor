using UnityEngine;
using System.Collections;

public class CoinAnimator : MonoBehaviour
{
    [Header("Настройки анимации")]
    [SerializeField] private float dropDuration = 0.5f;
    [SerializeField] private float spinDuration = 0.4f;
    [SerializeField] private float bounceHeight = 0.5f; // ВНИМАНИЕ: Теперь это мировые единицы, а не пиксели. Значение должно быть маленьким.
    [SerializeField] private float pauseBetweenSpins = 3f; // Пауза между вращениями
    [SerializeField] private AnimationCurve dropCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    // Убрали Image и RectTransform, так как работаем с 3D-объектом

    public void AnimateCoin(Vector3 startPos, Vector3 endPos, float dropDelay, float spinDelay)
    {
        StartCoroutine(AnimationSequence(startPos, endPos, dropDelay, spinDelay));
    }

// В файле CoinAnimator.cs

private IEnumerator AnimationSequence(Vector3 startPos, Vector3 endPos, float dropDelay, float spinDelay)
{
    // Прячем монетку, чтобы она не мелькнула в начале <-- УДАЛЯЕМ ЭТУ ЛОГИКУ
    // gameObject.SetActive(false); // <--- УДАЛИТЬ
    transform.position = startPos;
    // Сбрасываем вращение
    transform.rotation = Quaternion.identity;

    // Просто ждем задержку, пока монетка висит в воздухе
    yield return new WaitForSeconds(dropDelay);
    
    // Показываем монетку <-- ЭТО ТОЖЕ БОЛЬШЕ НЕ НУЖНО
    // gameObject.SetActive(true); // <--- УДАЛИТЬ

    // Запускаем падение
    yield return StartCoroutine(DropCoroutine(startPos, endPos));
    
    // Ждем своей очереди, прежде чем начать вращаться
    yield return new WaitForSeconds(spinDelay);
    
    // Запускаем бесконечный цикл вращений
    StartCoroutine(SpinLoopCoroutine());
}

    private IEnumerator DropCoroutine(Vector3 startPos, Vector3 endPos)
    {
        float elapsedTime = 0f;

        while (elapsedTime < dropDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / dropDuration;
            transform.position = Vector3.LerpUnclamped(startPos, endPos, dropCurve.Evaluate(t));
            yield return null;
        }
        transform.position = endPos;

        // Отскок в 3D
        Vector3 bounceStartPos = endPos;
        Vector3 bouncePeakPos = endPos + Vector3.up * bounceHeight;
        float bounceDuration = dropDuration / 2.5f;

        // Вверх
        elapsedTime = 0f;
        while (elapsedTime < bounceDuration)
        {
            transform.position = Vector3.Lerp(bounceStartPos, bouncePeakPos, elapsedTime / bounceDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Вниз
        elapsedTime = 0f;
        while (elapsedTime < bounceDuration)
        {
            transform.position = Vector3.Lerp(bouncePeakPos, bounceStartPos, elapsedTime / bounceDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = endPos;
    }

    // Вращение для 3D-объекта
    private IEnumerator SpinLoopCoroutine()
    {
        // Бесконечный цикл
        while (true)
        {
            float elapsedTime = 0f;
            Quaternion startRotation = transform.rotation;
            // Вращаем на 360 градусов вокруг своей оси Y (вертикальная ось)
            Quaternion endRotation = startRotation * Quaternion.Euler(0, 360, 0); 

            while (elapsedTime < spinDuration)
            {
                // Плавное вращение через Slerp
                transform.rotation = Quaternion.Slerp(startRotation, endRotation, elapsedTime / spinDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            // Гарантируем точное конечное положение
            transform.rotation = startRotation;

            // Ждем 3 секунды перед следующим вращением
            yield return new WaitForSeconds(pauseBetweenSpins);
        }
    }
}
