using UnityEngine;
using UnityEngine.UI; // Обязательно добавь эту строку для работы с UI
using System.Collections.Generic; // Для работы со списками


public class GameManagerPetroglyths : MonoBehaviour // <-- Имя класса изменено
{
    [Header("UI Elements")]
    [SerializeField] private Image petroglyphToFindImage; // Сюда перетащим нашу иконку

    [Header("Game Settings")]
    [SerializeField] private List<Sprite> allPetroglyphSprites; // Список всех возможных спрайтов петроглифов

    private List<Sprite> availablePetroglyphs; // Список петроглифов, которые еще не были найдены
    private Sprite currentPetroglyph; // Текущий петроглиф, который нужно найти

    void Start()
    {
        // Копируем все спрайты в список доступных для поиска
        availablePetroglyphs = new List<Sprite>(allPetroglyphSprites);
        // Выбираем следующий петроглиф для поиска
        SelectNextPetroglyph();
    }

    public void SelectNextPetroglyph()
    {
        if (availablePetroglyphs.Count > 0)
        {
            // Выбираем случайный индекс из списка доступных петроглифов
            int randomIndex = Random.Range(0, availablePetroglyphs.Count);
            currentPetroglyph = availablePetroglyphs[randomIndex];

            // Устанавливаем спрайт в наш UI элемент
            petroglyphToFindImage.sprite = currentPetroglyph;

            // Удаляем выбранный петроглиф из списка, чтобы он не повторялся
            availablePetroglyphs.RemoveAt(randomIndex);

            Debug.Log("Нужно найти: " + currentPetroglyph.name);
        }
        else
        {
            // Все петроглифы найдены!
            Debug.Log("Поздравляю! Вы нашли все петроглифы!");
            // Здесь позже будет логика победы в игре
        }
    }
}