using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Звуки событий")]
    [SerializeField] private AudioClip newLevelSound;
    [SerializeField] private AudioClip winSound;
    [SerializeField] private AudioClip loseSound;

    [Header("Звуки геймплея")]
    [SerializeField] private AudioClip digTapSound;
    [SerializeField] private AudioClip itemRevealSound;
    [SerializeField] private AudioClip timerTickSound;
    [SerializeField] private AudioClip brushSwipeSound; // <-- НОВОЕ ПОЛЕ ДЛЯ ЗВУКА КИСТИ

    [Header("Звуки предметов")]
    [SerializeField] private AudioClip pickupItemSound;
    [SerializeField] private AudioClip inventoryAddSound;
    [SerializeField] private AudioClip itemDropSound;
    [SerializeField] private AudioClip errorSound;

    private AudioSource audioSource;
    private Coroutine tickingCoroutine = null;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); return; }
        audioSource = GetComponent<AudioSource>();
        audioSource.ignoreListenerPause = true;
    }

    void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) { StopAllSounds(); }

    public void StopAllSounds()
    {
        if (tickingCoroutine != null) { StopCoroutine(tickingCoroutine); tickingCoroutine = null; }
        audioSource.Stop();
    }

    public void StartTickingSound() { if (tickingCoroutine == null && timerTickSound != null) { tickingCoroutine = StartCoroutine(TickingCoroutine()); } }
    public void StopTickingSound() { if (tickingCoroutine != null) { StopCoroutine(tickingCoroutine); tickingCoroutine = null; } }
    private IEnumerator TickingCoroutine() { while (true) { audioSource.PlayOneShot(timerTickSound); yield return new WaitForSecondsRealtime(1f); } }
    
    // --- НОВЫЙ МЕТОД ---
    public void PlayBrushSwipeSound()
    {
        if (brushSwipeSound != null)
        {
            audioSource.PlayOneShot(brushSwipeSound);
        }
    }
    // -------------------

    public void PlayNewLevelSound() { if (newLevelSound != null) audioSource.PlayOneShot(newLevelSound); }
    public void PlayWinSound() { if (winSound != null) audioSource.PlayOneShot(winSound); }
    public void PlayLoseSound() { if (loseSound != null) audioSource.PlayOneShot(loseSound); }
    public void PlayPickupItemSound() { if (pickupItemSound != null) audioSource.PlayOneShot(pickupItemSound); }
    public void PlayInventoryAddSound() { if (inventoryAddSound != null) audioSource.PlayOneShot(inventoryAddSound); }
    public void PlayItemDropSound() { if (itemDropSound != null) audioSource.PlayOneShot(itemDropSound); }
    public void PlayErrorSound() { if (errorSound != null) audioSource.PlayOneShot(errorSound); }
    public void PlayDigSound() { if (digTapSound != null) audioSource.PlayOneShot(digTapSound); }
    public void PlayItemRevealSound() { if (itemRevealSound != null) audioSource.PlayOneShot(itemRevealSound); }
}