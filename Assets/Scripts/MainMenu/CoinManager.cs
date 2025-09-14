using UnityEngine;

public static class CoinManager
{
    private static readonly string coinsKey = "PlayerTotalCoins";

    public static void AddCoins(int amount)
    {
        int currentCoins = GetCoins();
        int newTotal = currentCoins + amount;
        PlayerPrefs.SetInt(coinsKey, newTotal);
        PlayerPrefs.Save();
    }

    public static int GetCoins()
    {
        return PlayerPrefs.GetInt(coinsKey, 0);
    }
}