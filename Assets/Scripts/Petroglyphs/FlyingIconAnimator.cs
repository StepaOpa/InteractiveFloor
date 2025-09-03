using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class FlyingIconAnimator : MonoBehaviour
{
    [Header("���������� ��� ��������")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Transform shineTransform;

    [Header("��������� ������")]
    [SerializeField] private float shineRotationSpeed = 90f;
    [SerializeField] private Gradient shineColorGradient; // --- ����� ������ ---
    [SerializeField] private float colorChangeSpeed = 1f; // --- ����� ������ ---

    private Image shineImage; // ������ �� ��������� Image ������

    // Awake ���������� ������ Start, �������� ��� �������������
    private void Awake()
    {
        // �������� ��������� Image � ������ ���� ���, ����� �� ������ ��� � �����
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

                // --- ����� ������ ��������� ����� ---
                if (shineImage != null && shineColorGradient != null)
                {
                    // Time.time * colorChangeSpeed ���� ��������� �������� �����.
                    // �������� % 1f ���������� ��� ���������� ����������� � ��������� �� 0 �� 1.
                    // Gradient.Evaluate(t) ����� ���� �� ��������� � ����� t (�� 0 �� 1).
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