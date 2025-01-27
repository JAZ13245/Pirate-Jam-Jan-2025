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
        if(agent.remainingDistance <= agent.stoppingDistance)
            agent.SetDestination(player.transform.position);

        base.Update();
    }
}
