using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System; // Нужно для использования Action

public class FlyingIconAnimator : MonoBehaviour
{
    [Header("Компоненты для анимации")]
    [SerializeField] private Image iconImage; // Сюда перетащим основное изображение
    [SerializeField] private Transform shineTransform; // Сюда перетащим объект "сияния"

    [Header("Настройки сияния")]
    [SerializeField] private float shineRotationSpeed = 90f; // Скорость вращения лучей

    // Этот метод будет запускать всю анимацию извне
    public void StartAnimation(Sprite sprite, Vector3 startPos, Vector3 endPos, float speed, float startScale, Action onComplete)
    {
        // Устанавливаем спрайт и начальные значения
        iconImage.sprite = sprite;
        transform.position = startPos;
        transform.localScale = Vector3.one * startScale;

        // Запускаем корутину, которая выполнит всю работу
        StartCoroutine(AnimateCoroutine(startPos, endPos, speed, startScale, onComplete));
    }

    private IEnumerator AnimateCoroutine(Vector3 startPos, Vector3 endPos, float speed, float startScale, Action onComplete)
    {
        // Включаем объект сияния в начале анимации
        if (shineTransform != null)
        {
            shineTransform.gameObject.SetActive(true);
        }

        Vector3 finalScale = Vector3.one;

        float distance = Vector3.Distance(startPos, endPos);
        float duration = (speed > 0) ? distance / speed : 0;

        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            float smoothedT = 1f - Mathf.Cos(t * Mathf.PI * 0.5f);

            // Анимируем позицию и размер
            transform.position = Vector3.Lerp(startPos, endPos, smoothedT);
            transform.localScale = Vector3.Lerp(Vector3.one * startScale, finalScale, smoothedT);

            // Вращаем сияние для красивого эффекта
            if (shineTransform != null)
            {
                shineTransform.Rotate(0, 0, shineRotationSpeed * Time.deltaTime);
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Вызываем событие "onComplete", чтобы сообщить GameManager'у, что мы закончили
        onComplete?.Invoke();

        // Уничтожаем объект после завершения
        Destroy(gameObject);
    }
}