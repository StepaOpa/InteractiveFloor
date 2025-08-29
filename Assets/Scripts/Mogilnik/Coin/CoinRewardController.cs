using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class CoinRewardController : MonoBehaviour
{
    [Header("Ссылки на объекты")]
    [SerializeField] private GameObject coinPrefab; // Сюда перетащим префаб нашей монетки
    [SerializeField] private Transform coinContainer; // Пустой объект, где будут создаваться монетки

    [Header("Настройки расположения")]
    [SerializeField] private float coinSpacing = 0.6f; // Расстояние между центрами монеток
    [SerializeField] private Vector3 startPositionOffset = new Vector3(0, 5f, 0); // Насколько высоко над центром будут появляться монетки

    [Header("Настройки анимации")]
    [SerializeField] private float delayBetweenDrops = 0.2f; // Задержка между падением каждой следующей монетки
    [SerializeField] private float delayBetweenSpins = 0.1f; // Задержка между вращением каждой следующей монетки

    private List<GameObject> spawnedCoins = new List<GameObject>();

// В файле CoinRewardController.cs

    public void StartRewardSequence()
    {
    // Убираем случайность и жестко задаем 1 монету
    int numberOfCoins = 1; 

    // Запускаем всю последовательность
    StartCoroutine(RewardSequenceCoroutine(numberOfCoins));
    }
    // В файле CoinRewardController.cs

    private IEnumerator RewardSequenceCoroutine(int count) // count мы больше не используем, но оставим для совместимости
    {
    // 1. Очищаем старые монетки
    ClearCoins();

    // Ждем один кадр, чтобы Unity успел удалить старые объекты, если они были
    yield return null; 

    // 2. Рассчитываем позиции ровно для одной монетки в центре
    // Финальная позиция - это просто центр нашего контейнера
    Vector3 targetPosition = coinContainer.position;

    // Начальная позиция будет высоко над финальной
    Vector3 startPosition = targetPosition + startPositionOffset;

    // 3. Создаем одну монетку
    GameObject coinObject = Instantiate(coinPrefab, coinContainer);
    spawnedCoins.Add(coinObject);

    // 4. Запускаем ее анимацию без задержек
    CoinAnimator animator = coinObject.GetComponent<CoinAnimator>();
    if (animator != null)
    {
        // Передаем позиции и нулевые задержки, так как монетка одна
        animator.AnimateCoin(startPosition, targetPosition, 0f, 0f);
    }
    }

    // Метод для очистки монеток (полезен при перезапуске)
    public void ClearCoins()
    {
        foreach (var coin in spawnedCoins)
        {
            Destroy(coin);
        }
        spawnedCoins.Clear();
    }
}

