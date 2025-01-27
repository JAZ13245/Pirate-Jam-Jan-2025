using UnityEngine;
using UnityEngine.AI;

public class AggresiveState : BaseState
{
    private PlayerCharacter player;
    private NavMeshAgent agent;

    public AggresiveState(Enemy enemy, StateMachine stateMachine) : base(enemy, stateMachine)
    {
        player = enemy.player.GetComponent<Player>().GetPlayerCharacter;
        agent = enemy.agent;
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        enemy.transform.LookAt(player.transform.position);
        agent.SetDestination(player.transform.position);

        // If the player is in range
        if (agent.remainingDistance <= agent.stoppingDistance)
            agent.isStopped = true;
        else
            agent.isStopped = false;


        base.Update();
    }
}
