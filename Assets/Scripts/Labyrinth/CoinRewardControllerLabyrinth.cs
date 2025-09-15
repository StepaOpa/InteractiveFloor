using UnityEngine;
using System.Collections.Generic;
using System.Collections;

// Убедитесь, что имя класса совпадает с именем файла!
public class CoinRewardControllerLabyrinth : MonoBehaviour
{
    [Header("Ссылки на объекты")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private Transform coinContainer;

    [Header("Настройки расположения")]
    [SerializeField] private float coinSpacing = 0.6f;
    [SerializeField] private Vector3 startPositionOffset = new Vector3(0, 5f, 0);

    [Header("Настройки анимации")]
    [SerializeField] private float delayBetweenDrops = 0.2f;
    [SerializeField] private float delayBetweenSpins = 0.1f;

    private List<GameObject> spawnedCoins = new List<GameObject>();

    // <<< КЛЮЧЕВОЕ ИЗМЕНЕНИЕ >>>
    // Этот метод больше не запускает корутину сам.
    // Он просто ВОЗВРАЩАЕТ "инструкцию" (IEnumerator), чтобы GameManager мог ее запустить.
    public IEnumerator GetRewardSequenceCoroutine(int numberOfCoins)
    {
        // Раньше тут был StartCoroutine. Теперь мы просто вызываем метод,
        // а yield return "пробрасывает" его выполнение наверх.
        yield return RewardSequenceCoroutine(numberOfCoins);
    }

    // Этот метод остается без изменений, он все так же создает монеты
    private IEnumerator RewardSequenceCoroutine(int count)
    {
        ClearCoins();
        float totalWidth = (count - 1) * coinSpacing;
        Vector3 startOffset = new Vector3(-totalWidth / 2, 0, 0);

        for (int i = 0; i < count; i++)
        {
            Vector3 targetPosition = coinContainer.position + startOffset + new Vector3(i * coinSpacing, 0, 0);
            Vector3 startPosition = targetPosition + startPositionOffset;
            GameObject coinObject = Instantiate(coinPrefab, coinContainer);
            spawnedCoins.Add(coinObject);
            CoinAnimator animator = coinObject.GetComponent<CoinAnimator>();
            if (animator != null)
            {
                animator.AnimateCoin(startPosition, targetPosition, i * delayBetweenDrops, i * delayBetweenSpins);
            }
            yield return null;
        }
    }

    // Метод очистки остается без изменений
    public void ClearCoins()
    {
        foreach (var coin in spawnedCoins)
        {
            if (coin != null)
            {
                Destroy(coin);
            }
        }
        spawnedCoins.Clear();
    }
}