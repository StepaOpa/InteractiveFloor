// PlayerControllerLabyrinth.cs
using UnityEngine;
using System.Collections;
using System.Linq; // Нужно для удобного поиска

public class PlayerControllerLabyrinth : MonoBehaviour
{
    [Header("Настройки движения")]
    public float moveSpeed = 5.0f;
    public WaypointLabyrinth startingWaypoint;

    private GameManagerLabyrinth gameManager;
    public bool canMove = true;
    private WaypointLabyrinth currentWaypoint;
    private bool isMoving = false;
    private Vector3 moveDirection = Vector3.zero;
    public Vector3 LastInputDirection { get; private set; } = Vector3.forward;

    void Start()
    {
        gameManager = FindObjectOfType<GameManagerLabyrinth>();
        currentWaypoint = startingWaypoint;
        if (currentWaypoint != null)
        {
            transform.position = currentWaypoint.transform.position;
        }
        else
        {
            Debug.LogError("Стартовая точка не назначена!", this.gameObject);
            canMove = false;
        }
    }

    void Update()
    {
        if (!canMove || isMoving) return;
        Vector3 inputDirection = GetInputDirection();
        if (inputDirection != Vector3.zero)
        {
            AttemptMove(inputDirection);
        }
    }

    private void AttemptMove(Vector3 worldInputDirection)
    {
        if (currentWaypoint.neighbors.Count == 0) return; // Некуда идти

        // --- НОВАЯ СУПЕР-ЛОГИКА ---
        // Мы ищем соседа, который находится в направлении, наиболее близком к нашему вводу.

        WaypointLabyrinth bestTarget = null;
        float bestDot = -1; // Начинаем с наихудшего значения

        foreach (var neighbor in currentWaypoint.neighbors)
        {
            // Находим вектор направления от нас к соседу
            Vector3 directionToNeighbor = (neighbor.transform.position - transform.position).normalized;

            // Вычисляем, насколько это направление "похоже" на то, что нажал игрок
            float dot = Vector3.Dot(worldInputDirection, directionToNeighbor);

            // Если это направление "похоже" больше, чем все предыдущие, запоминаем его
            if (dot > bestDot)
            {
                bestDot = dot;
                bestTarget = neighbor;
            }
        }

        // Если мы нашли подходящую цель (dot > 0.7 означает, что угол меньше ~45 градусов)
        if (bestTarget != null && bestDot > 0.7f)
        {
            LastInputDirection = worldInputDirection;
            StartCoroutine(MoveToTarget(bestTarget));
        }
    }

    // Остальная часть скрипта остается практически без изменений
    private IEnumerator MoveToTarget(WaypointLabyrinth destination)
    {
        isMoving = true;
        Vector3 startPosition = transform.position;
        Vector3 endPosition = destination.transform.position;
        float journeyLength = Vector3.Distance(startPosition, endPosition);
        if (journeyLength <= 0) { isMoving = false; yield break; }
        float startTime = Time.time;

        while (transform.position != endPosition)
        {
            float distCovered = (Time.time - startTime) * moveSpeed;
            float fractionOfJourney = distCovered / journeyLength;
            transform.position = Vector3.Lerp(startPosition, endPosition, fractionOfJourney);
            yield return null;
        }
        currentWaypoint = destination;
        isMoving = false;
    }

    private Vector3 GetInputDirection()
    {
        if (Input.GetKeyDown(KeyCode.W)) return Vector3.forward;
        if (Input.GetKeyDown(KeyCode.S)) return Vector3.back;
        if (Input.GetKeyDown(KeyCode.A)) return Vector3.left;
        if (Input.GetKeyDown(KeyCode.D)) return Vector3.right;

        if (moveDirection.magnitude > 0)
        {
            Vector3 direction = moveDirection;
            moveDirection = Vector3.zero;
            return direction;
        }
        return Vector3.zero;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("WinZone")) { gameManager.WinGame(); }
        if (other.CompareTag("Trap")) { gameManager.LoseGame("Вы попались в ловушку!"); }
        if (other.CompareTag("Net"))
        {
            gameManager.CatchOneFish(other.transform);
            other.enabled = false;
        }
    }

    public void OnPointerDownForward() { moveDirection = Vector3.forward; }
    public void OnPointerDownBack() { moveDirection = Vector3.back; }
    public void OnPointerDownLeft() { moveDirection = Vector3.left; }
    public void OnPointerDownRight() { moveDirection = Vector3.right; }
}