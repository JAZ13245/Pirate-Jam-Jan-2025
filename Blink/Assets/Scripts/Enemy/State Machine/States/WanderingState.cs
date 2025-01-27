using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class WanderingState : BaseState
{
    private NavMeshAgent agent;
    private float radius = 20f;
    private Vector3 wanderPoint;

    public WanderingState(Enemy enemy, StateMachine stateMachine) : base(enemy, stateMachine)
    {
        agent = enemy.agent;
    }

    public override void Enter()
    {
        base.Enter();
        wanderPoint = GetWanderPoint();
        agent.SetDestination(wanderPoint);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            wanderPoint = GetWanderPoint();
            agent.SetDestination(wanderPoint);
        }
    }

    private Vector3 GetWanderPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection.y = 0f;

        Vector3 randomPoint = enemy.transform.position + randomDirection;

        NavMeshHit hit;
        Vector3 finalPosition = enemy.transform.position;

        if(NavMesh.SamplePosition(randomPoint, out hit, radius, 1))
        {
            finalPosition = hit.position;
        }

        return finalPosition;
    }
}
