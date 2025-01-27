using UnityEngine;
using UnityEngine.AI;

public class BaseAggresive : ScriptableObject
{
    protected Enemy enemy;
    protected Transform transform;
    protected GameObject gameObject;

    protected PlayerCharacter player;
    protected NavMeshAgent agent;
    protected bool closeToPlayer;

    public virtual void Initialize(GameObject gameObject, Enemy enemy)
    {
        this.gameObject = gameObject;
        transform = gameObject.transform;
        this.enemy = enemy;

        agent = enemy.GetComponent<NavMeshAgent>();
        player = enemy.player;
    }

    public virtual void CallEnter() 
    {
        //enemy.enemyManager.SetAllEnemiesToAttack();
    }
    public virtual void CallExit() { }
    public virtual void CallUpdate() 
    {
        enemy.transform.LookAt(player.transform.position);
        agent.SetDestination(player.transform.position);
        enemy.OnShoot();

        // If the player is in range
        if (Vector3.Distance(enemy.transform.position, player.transform.position) <= agent.stoppingDistance)
            closeToPlayer = true;
        else
            closeToPlayer = false;
    }
}
