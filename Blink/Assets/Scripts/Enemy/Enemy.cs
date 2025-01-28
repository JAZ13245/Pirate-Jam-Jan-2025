using System.Collections;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public NavMeshAgent agent { get; private set; }
    public Player player { get; private set; }
    public PlayerCharacter playerBody { get; private set; }
    public EnemyManager enemyManager { get; private set; }
    [SerializeField] private Gun gun;

    public StateMachine stateMachine { get; set; }
    public WanderingState wanderingState { get; set; }
    public AggresiveState aggresiveState { get; set; }
    public AlertState alertState { get; set; }
    public DeathState deathState { get; set; }

    [SerializeField] private BaseWander wander;
    [SerializeField] private BaseAggresive aggressive;

    public BaseWander BaseWanderInstance { get; set; }
    public BaseAggresive BaseAggresiveInstance { get; set; }

    // Player detection
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private LayerMask obstructionMask;
    public float radius;
    [Range(0f, 360f)]
    public float angle;

    public bool canSeePlayer {  get; private set; }

    private void Awake()
    {
        GameObject playerGameObject = GameObject.FindGameObjectWithTag("Player");
        player = playerGameObject.GetComponent<Player>();
        playerBody = player.GetPlayerCharacter;

        agent = GetComponent<NavMeshAgent>();
        enemyManager = GameObject.FindGameObjectWithTag("EnemyManager").GetComponent<EnemyManager>();

        BaseWanderInstance = Instantiate(wander);
        BaseAggresiveInstance = Instantiate(aggressive);

        stateMachine = new StateMachine();

        wanderingState = new WanderingState(this, stateMachine);
        aggresiveState = new AggresiveState(this, stateMachine);
        deathState = new DeathState(this, stateMachine);
        alertState = new AlertState(this, stateMachine);

    }

    private void Start()
    {
        BaseWanderInstance.Initialize(gameObject, this);
        BaseAggresiveInstance.Initialize(gameObject, this);

        // Starts the state machine on the wandering state
        stateMachine.Initialize(wanderingState);

        StartCoroutine(UpdateFOV());
    }

    private void Update()
    {
        stateMachine.currentState.Update();
    }

    public void OnShoot()
    {
        if(canSeePlayer)
            gun.Shoot(player, playerBody);

    }

    // Field of view check is done inside a enumerator
    // So the check isn't run as often and saves on performance
    private IEnumerator UpdateFOV()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.2f);
            FieldOfView();
        }
    }

    private void FieldOfView()
    {
        Collider[] rangeChecks = Physics.OverlapSphere(transform.position, radius, playerMask);

        if (rangeChecks.Length != 0)
        {
            Transform target = rangeChecks[0].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;

            if (Vector3.Angle(transform.forward, directionToTarget) < angle / 2)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, directionToTarget, distanceToTarget, obstructionMask))
                    canSeePlayer = true;
                else
                    canSeePlayer = false;
            }
            else
                canSeePlayer = false;
        }
        else if (canSeePlayer)
            canSeePlayer = false;

    }
}
