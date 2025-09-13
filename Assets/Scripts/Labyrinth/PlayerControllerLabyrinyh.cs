using UnityEngine;
using System.Collections;
using System.Linq;
using UnityEngine.UI;

public class PlayerControllerLabyrinth : MonoBehaviour
{
    [Header("Настройки движения")]
    [Tooltip("Максимальное количество точек, которые рыбки проходят за один 'рывок'")]
    public int burstMoveLength = 3;
    [Tooltip("Время в секундах на прохождение одного отрезка пути")]
    public float moveDurationPerPoint = 0.3f;
    [Tooltip("Насколько медленнее проходится последний отрезок пути (1 = так же, 2 = в два раза медленнее)")]
    public float slowdownFactor = 1.8f;
    [Tooltip("Стартовая точка, с которой начинается игра")]
    public WaypointLabyrinth startingWaypoint;

    [Header("UI и Паузы")]
    public Image forwardArrowImage;
    public Image backArrowImage;
    public Image leftArrowImage;
    public Image rightArrowImage;
    public Color enabledArrowColor = Color.white;
    public Color disabledArrowColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
    [Tooltip("Задержка в секундах на перекрестках и в тупиках")]
    public float decisionPointDelay = 0.8f;

    // Ссылки и состояние
    private GameManagerLabyrinth gameManager;
    private WaypointLabyrinth currentWaypoint;
    private WaypointLabyrinth previousWaypoint;
    private bool isMoving = false;
    private Vector3 moveDirectionInput = Vector3.zero;
    public Vector3 CurrentMovementDirection { get; private set; } = Vector3.forward;
    private Coroutine movementCoroutine;
    public bool canMove = true;

    void Start()
    {
        gameManager = FindObjectOfType<GameManagerLabyrinth>();
        currentWaypoint = startingWaypoint;
        previousWaypoint = startingWaypoint;
        if (currentWaypoint != null)
        {
            transform.position = currentWaypoint.transform.position;
            UpdateArrowVisuals();
        }
    }

    void Update()
    {
        if (!canMove) return;

        ReadInput();
        if (!isMoving && moveDirectionInput != Vector3.zero)
        {
            AttemptMove(moveDirectionInput);
            moveDirectionInput = Vector3.zero;
        }
    }

    private void AttemptMove(Vector3 worldDirection)
    {
        WaypointLabyrinth target = FindNeighborInDirection(worldDirection);
        if (target != null)
        {
            if (movementCoroutine != null) StopCoroutine(movementCoroutine);
            movementCoroutine = StartCoroutine(BurstMoveCoroutine(target));
        }
    }

    private IEnumerator BurstMoveCoroutine(WaypointLabyrinth initialTarget)
    {
        isMoving = true;
        WaypointLabyrinth nextTargetInLine = initialTarget;

        for (int i = 0; i < burstMoveLength; i++)
        {
            if (nextTargetInLine == null) break;

            Vector3 startPosition = transform.position;
            Vector3 endPosition = nextTargetInLine.transform.position;

            if ((endPosition - startPosition).sqrMagnitude > 0.01f)
            {
                CurrentMovementDirection = (endPosition - startPosition).normalized;
            }

            bool isNextStepDecisionPoint = nextTargetInLine.Type != WaypointType.Standard;
            bool isLastStepOfBurst = i == burstMoveLength - 1;
            bool isDeadEnd = nextTargetInLine.neighbors.Count(n => n != currentWaypoint) == 0;
            bool shouldSlowDown = isNextStepDecisionPoint || isLastStepOfBurst || isDeadEnd;
            float currentDuration = shouldSlowDown ? moveDurationPerPoint * slowdownFactor : moveDurationPerPoint;

            float timeElapsed = 0;
            while (timeElapsed < currentDuration)
            {
                transform.position = Vector3.Lerp(startPosition, endPosition, timeElapsed / currentDuration);
                timeElapsed += Time.deltaTime;
                yield return null;
            }
            transform.position = endPosition;

            previousWaypoint = currentWaypoint;
            currentWaypoint = nextTargetInLine;

            UpdateArrowVisuals();

            // === ГЛАВНАЯ ЛОГИКА ОСТАНОВКИ ===
            // Если мы встали на точку, которая НЕ является стандартным прямым участком...
            if (currentWaypoint.Type != WaypointType.Standard)
            {
                // ...то немедленно прекращаем дальнейшее скольжение.
                break;
            }

            nextTargetInLine = currentWaypoint.neighbors.FirstOrDefault(n => n != previousWaypoint);
        }

        isMoving = false;

        // Если мы остановились на точке принятия решения, делаем паузу.
        if (currentWaypoint.Type != WaypointType.Standard)
        {
            yield return new WaitForSeconds(decisionPointDelay);
        }
    }

    private void UpdateArrowVisuals()
    {
        SetArrowState(forwardArrowImage, FindNeighborInDirection(Vector3.forward) != null);
        SetArrowState(backArrowImage, FindNeighborInDirection(Vector3.back) != null);
        SetArrowState(leftArrowImage, FindNeighborInDirection(Vector3.left) != null);
        SetArrowState(rightArrowImage, FindNeighborInDirection(Vector3.right) != null);
    }

    private WaypointLabyrinth FindNeighborInDirection(Vector3 desiredDirection)
    {
        if (currentWaypoint == null) return null;
        WaypointLabyrinth bestMatch = null;
        float bestDot = 0.7f;

        foreach (var neighbor in currentWaypoint.neighbors)
        {
            Vector3 neighborDirection = (neighbor.transform.position - currentWaypoint.transform.position).normalized;
            float dot = Vector3.Dot(desiredDirection, neighborDirection);
            if (dot > bestDot)
            {
                bestDot = dot;
                bestMatch = neighbor;
            }
        }
        return bestMatch;
    }

    private void SetArrowState(Image arrow, bool isActive)
    {
        if (arrow != null) arrow.color = isActive ? enabledArrowColor : disabledArrowColor;
    }

    private void ReadInput()
    {
        if (Input.GetKeyDown(KeyCode.W)) moveDirectionInput = Vector3.forward;
        if (Input.GetKeyDown(KeyCode.S)) moveDirectionInput = Vector3.back;
        if (Input.GetKeyDown(KeyCode.A)) moveDirectionInput = Vector3.left;
        if (Input.GetKeyDown(KeyCode.D)) moveDirectionInput = Vector3.right;
    }

    public void OnPointerDownForward() { moveDirectionInput = Vector3.forward; }
    public void OnPointerDownBack() { moveDirectionInput = Vector3.back; }
    public void OnPointerDownLeft() { moveDirectionInput = Vector3.left; }
    public void OnPointerDownRight() { moveDirectionInput = Vector3.right; }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("WinZone")) gameManager.WinGame();
        if (other.CompareTag("Trap")) gameManager.LoseGame("Вы попались в ловушку!");
        if (other.CompareTag("Net"))
        {
            gameManager.CatchOneFish(other.transform);
            other.enabled = false;
        }
    }
}