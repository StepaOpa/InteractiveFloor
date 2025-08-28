using UnityEngine;

public class ChunkObstacleSpawner : MonoBehaviour
{
    [SerializeField] private GameObject[] obstaclePrefabs;
    [SerializeField] private int obstaclesCount = 5;
    [SerializeField] private Vector2 uniformScaleRange = new Vector2(0.8f, 1.5f);
    [SerializeField] private float borderPadding = 0.5f;
    [SerializeField] private bool randomYRotation = true;

    // Область для размещения препятствий
    [SerializeField] private Collider areaCollider;   // предпочтительно указывать этот коллайдер плоскости
    [SerializeField] private Renderer areaRenderer;   // либо указывать рендерер плоскости
    [SerializeField] private Transform obstaclesParent; // родитель для спавнимых объектов (по умолчанию сам чанк)

    private void Reset()
    {
        // Попробуем автоматически найти область на этом чанке
        if (areaCollider == null) areaCollider = GetComponentInChildren<Collider>();
        if (areaRenderer == null) areaRenderer = GetComponentInChildren<MeshRenderer>();
        if (obstaclesParent == null) obstaclesParent = transform;
    }

    public GameObject[] SpawnObstacles(Transform parent)
    {
        if (obstaclePrefabs == null || obstaclePrefabs.Length == 0 || obstaclesCount <= 0)
            return null;

        Bounds bounds = GetAreaBounds();
        GameObject[] obstacles = new GameObject[obstaclesCount];
        for (int i = 0; i < obstaclesCount; i++)
        {
            Vector3 pos = GetRandomPointOnArea(bounds);
            pos = ProjectToSurfaceIfPossible(pos);

            GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
            obstacles[i] = Instantiate(prefab, pos, Quaternion.identity, parent);

            // Случайный масштаб (равномерный)
            float s = Random.Range(uniformScaleRange.x, uniformScaleRange.y);
            obstacles[i].transform.localScale *= s;

            // Случайный поворот по Y (необязательно)
            if (randomYRotation)
            {
                Vector3 e = obstacles[i].transform.eulerAngles;
                e.y = Random.Range(0f, 360f);
                obstacles[i].transform.eulerAngles = e;
            }
        }
        return obstacles;
    }

    private Bounds GetAreaBounds()
    {
        if (areaCollider != null) return areaCollider.bounds;
        if (areaRenderer != null) return areaRenderer.bounds;
        // Фолбэк: пытаемся взять рендерер на корне
        var r = GetComponent<Renderer>();
        return r != null ? r.bounds : new Bounds(transform.position, new Vector3(10f, 0.1f, 10f));
    }

    private Vector3 GetRandomPointOnArea(Bounds b)
    {
        float x = Random.Range(b.min.x + borderPadding, b.max.x - borderPadding);
        float z = Random.Range(b.min.z + borderPadding, b.max.z - borderPadding);
        return new Vector3(x, b.center.y, z);
    }

    private Vector3 ProjectToSurfaceIfPossible(Vector3 worldPos)
    {
        if (areaCollider == null)
            return worldPos;

        // Бросаем луч сверху вниз, чтобы поставить на поверхность плоскости
        Vector3 rayOrigin = worldPos + Vector3.up * 10f;
        if (areaCollider.Raycast(new Ray(rayOrigin, Vector3.down), out RaycastHit hit, 50f))
        {
            worldPos = hit.point;
        }
        return worldPos;
    }
}


