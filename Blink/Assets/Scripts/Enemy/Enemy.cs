using System.Collections;
using Unity.AI.Navigation;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Animations;

public class Enemy : MonoBehaviour
{
    public NavMeshAgent agent { get; private set; }
    public Player player { get; private set; }
    public PlayerCharacter playerBody { get; private set; }
    public EnemyManager enemyManager { get; private set; }
    [SerializeField] private Gun gun;
    private float height;

    public StateMachine stateMachine { get; set; }
    public WanderingState wanderingState { get; set; }
    public AggresiveState aggresiveState { get; set; }
    public AlertState alertState { get; set; }
    public DeathState deathState { get; set; }

    public Animator animationContoller;
    private Vector3 previousPosition;
    private float currentSpeed;

    [SerializeField] private BaseWander wander;
    [SerializeField] private BaseAggresive aggressive;

    public BaseWander BaseWanderInstance { get; set; }
    public BaseAggresive BaseAggresiveInstance { get; set; }

    [SerializeField, Range(1, 10), Tooltip("The number of blood blobs to spawn when this enemy explodes.")] private int bloodBlobSpawnCount;
    [SerializeField, Range(8f, 20f), Tooltip("The minimum velocity that this blood blob can have when it is created.")] private float minGenVelocity;
    [SerializeField, Range(8f, 20f), Tooltip("The maximum velocity that this blood blob can have when it is created.")] private float maxGenVelocity;
    [SerializeField] private GameObject bloodBlobPrefab;

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
        height = GetComponent<CapsuleCollider>().height;

    }

    private void Start()
    {
        BaseWanderInstance.Initialize(gameObject, this);
        BaseAggresiveInstance.Initialize(gameObject, this);
        previousPosition = transform.position;

        // Starts the state machine on the wandering state
        stateMachine.Initialize(wanderingState);

        StartCoroutine(UpdateFOV());
    }

    private void Update()
    {
        stateMachine.currentState.Update();

        Vector3 currentMovement = transform.position - previousPosition;
        currentSpeed = currentMovement.magnitude / Time.deltaTime;
        previousPosition = transform.position;
        animationContoller.SetFloat("speed", currentSpeed);
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

    public void KillEnemy()
    {
        Explode(this.transform.up);
        Destroy(gameObject);
    }

    private void Explode(Vector3 direction = default)
    {
        Vector3 spawnPoint = new Vector3(transform.position.x, transform.position.y + height/2, transform.position.z);
        for (int i = 0; i < bloodBlobSpawnCount; i++)
        {
            GameObject bloodBlob = Instantiate(bloodBlobPrefab, spawnPoint, Quaternion.identity);

            Vector3 launchDirection;
            if (direction.magnitude == 0)
            {
                launchDirection = Random.onUnitSphere * Random.Range(minGenVelocity, maxGenVelocity);
            }
            else
            {
                if (i == 0)
                {
                    launchDirection = -direction * maxGenVelocity;
                }
                else
                {
                    launchDirection = Random.onUnitSphere * Random.Range(minGenVelocity, maxGenVelocity);
                    if (Vector3.Dot(launchDirection, direction) < 0)
                    {
                        launchDirection *= -1;
                    }
                }
            }

            bloodBlob.GetComponent<Rigidbody>().linearVelocity = launchDirection;
        }
    }
}
