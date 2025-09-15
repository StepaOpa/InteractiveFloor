using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;


public class PlayerControllerLabyrinth : MonoBehaviour
{
    // Структура для хранения кандидата и его "оценки" (насколько он подходит)
    private struct NeighborCandidate
    {
        public WaypointLabyrinth waypoint;
        public float dotProduct; // Оценка от -1 до 1
        public Vector3 direction; // Какому мировому направлению он соответствует
    }

    [Header("Настройки движения")]
    public int burstMoveLength = 3;
    public float moveDurationPerPoint = 0.3f;
    public float slowdownFactor = 1.8f;
    public WaypointLabyrinth startingWaypoint;

    [Header("UI и Паузы")]
    public Image forwardArrowImage;
    public Image backArrowImage;
    public Image leftArrowImage;
    public Image rightArrowImage;
    public Color enabledArrowColor = Color.white;
    public Color disabledArrowColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
    public float decisionPointDelay = 0.8f;
    [Tooltip("Насколько сильно путь должен совпадать с направлением стрелки, чтобы она загорелась (0.5 = 60 градусов)")]
    [Range(0.1f, 0.9f)]
    public float directionMatchThreshold = 0.5f;

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
        WaypointLabyrinth target = FindAssignedNeighborIn(worldDirection);
        if (target != null)
        {
            if (movementCoroutine != null) StopCoroutine(movementCoroutine);
            movementCoroutine = StartCoroutine(BurstMoveCoroutine(target));
        }
    }

    private void UpdateArrowVisuals()
    {
        SetArrowState(forwardArrowImage, false);
        SetArrowState(backArrowImage, false);
        SetArrowState(leftArrowImage, false);
        SetArrowState(rightArrowImage, false);

        if (currentWaypoint == null) return;

        var fwdCandidate = FindBestNeighborForDirection(Vector3.forward);
        var bckCandidate = FindBestNeighborForDirection(Vector3.back);
        var lftCandidate = FindBestNeighborForDirection(Vector3.left);
        var rgtCandidate = FindBestNeighborForDirection(Vector3.right);

        var allCandidates = new List<NeighborCandidate> { fwdCandidate, bckCandidate, lftCandidate, rgtCandidate };
        var assignedWaypoints = new HashSet<WaypointLabyrinth>();

        foreach (var candidate in allCandidates.OrderByDescending(c => c.dotProduct))
        {
            if (candidate.waypoint != null && candidate.dotProduct > directionMatchThreshold && !assignedWaypoints.Contains(candidate.waypoint))
            {
                AssignArrowToDirection(candidate.direction);
                assignedWaypoints.Add(candidate.waypoint);
            }
        }
    }

    private NeighborCandidate FindBestNeighborForDirection(Vector3 desiredDirection)
    {
        var bestCandidate = new NeighborCandidate { dotProduct = -2f, direction = desiredDirection };

        foreach (var neighbor in currentWaypoint.neighbors)
        {
            if (neighbor == null) continue;
            Vector3 neighborDirection = (neighbor.transform.position - currentWaypoint.transform.position).normalized;
            float dot = Vector3.Dot(desiredDirection, neighborDirection);

            if (dot > bestCandidate.dotProduct)
            {
                bestCandidate.dotProduct = dot;
                bestCandidate.waypoint = neighbor;
            }
        }
        return bestCandidate;
    }

    private void AssignArrowToDirection(Vector3 direction)
    {
        if (direction == Vector3.forward) SetArrowState(forwardArrowImage, true);
        else if (direction == Vector3.back) SetArrowState(backArrowImage, true);
        else if (direction == Vector3.left) SetArrowState(leftArrowImage, true);
        else if (direction == Vector3.right) SetArrowState(rightArrowImage, true);
    }

    private WaypointLabyrinth FindAssignedNeighborIn(Vector3 worldDirection)
    {
        var candidate = FindBestNeighborForDirection(worldDirection);
        if (candidate.dotProduct > directionMatchThreshold)
        {
            return candidate.waypoint;
        }
        return null;
    }

    private IEnumerator BurstMoveCoroutine(WaypointLabyrinth initialTarget)
    {
        isMoving = true;
        WaypointLabyrinth targetForNextStep = initialTarget;
        for (int stepsMade = 0; stepsMade < burstMoveLength; stepsMade++)
        {
            if (targetForNextStep == null) break;
            Vector3 startPosition = transform.position;
            Vector3 endPosition = targetForNextStep.transform.position;
            CurrentMovementDirection = (endPosition - startPosition).normalized;
            bool isNextPointDecision = targetForNextStep.Type != WaypointType.Standard;
            bool isLastPossibleStep = stepsMade == burstMoveLength - 1;
            float duration = (isNextPointDecision || isLastPossibleStep) ? moveDurationPerPoint * slowdownFactor : moveDurationPerPoint;
            float timeElapsed = 0;
            while (timeElapsed < duration)
            {
                transform.position = Vector3.Lerp(startPosition, endPosition, timeElapsed / duration);
                timeElapsed += Time.deltaTime;
                yield return null;
            }
            transform.position = endPosition;
            previousWaypoint = currentWaypoint;
            currentWaypoint = targetForNextStep;
            UpdateArrowVisuals();
            if (currentWaypoint.Type != WaypointType.Standard)
            {
                break;
            }
            targetForNextStep = currentWaypoint.neighbors.FirstOrDefault(n => n != previousWaypoint);
        }
        isMoving = false;
        if (currentWaypoint.Type != WaypointType.Standard)
        {
            yield return new WaitForSeconds(decisionPointDelay);
        }
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

        // <<< ИСПРАВЛЕНИЕ ЗДЕСЬ >>>
        // Меняем .Lose на .LoseGame, как в вашем скрипте GameManagerLabyrinth
        if (other.CompareTag("Trap")) gameManager.LoseGame("Вы попались в ловушку!");

        if (other.CompareTag("Net"))
        {
            gameManager.CatchOneFish(other.transform);
            other.enabled = false;
        }
    }
}