using UnityEngine;
using System.Collections; // Добавлено для корутин

// Требуем, чтобы на этом объекте всегда был компонент AudioSource
[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    // Singleton для легкого доступа из любого другого скрипта
    public static SoundManager Instance { get; private set; }

    // Ссылки на все ваши аудиофайлы. Вы перетащите их в инспекторе.
    [Header("Звуки событий")]
    [SerializeField] private AudioClip newLevelSound;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip loseSound;

    [Header("Звуки геймплея")]
    [SerializeField] private AudioClip digTapSound;       // Звук тапа (копания)
    [SerializeField] private AudioClip itemRevealSound;   // Звук появления предмета
    [SerializeField] private AudioClip timerTickSound;    // Звук тиканья таймера

    [Header("Звуки предметов")]
    [SerializeField] private AudioClip pickupItemSound;      // Поднятие для осмотра
    [SerializeField] private AudioClip inventoryAddSound;    // Успешно добавлен в инвентарь
    [SerializeField] private AudioClip itemDropSound;        // Выбросить/вернуть на место
    [SerializeField] private AudioClip errorSound;           // Попытка взять мусор

    private AudioSource audioSource;
    private bool isTicking = false; // Флаг, чтобы звук тиканья не запускался много раз

    void Awake()
    {
        // Настройка Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Делаем менеджер постоянным между сценами
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Получаем доступ к компоненту AudioSource
        audioSource = GetComponent<AudioSource>();
    }

    // Далее идут публичные методы, которые будут вызывать другие скрипты

    public void PlayNewLevelSound()
    {
        if (newLevelSound != null)
        {
            audioSource.PlayOneShot(newLevelSound);
        }
    }

    public void PlayWinSound()
    {
        if (winSound != null)
        {
            audioSource.PlayOneShot(winSound);
        }
    }

    public void PlayLoseSound()
    {
        if (loseSound != null)
        {
            audioSource.PlayOneShot(loseSound);
        }
    }

    public void PlayPickupItemSound()
    {
        if (pickupItemSound != null)
        {
            audioSource.PlayOneShot(pickupItemSound);
        }
    }

    public void PlayInventoryAddSound()
    {
        if (inventoryAddSound != null)
        {
            audioSource.PlayOneShot(inventoryAddSound);
        }
    }

    public void PlayItemDropSound()
    {
        if (itemDropSound != null)
        {
            audioSource.PlayOneShot(itemDropSound);
        }
    }

    public void PlayErrorSound()
    {
        if (errorSound != null)
        {
            audioSource.PlayOneShot(errorSound);
        }
    }
    
    // --- НОВЫЕ МЕТОДЫ ---
    
    public void PlayDigSound()
    {
        if (digTapSound != null)
        {
            audioSource.PlayOneShot(digTapSound);
        }
    }

    public void PlayItemRevealSound()
    {
        if (itemRevealSound != null)
        {
            audioSource.PlayOneShot(itemRevealSound);
        }
    }

    // Метод для тиканья, который проигрывает звук в цикле
    public void StartTickingSound()
    {
        if (timerTickSound != null && !isTicking)
        {
            isTicking = true;
            // Запускаем корутину, чтобы проигрывать звук каждую секунду
            StartCoroutine(TickingCoroutine());
        }
    }

    public void StopTickingSound()
    {
        isTicking = false;
        // Остановка корутины не нужна, она сама остановится благодаря флагу
    }

    private IEnumerator TickingCoroutine()
    {
        while (isTicking)
        {
            audioSource.PlayOneShot(timerTickSound);
            yield return new WaitForSecondsRealtime(1f); // Используем Realtime, чтобы звук работал даже на паузе
        }
    }
}