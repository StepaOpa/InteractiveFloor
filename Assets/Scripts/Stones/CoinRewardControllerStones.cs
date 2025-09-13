using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class CoinRewardControllerStones : MonoBehaviour
{
    [Header("Ссылки на объекты")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private Transform coinContainer;

    [Header("Настройки расположения")]
    [SerializeField] private float coinSpacing = 1.0f;
    [Tooltip("Смещение стартовой позиции относительно финальной в ЛОКАЛЬНЫХ координатах контейнера.")]
    [SerializeField] private Vector3 startPositionOffset = new Vector3(0, 5f, 0); // "5 единиц вверх" относительно контейнера

    [Header("Настройки анимации")]
    [SerializeField] private float delayBetweenDrops = 0.1f;
    [SerializeField] private float delayBetweenSpins = 0.05f;

    private List<GameObject> spawnedCoins = new List<GameObject>();

    public IEnumerator GetRewardSequenceCoroutine(int numberOfCoins)
    {
        yield return RewardSequenceCoroutine(numberOfCoins);
    }

    private IEnumerator RewardSequenceCoroutine(int count)
    {
        ClearCoins();
        if (coinContainer == null)
        {
            Debug.LogError("Контейнер для монет (CoinContainer) не назначен в инспекторе!");
            yield break;
        }

        float totalWidth = (count - 1) * coinSpacing;
        Vector3 rowOffset = new Vector3(-totalWidth / 2, 0, 0);

        for (int i = 0; i < count; i++)
        {
            // 1. Рассчитываем финальную позицию монеты в ЛОКАЛЬНОМ пространстве контейнера.
            Vector3 localTargetPosition = rowOffset + new Vector3(i * coinSpacing, 0, 0);

            // 2. Превращаем локальную целевую позицию в мировую (для вычисления старта).
            Vector3 worldTargetPosition = coinContainer.TransformPoint(localTargetPosition);

            // 3. Превращаем ЛОКАЛЬНЫЙ вектор смещения в МИРОВОЙ вектор, учитывая поворот контейнера.
            Vector3 worldOffsetVector = coinContainer.TransformDirection(startPositionOffset);

            // 4. Считаем мировую стартовую позицию.
            Vector3 worldStartPosition = worldTargetPosition + worldOffsetVector;

            // 5. Создаем монету КАК ДОЧЕРНИЙ ОБЪЕКТ контейнера.
            GameObject coinObject = Instantiate(coinPrefab, coinContainer);
            spawnedCoins.Add(coinObject);

            // 6. Конвертируем мировую стартовую позицию ОБРАТНО в локальную для аниматора.
            Vector3 localStartPosition = coinContainer.InverseTransformPoint(worldStartPosition);

            CoinAnimatorStones animator = coinObject.GetComponent<CoinAnimatorStones>();
            if (animator != null)
            {
                // 7. Передаем в аниматор ЛОКАЛЬНЫЕ координаты.
                animator.AnimateCoin(localStartPosition, localTargetPosition, i * delayBetweenDrops, i * delayBetweenSpins, Quaternion.identity);
            }
            else
            {
                Debug.LogError("На префабе монеты отсутствует скрипт CoinAnimatorStones!", coinObject);
            }
            yield return null;
        }
    }

    public void ClearCoins()
    {
        foreach (var coin in spawnedCoins)
        {
            if (coin != null) Destroy(coin);
        }
        spawnedCoins.Clear();
    }
}