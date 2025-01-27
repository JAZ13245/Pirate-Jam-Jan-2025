using UnityEngine;
using UnityEngine.AI;
public class BaseAggresive : ScriptableObject
{
    protected Enemy enemy;
    protected Transform transform;
    protected GameObject gameObject;

    protected PlayerCharacter player;
    private NavMeshAgent agent;

    public virtual void Initialize(GameObject gameObject, Enemy enemy)
    {
        this.gameObject = gameObject;
        transform = gameObject.transform;
        this.enemy = enemy;

        agent = enemy.GetComponent<NavMeshAgent>();
        player = enemy.player.GetComponent<Player>().GetPlayerCharacter;
    }

    public virtual void CallEnter() { }
    public virtual void CallExit() { }
    public virtual void CallUpdate() 
    {
        enemy.transform.LookAt(player.transform.position);
        agent.SetDestination(player.transform.position);

        // If the player is in range
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            agent.isStopped = true;

        }
        else
            agent.isStopped = false;
    }
}
