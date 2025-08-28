using UnityEngine;
using System.Collections;

public class LevelTransitionEffect : MonoBehaviour
{
    [Header("Настройки эффекта")]
    [SerializeField] private GameObject particlePrefab;
    [SerializeField] private int numberOfParticles = 30;
    [SerializeField] private float explosionForce = 5f;
    [SerializeField] private float effectDuration = 1.5f; // Время полета частицы до начала исчезновения
    [SerializeField] private float fadeDuration = 1.5f;   // Длительность самого затухания

    [Header("Настройки области взрыва")]
    [SerializeField] private Vector2 explosionArea = new Vector2(4f, 4f);

    [Header("Случайные размеры частичек")]
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 1.5f;

    void Start()
    {
        TriggerEffect();
        // Уничтожаем объект-контейнер после того, как все частицы точно исчезнут
        Destroy(gameObject, effectDuration + fadeDuration + 0.1f);
    }

    private void TriggerEffect()
    {
        if (particlePrefab == null)
        {
            Debug.LogError("Префаб частицы не назначен в LevelTransitionEffect!");
            return;
        }

        Vector3 centerPosition = transform.position;

        for (int i = 0; i < numberOfParticles; i++)
        {
            float randomX = Random.Range(-explosionArea.x / 2, explosionArea.x / 2);
            float randomZ = Random.Range(-explosionArea.y / 2, explosionArea.y / 2);
            Vector3 spawnPosition = centerPosition + new Vector3(randomX, 0, randomZ);
            
            GameObject particle = Instantiate(particlePrefab, spawnPosition, Quaternion.identity);

            float randomScale = Random.Range(minScale, maxScale);
            particle.transform.localScale = Vector3.one * randomScale;

            Rigidbody rb = particle.AddComponent<Rigidbody>();
            
            Vector3 randomDirection = new Vector3(
                Random.Range(-0.5f, 0.5f),
                Random.Range(1f, 1.5f),
                Random.Range(-0.5f, 0.5f)
            );

            rb.AddForce(randomDirection * explosionForce, ForceMode.Impulse);

            StartCoroutine(FadeAndDestroyParticle(particle));
        }
    }

    private IEnumerator FadeAndDestroyParticle(GameObject particle)
    {
        // Ждем, пока частица полетает
        yield return new WaitForSeconds(effectDuration);

        // --- КЛЮЧЕВОЕ ИСПРАВЛЕНИЕ ---
        // "Замораживаем" частицу, чтобы она перестала падать
        Rigidbody rb = particle.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true; // Отключаем физику (гравитацию, скорость)
        }
        // ------------------------------

        // Получаем доступ к рендереру и материалу
        Renderer renderer = particle.GetComponent<Renderer>();
        if (renderer == null)
        {
            Destroy(particle);
            yield break;
        }

        Material material = renderer.material;
        Color startColor = material.color;
        float elapsedTime = 0f;

        // Цикл плавного изменения прозрачности
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            
            // Вычисляем новую прозрачность от 1 до 0
            float newAlpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            
            // Применяем новый цвет
            material.color = new Color(startColor.r, startColor.g, startColor.b, newAlpha);

            yield return null;
        }

        // Уничтожаем объект после того, как он стал полностью невидимым
        Destroy(particle);
    }
}