using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public StateMachine stateMachine { get; set; }
    public WanderingState wanderingState { get; set; }
    public AggresiveState aggresiveState { get; set; }
    public DeathState deathState { get; set; }

    public NavMeshAgent agent { get; private set;}

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        stateMachine = new StateMachine();

        wanderingState = new WanderingState(this, stateMachine);
        aggresiveState = new AggresiveState(this, stateMachine);
        deathState = new DeathState(this, stateMachine);

    }

    private void Start()
    {
        // Starts the state machine on the wandering state
        stateMachine.Initialize(wanderingState);
    }

    private void Update()
    {
        stateMachine.currentState.Update();
    }
}
