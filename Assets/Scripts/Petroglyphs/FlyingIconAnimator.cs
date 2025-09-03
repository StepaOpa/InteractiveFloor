using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class FlyingIconAnimator : MonoBehaviour
{
    [Header("Компоненты для анимации")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Transform shineTransform;

    [Header("Настройки сияния")]
    [SerializeField] private float shineRotationSpeed = 90f;
    [SerializeField] private Gradient shineColorGradient; // --- НОВАЯ СТРОКА ---
    [SerializeField] private float colorChangeSpeed = 1f; // --- НОВАЯ СТРОКА ---

    private Image shineImage; // Ссылка на компонент Image сияния

    // Awake вызывается раньше Start, идеально для инициализации
    private void Awake()
    {
        // Получаем компонент Image у сияния один раз, чтобы не делать это в цикле
        if (shineTransform != null)
        {
            shineImage = shineTransform.GetComponent<Image>();
        }
    }

    public void StartAnimation(Sprite sprite, Vector3 startPos, Vector3 endPos, float speed, float startScale, Action onComplete)
    {
        iconImage.sprite = sprite;
        transform.position = startPos;
        transform.localScale = Vector3.one * startScale;

        StartCoroutine(AnimateCoroutine(startPos, endPos, speed, startScale, onComplete));
    }

    private IEnumerator AnimateCoroutine(Vector3 startPos, Vector3 endPos, float speed, float startScale, Action onComplete)
    {
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

            transform.position = Vector3.Lerp(startPos, endPos, smoothedT);
            transform.localScale = Vector3.Lerp(Vector3.one * startScale, finalScale, smoothedT);

            if (shineTransform != null)
            {
                shineTransform.Rotate(0, 0, shineRotationSpeed * Time.deltaTime);

                // --- НОВАЯ ЛОГИКА ИЗМЕНЕНИЯ ЦВЕТА ---
                if (shineImage != null && shineColorGradient != null)
                {
                    // Time.time * colorChangeSpeed дает постоянно растущее число.
                    // Оператор % 1f заставляет его циклически повторяться в диапазоне от 0 до 1.
                    // Gradient.Evaluate(t) берет цвет из градиента в точке t (от 0 до 1).
                    float colorT = (Time.time * colorChangeSpeed) % 1f;
                    shineImage.color = shineColorGradient.Evaluate(colorT);
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        onComplete?.Invoke();
        Destroy(gameObject);
    }
}