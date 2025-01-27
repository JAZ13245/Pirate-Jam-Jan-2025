using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "Aggressive-Aggressive Chase", menuName = "Enemy Logic/Aggressive Logic/Aggressive Chase")]
public class AggressiveChase : BaseAggresive
{
    private PlayerCharacter player;
    private NavMeshAgent agent;

    public override void CallEnter()
    {
        base.CallEnter();
        player = enemy.player.GetComponent<Player>().GetPlayerCharacter;
        agent = enemy.agent;
    }

    public override void CallExit()
    {
        base.CallExit();
    }

    public override void CallUpdate()
    {
        enemy.transform.LookAt(player.transform.position);
        agent.SetDestination(player.transform.position);

        // If the player is in range
        if (agent.remainingDistance <= agent.stoppingDistance)
            agent.isStopped = true;
        else
            agent.isStopped = false;

        base.CallUpdate();
    }

    public override void Initialize(GameObject gameObject, Enemy enemy)
    {
        base.Initialize(gameObject, enemy);
    }
}
