using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(PlayerPickup))]
public class AIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PickupItem itItem;
    [SerializeField] private Transform player;
    [SerializeField] private Transform holdPoint;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float fleeDistance = 10f;

    [Header("Throw")]
    [SerializeField] private float throwRange = 8f;
    [SerializeField] private float minThrowRange = 2f;
    [SerializeField] private float throwAccuracy = 0.9f;
    [SerializeField] private float aimHeight = 1f;
    [SerializeField] private float throwCooldown = 1.5f;
    [SerializeField] private float aimTime = 0.4f;

    [Header("Pickup")]
    [SerializeField] private float pickupReachDistance = 1.8f;
    [SerializeField] private float reactionTime = 0.15f;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private NavMeshAgent agent;
    private PlayerPickup pickup;
    private PlayerPickup playerPickup;   // Reference to the human player's PlayerPickup
    private float throwTimer;
    private float reactionTimer;
    private float aimTimer;

    // Tracks whether the AI was the last one to throw the ball
    private bool aiThrewLast = false;

    private enum State { Idle, ChaseBall, FleeFromBall, ChasePlayer, AimAndThrow }
    private State currentState = State.Idle;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        pickup = GetComponent<PlayerPickup>();
        agent.speed = moveSpeed;
    }

    private void Start()
    {
        if (itItem == null) itItem = FindObjectOfType<PickupItem>();

        if (player == null)
        {
            PlayerPickup[] allPlayers = FindObjectsOfType<PlayerPickup>();
            foreach (var p in allPlayers)
            {
                if (p != pickup) { player = p.transform; playerPickup = p; break; }
            }
        }
        else
        {
            playerPickup = player.GetComponent<PlayerPickup>();
        }
    }

    private void Update()
    {
        if (itItem == null || player == null) return;

        throwTimer -= Time.deltaTime;
        reactionTimer -= Time.deltaTime;

        DecideState();
        ExecuteState();
    }

    // ---------- DECISION ----------

    private void DecideState()
    {
        bool aiHasBall = pickup.IsHoldingItem();
        bool playerHasBall = playerPickup != null && playerPickup.IsHoldingItem();
        bool ballIsLoose = !itItem.IsHeld();

        if (aiHasBall)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer <= throwRange && distanceToPlayer >= minThrowRange && throwTimer <= 0f)
                currentState = State.AimAndThrow;
            else
                currentState = State.ChasePlayer;
        }
        else if (playerHasBall)
        {
            // Player is holding the ball → AI runs away
            currentState = State.FleeFromBall;
            aiThrewLast = false; // player has it, so it wasn't the AI's throw
        }
        else if (ballIsLoose)
        {
            // Ball is on the ground. Was it the AI's throw?
            if (aiThrewLast)
                currentState = State.ChaseBall;   // AI missed → go get it back
            else
                currentState = State.FleeFromBall; // Player missed → keep running away
        }
        else
        {
            currentState = State.Idle;
        }
    }

    // ---------- EXECUTION ----------

    private void ExecuteState()
    {
        switch (currentState)
        {
            case State.ChaseBall: ChaseBall(); break;
            case State.FleeFromBall: FleeFromBall(); break;
            case State.ChasePlayer: ChasePlayer(); break;
            case State.AimAndThrow: DoAimAndThrow(); break;
        }
    }

    private void ChaseBall()
    {
        Vector3 targetPos = itItem.transform.position;
        agent.isStopped = false;
        agent.SetDestination(targetPos);

        float dist = Vector3.Distance(transform.position, targetPos);
        if (dist <= pickupReachDistance && itItem.CanBePickedUp() && !pickup.IsHoldingItem())
        {
            if (reactionTimer <= 0f)
            {
                pickup.PickUp(itItem);
                reactionTimer = reactionTime;
                aiThrewLast = false; // AI picked it back up, no longer a "missed throw"
                if (debugLogs) Debug.Log("[AI] Picked up the ball.");
            }
        }
    }

    private void FleeFromBall()
    {
        // Flee away from the ball's position (which is on the player if they're holding it,
        // or on the ground if they just threw and missed)
        Vector3 ballPos = itItem.transform.position;
        Vector3 awayDir = (transform.position - ballPos);
        awayDir.y = 0f;

        if (awayDir.sqrMagnitude < 0.001f)
            awayDir = new Vector3(Random.Range(-1f, 1f), 0, Random.Range(-1f, 1f));

        awayDir.Normalize();
        Vector3 desiredPos = transform.position + awayDir * fleeDistance;

        if (NavMesh.SamplePosition(desiredPos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            agent.isStopped = false;
            agent.SetDestination(hit.position);
        }
    }

    private void ChasePlayer()
    {
        agent.isStopped = false;
        agent.SetDestination(player.position);
    }

    private void DoAimAndThrow()
    {
        agent.isStopped = true;

        Vector3 lookDir = (player.position - transform.position);
        lookDir.y = 0;
        if (lookDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
        }

        aimTimer += Time.deltaTime;

        float angleToPlayer = Vector3.Angle(transform.forward, lookDir.normalized);
        if (aimTimer >= aimTime && angleToPlayer < 15f)
        {
            FireThrow();
        }
    }

    private void FireThrow()
    {
        float distance = Vector3.Distance(transform.position, player.position);

        if (debugLogs) Debug.Log($"[AI] THROWING at player. Distance: {distance:F2}");

        // Mark that the AI just threw — if it misses, it will chase the ball
        aiThrewLast = true;

        pickup.ThrowAt(CalculateForceForDistance(distance));

        throwTimer = throwCooldown;
        aimTimer = 0f;

        agent.isStopped = false;
    }

    // ---------- HELPERS ----------

    private float CalculateForceForDistance(float distance)
    {
        return Mathf.Clamp(distance * 1.6f, 6f, 18f);
    }
}