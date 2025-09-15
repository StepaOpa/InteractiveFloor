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

    // НОВЫЙ МЕТОД: для сброса монеток
    public static void ResetCoins()
    {
        // Просто удаляем запись о монетах из памяти устройства.
        // При следующем вызове GetCoins(), если записи нет, он вернет 0.
        PlayerPrefs.DeleteKey(coinsKey);
        PlayerPrefs.Save();
        Debug.Log("Счетчик монет сброшен!"); // Сообщение для проверки в консоли
    }
}