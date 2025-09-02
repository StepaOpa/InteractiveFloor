using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;


public class GameManagerPetroglyphs : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Image petroglyphToFindImage;

    [Header("Game Settings")]
    [SerializeField] private List<Sprite> allPetroglyphSprites;

    private List<Sprite> availablePetroglyphs;
    private Sprite currentPetroglyph;

    void Start()
    {
        availablePetroglyphs = new List<Sprite>(allPetroglyphSprites);
        SelectNextPetroglyph();
    }

    public void SelectNextPetroglyph()
    {
        if (availablePetroglyphs.Count > 0)
        {
            int randomIndex = Random.Range(0, availablePetroglyphs.Count);
            currentPetroglyph = availablePetroglyphs[randomIndex];
            petroglyphToFindImage.sprite = currentPetroglyph;
            availablePetroglyphs.RemoveAt(randomIndex);
            Debug.Log("Нужно найти: " + currentPetroglyph.name);
        }
        else
        {
            Debug.Log("Поздравляю! Вы нашли все петроглифы!");
            // Здесь позже будет логика победы в игре
        }
    }

    // --- НОВЫЙ МЕТОД ---
    // Этот метод проверяет, правильный ли петроглиф был найден
    public void CheckFoundPetroglyph(PetroglyphLocation foundLocation)
    {
        if (foundLocation.petroglyphSprite == currentPetroglyph)
        {
            Debug.Log("ПРАВИЛЬНО! Найден петроглиф: " + currentPetroglyph.name);

            // Пока что мы просто выбираем следующий петроглиф.
            // Эффект перелетания добавим позже.
            SelectNextPetroglyph();
        }
        else
        {
            Debug.Log("Неверно. Это другой рисунок.");
        }
    }
}