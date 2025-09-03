// ����: CoinControllerIcebreaker.cs

using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class CoinControllerIcebreaker : MonoBehaviour
{
    [Header("������ �� �������")]
    [SerializeField] private GameObject coinPrefab;
    [SerializeField] private Transform coinContainer;

    [Header("��������� ������������")]
    [SerializeField] private float coinSpacing = 0.6f;
    [Tooltip("�������� ��������� ������� ������������ ��������� � ��������� ����������� ����������.")]
    [SerializeField] private Vector3 startPositionOffset = new Vector3(0, 0, 5f);

    [Header("��������� ��������")]
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
            // 1. ������������ ��������� ������� ������ � ��������� ������������.
            Vector3 localTargetPosition = rowOffset + new Vector3(i * coinSpacing, 0, 0);

            // === ������� ��������� ===
            // 2. ���������� ��������� ������� ������� � �������.
            Vector3 worldTargetPosition = coinContainer.TransformPoint(localTargetPosition);

            // 3. ���������� ��������� ������ �������� � ������� ������.
            //    TransformDirection ��������� ������� ����������, �� �� ��� �������.
            Vector3 worldOffsetVector = coinContainer.TransformDirection(startPositionOffset);

            // 4. ��������� ������� ������ �������� � ������� ������� �������.
            Vector3 worldStartPosition = worldTargetPosition + worldOffsetVector;

            // 5. ���������� ������� ��������� ������� ������� � ��������� ��� ���������.
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
                Debug.LogError("�� ������� ������ ����������� ������ CoinAnimatorIcebreaker!");
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