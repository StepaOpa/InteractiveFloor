using UnityEngine;
using System; // Необходимо для [Serializable]

// Этот атрибут автоматически добавит компонент AudioSource, если его нет
[RequireComponent(typeof(AudioSource))]
public class SoundManagerStones : MonoBehaviour
{
    // --- Синглтон (Singleton) ---
    // Это позволит нам обращаться к менеджеру из любого скрипта вот так: SoundManagerStones.instance
    public static SoundManagerStones instance;

    // --- Структура для хранения звуков ---
    // Мы создадим удобный список в инспекторе, где каждому звуку можно дать имя
    [Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
    }

    public Sound[] sounds; // Массив всех звуков в игре
    private AudioSource audioSource;

    // Awake вызывается раньше, чем Start
    void Awake()
    {
        // Настройка синглтона
        if (instance == null)
        {
            instance = this;
            // DontDestroyOnLoad(gameObject); // Раскомментируйте, если у вас будет несколько сцен
        }
        else
        {
            Destroy(gameObject); // Уничтожаем дубликат, если он есть
            return;
        }

        audioSource = GetComponent<AudioSource>();
    }

    // --- Главный метод для проигрывания звука ---
    public void PlaySound(string soundName)
    {
        // Ищем звук в нашем массиве по имени
        Sound soundToPlay = Array.Find(sounds, s => s.name == soundName);

        if (soundToPlay != null)
        {
            // PlayOneShot позволяет проигрывать короткие звуки, не прерывая друг друга
            audioSource.PlayOneShot(soundToPlay.clip);
        }
        else
        {
            // Если звук с таким именем не найден, выводим предупреждение
            Debug.LogWarning("Звук не найден: " + soundName);
        }
    }
}