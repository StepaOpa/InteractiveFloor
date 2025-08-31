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

    // Главный публичный метод, который мы будем вызывать извне (например, из EndGamePanelUI)
    // ИЗМЕНЕНО: Теперь метод принимает количество монет (очков) как аргумент
    public void StartRewardSequence(int numberOfCoins)
    {
        // УБРАНО: Возвращаем случайное количество от 1 до 10
        // int numberOfCoins = Random.Range(1, 11); 

        // Запускаем всю последовательность
        StartCoroutine(RewardSequenceCoroutine(numberOfCoins));
    }

    private IEnumerator RewardSequenceCoroutine(int count)
    {
        // Перед началом всегда очищаем старые монетки, если они были
        ClearCoins();

        // 1. Рассчитываем позиции для всех монеток
        // Общая ширина всего ряда монеток
        float totalWidth = (count - 1) * coinSpacing;
        // Находим позицию самой левой монетки, чтобы весь ряд был отцентрован
        Vector3 startOffset = new Vector3(-totalWidth / 2, 0, 0);

        // 2. Создаем монетки и запускаем их анимацию
        for (int i = 0; i < count; i++)
        {
            // Вычисляем финальную позицию для текущей монетки
            Vector3 targetPosition = coinContainer.position + startOffset + new Vector3(i * coinSpacing, 0, 0);

            // Начальная позиция будет высоко над финальной
            Vector3 startPosition = targetPosition + startPositionOffset;

            // Создаем саму монетку
            GameObject coinObject = Instantiate(coinPrefab, coinContainer);
            spawnedCoins.Add(coinObject);

            // Получаем ее скрипт аниматора
            CoinAnimator animator = coinObject.GetComponent<CoinAnimator>();
            if (animator != null)
            {
                // Даем команду на анимацию, передавая все нужные параметры:
                // начальную и конечную позицию, а также задержки для падения и вращения.
                animator.AnimateCoin(startPosition, targetPosition, i * delayBetweenDrops, i * delayBetweenSpins);
            }

            // Ждем следующего кадра перед созданием новой монетки, чтобы не было лагов
            yield return null;
        }
    }

    // Метод для очистки монеток (полезен при перезапуске)
    public void ClearCoins()
    {
        foreach (var coin in spawnedCoins)
        {
            if (coin != null) // Добавим проверку на всякий случай
            {
                Destroy(coin);
            }
        }
        spawnedCoins.Clear();
    }
}