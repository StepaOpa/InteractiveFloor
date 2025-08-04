using System.Collections.Generic;
using UnityEngine;

public class InteractiveEffectsController : MonoBehaviour
{
    [Header("Effect Settings")]
    [Tooltip("Префаб эффекта для отображения в местах обнаружения")]
    public GameObject effectPrefab;
    [Tooltip("Размер эффекта в мировых координатах")]
    public float effectSize = 0.5f;
    [Tooltip("Время жизни эффекта после исчезновения объекта")]
    public float effectLifetime = 2.0f;
    [Tooltip("Максимальное количество одновременных эффектов")]
    public int maxEffects = 20;
    
    [Header("Interaction Zones")]
    [Tooltip("Радиус зоны взаимодействия вокруг обнаруженного объекта")]
    public float interactionRadius = 0.3f;
    [Tooltip("Минимальное время присутствия объекта для активации эффекта")]
    public float activationTime = 0.5f;
    
    [Header("Visual Feedback")]
    public Color activeColor = Color.cyan;
    public Color inactiveColor = Color.white;
    [Tooltip("Скорость анимации эффектов")]
    public float animationSpeed = 2.0f;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip interactionSound;
    [Range(0f, 1f)]
    public float soundVolume = 0.5f;
    
    // Ссылка на контроллер пола
    private InteractiveFloorController floorController;
    
    // Активные эффекты
    private List<InteractionEffect> activeEffects = new List<InteractionEffect>();
    
    // Пул объектов для эффектов
    private Queue<GameObject> effectPool = new Queue<GameObject>();
    
    // Класс для управления отдельным эффектом
    private class InteractionEffect
    {
        public GameObject effectObject;
        public Vector3 worldPosition;
        public float activationTimer;
        public float lifetimeTimer;
        public bool isActivated;
        public bool hasObject;
        public Renderer effectRenderer;
        public Transform effectTransform;
        
        public InteractionEffect(GameObject obj)
        {
            effectObject = obj;
            effectRenderer = obj.GetComponent<Renderer>();
            effectTransform = obj.transform;
            activationTimer = 0f;
            lifetimeTimer = 0f;
            isActivated = false;
            hasObject = false;
        }
    }
    
    void Start()
    {
        // Находим контроллер пола
        floorController = FindObjectOfType<InteractiveFloorController>();
        if (floorController == null)
        {
            Debug.LogError("InteractiveFloorController не найден!");
            enabled = false;
            return;
        }
        
        // Подписываемся на события обнаружения объектов
        floorController.OnObjectsDetected += OnObjectsDetected;
        floorController.OnCalibrationComplete += OnCalibrationComplete;
        
        // Создаем пул эффектов
        CreateEffectPool();
        
        // Настраиваем аудио
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        audioSource.volume = soundVolume;
        audioSource.playOnAwake = false;
        
        Debug.Log("InteractiveEffectsController инициализирован");
    }
    
    void OnDestroy()
    {
        if (floorController != null)
        {
            floorController.OnObjectsDetected -= OnObjectsDetected;
            floorController.OnCalibrationComplete -= OnCalibrationComplete;
        }
    }
    
    void CreateEffectPool()
    {
        for (int i = 0; i < maxEffects; i++)
        {
            GameObject effect = CreateEffectObject();
            effect.SetActive(false);
            effectPool.Enqueue(effect);
        }
    }
    
    GameObject CreateEffectObject()
    {
        GameObject effect;
        
        if (effectPrefab != null)
        {
            effect = Instantiate(effectPrefab, transform);
        }
        else
        {
            // Создаем простой эффект по умолчанию
            effect = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            effect.transform.SetParent(transform);
            
            // Убираем коллайдер
            Collider col = effect.GetComponent<Collider>();
            if (col != null) Destroy(col);
            
            // Настраиваем материал
            Renderer renderer = effect.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = new Material(Shader.Find("Standard"));
                renderer.material.color = inactiveColor;
                renderer.material.SetFloat("_Metallic", 0.5f);
                renderer.material.SetFloat("_Smoothness", 0.8f);
            }
        }
        
        // Устанавливаем размер
        effect.transform.localScale = Vector3.one * effectSize;
        
        return effect;
    }
    
    void OnCalibrationComplete()
    {
        Debug.Log("Система эффектов готова к работе");
    }
    
    void OnObjectsDetected(List<InteractiveFloorController.DetectedObject> detectedObjects)
    {
        // Отмечаем все эффекты как не имеющие объекта
        foreach (var effect in activeEffects)
        {
            effect.hasObject = false;
        }
        
        // Обрабатываем каждый обнаруженный объект
        foreach (var obj in detectedObjects)
        {
            ProcessDetectedObject(obj);
        }
        
        // Обновляем эффекты без объектов
        UpdateEffectsWithoutObjects();
    }
    
    void ProcessDetectedObject(InteractiveFloorController.DetectedObject detectedObject)
    {
        // Используем уже преобразованные мировые координаты
        Vector3 worldPos = new Vector3(
            detectedObject.worldPos.x,
            0.05f, // немного приподнимаем над полом
            detectedObject.worldPos.y
        );
        
        // Ищем существующий эффект рядом с этой позицией
        InteractionEffect nearbyEffect = FindNearbyEffect(worldPos);
        
        if (nearbyEffect != null)
        {
            // Обновляем существующий эффект
            nearbyEffect.hasObject = true;
            nearbyEffect.worldPosition = worldPos;
            nearbyEffect.activationTimer += Time.deltaTime;
            
            // Проверяем активацию
            if (!nearbyEffect.isActivated && nearbyEffect.activationTimer >= activationTime)
            {
                ActivateEffect(nearbyEffect);
            }
        }
        else
        {
            // Создаем новый эффект
            CreateNewEffect(worldPos);
        }
    }
    
    InteractionEffect FindNearbyEffect(Vector3 position)
    {
        foreach (var effect in activeEffects)
        {
            float distance = Vector3.Distance(effect.worldPosition, position);
            if (distance <= interactionRadius)
            {
                return effect;
            }
        }
        return null;
    }
    
    void CreateNewEffect(Vector3 position)
    {
        if (effectPool.Count > 0)
        {
            GameObject effectObj = effectPool.Dequeue();
            effectObj.SetActive(true);
            effectObj.transform.position = position;
            
            InteractionEffect newEffect = new InteractionEffect(effectObj);
            newEffect.worldPosition = position;
            newEffect.hasObject = true;
            
            activeEffects.Add(newEffect);
        }
    }
    
    void ActivateEffect(InteractionEffect effect)
    {
        effect.isActivated = true;
        
        // Меняем цвет на активный
        if (effect.effectRenderer != null)
        {
            effect.effectRenderer.material.color = activeColor;
        }
        
        // Воспроизводим звук
        PlayInteractionSound();
        
        Debug.Log("Эффект активирован!");
    }
    
    void UpdateEffectsWithoutObjects()
    {
        List<InteractionEffect> effectsToRemove = new List<InteractionEffect>();
        
        foreach (var effect in activeEffects)
        {
            if (!effect.hasObject)
            {
                effect.lifetimeTimer += Time.deltaTime;
                
                // Постепенно делаем эффект менее заметным
                if (effect.effectRenderer != null)
                {
                    Color currentColor = effect.effectRenderer.material.color;
                    currentColor.a = Mathf.Lerp(1f, 0f, effect.lifetimeTimer / effectLifetime);
                    effect.effectRenderer.material.color = currentColor;
                }
                
                if (effect.lifetimeTimer >= effectLifetime)
                {
                    effectsToRemove.Add(effect);
                }
            }
            else
            {
                // Сбрасываем таймер жизни если объект есть
                effect.lifetimeTimer = 0f;
                
                // Возвращаем полную непрозрачность
                if (effect.effectRenderer != null)
                {
                    Color currentColor = effect.effectRenderer.material.color;
                    currentColor.a = 1f;
                    effect.effectRenderer.material.color = currentColor;
                }
            }
        }
        
        // Удаляем устаревшие эффекты
        foreach (var effect in effectsToRemove)
        {
            RemoveEffect(effect);
        }
    }
    
    void RemoveEffect(InteractionEffect effect)
    {
        activeEffects.Remove(effect);
        effect.effectObject.SetActive(false);
        
        // Сбрасываем состояние эффекта
        effect.activationTimer = 0f;
        effect.lifetimeTimer = 0f;
        effect.isActivated = false;
        effect.hasObject = false;
        
        if (effect.effectRenderer != null)
        {
            effect.effectRenderer.material.color = inactiveColor;
        }
        
        effectPool.Enqueue(effect.effectObject);
    }
    
    void PlayInteractionSound()
    {
        if (audioSource != null && interactionSound != null)
        {
            audioSource.PlayOneShot(interactionSound, soundVolume);
        }
    }
    
    void Update()
    {
        // Анимируем активные эффекты
        foreach (var effect in activeEffects)
        {
            if (effect.effectTransform != null)
            {
                // Простая анимация вращения
                effect.effectTransform.Rotate(0, animationSpeed * Time.deltaTime * 50f, 0);
                
                // Плавное изменение масштаба для активированных эффектов
                if (effect.isActivated)
                {
                    float scale = effectSize * (1f + 0.2f * Mathf.Sin(Time.time * animationSpeed * 3f));
                    effect.effectTransform.localScale = Vector3.one * scale;
                }
            }
        }
    }
    
    // Публичные методы для настройки
    public void SetEffectPrefab(GameObject prefab)
    {
        effectPrefab = prefab;
    }
    
    public void SetInteractionRadius(float radius)
    {
        interactionRadius = radius;
    }
    
    public void SetActivationTime(float time)
    {
        activationTime = time;
    }
    
    public int GetActiveEffectsCount()
    {
        return activeEffects.Count;
    }
    
    public void ClearAllEffects()
    {
        List<InteractionEffect> allEffects = new List<InteractionEffect>(activeEffects);
        foreach (var effect in allEffects)
        {
            RemoveEffect(effect);
        }
    }
    
    // Debug информация
    void OnGUI()
    {
        if (floorController != null && floorController.showDebugInfo)
        {
            GUILayout.BeginArea(new Rect(10, 220, 300, 100));
            GUILayout.Label($"Активных эффектов: {activeEffects.Count}");
            GUILayout.Label($"Эффектов в пуле: {effectPool.Count}");
            
            if (GUILayout.Button("Очистить все эффекты"))
            {
                ClearAllEffects();
            }
            
            GUILayout.EndArea();
        }
    }
}
