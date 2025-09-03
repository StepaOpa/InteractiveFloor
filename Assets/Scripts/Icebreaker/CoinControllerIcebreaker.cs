// Файл: CoinControllerIcebreaker.cs

using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class CoinControllerIcebreaker : MonoBehaviour
{
    [Header("Ссылки на объекты")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private Transform coinContainer;

    [Header("Настройки расположения")]
    [SerializeField] private float coinSpacing = 0.6f;
    [Tooltip("Смещение стартовой позиции относительно финальной в ЛОКАЛЬНЫХ координатах контейнера.")]
    [SerializeField] private Vector3 startPositionOffset = new Vector3(0, 0, 5f);

    [Header("Настройки анимации")]
    [SerializeField] private float delayBetweenDrops = 0.2f;
    [SerializeField] private float delayBetweenSpins = 0.1f;

    private List<GameObject> spawnedCoins = new List<GameObject>();

    public void StartRewardSequence(int numberOfCoins)
    {
        StartCoroutine(RewardSequenceCoroutine(numberOfCoins));
    }

    private IEnumerator RewardSequenceCoroutine(int count)
    {
        ClearCoins();
        float totalWidth = (count - 1) * coinSpacing;
        Vector3 rowOffset = new Vector3(-totalWidth / 2, 0, 0);

        for (int i = 0; i < count; i++)
        {
            // 1. Рассчитываем финальную позицию монеты в ЛОКАЛЬНОМ пространстве.
            Vector3 localTargetPosition = rowOffset + new Vector3(i * coinSpacing, 0, 0);

            // === ГЛАВНОЕ ИЗМЕНЕНИЕ ===
            // 2. Превращаем локальную целевую позицию в мировую.
            Vector3 worldTargetPosition = coinContainer.TransformPoint(localTargetPosition);

            // 3. Превращаем ЛОКАЛЬНЫЙ вектор смещения в МИРОВОЙ вектор.
            //    TransformDirection учитывает поворот контейнера, но не его позицию.
            Vector3 worldOffsetVector = coinContainer.TransformDirection(startPositionOffset);

            // 4. Добавляем мировой вектор смещения к мировой целевой позиции.
            Vector3 worldStartPosition = worldTargetPosition + worldOffsetVector;

            // 5. Превращаем мировую стартовую позицию обратно в локальную для аниматора.
            Vector3 localStartPosition = coinContainer.InverseTransformPoint(worldStartPosition);
            // =========================

            GameObject coinObject = Instantiate(coinPrefab, coinContainer);
            spawnedCoins.Add(coinObject);

            CoinAnimatorIcebreaker animator = coinObject.GetComponent<CoinAnimatorIcebreaker>();
            if (animator != null)
            {
                animator.AnimateCoin(localStartPosition, localTargetPosition, i * delayBetweenDrops, i * delayBetweenSpins);
            }
            else
            {
                Debug.LogError("На префабе монеты отсутствует скрипт CoinAnimatorIcebreaker!");
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