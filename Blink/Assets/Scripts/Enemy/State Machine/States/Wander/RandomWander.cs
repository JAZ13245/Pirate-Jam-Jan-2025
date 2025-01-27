using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "Wander-Random Wander", menuName = "Enemy Logic/Wander Logic/Random Wander")]
public class RandomWander : BaseWander
{
    private NavMeshAgent agent;
    private float radius = 20f;

    public override void CallEnter()
    {
        base.CallEnter();
        agent = enemy.agent;
        SetWanderPoint(GetWanderPoint());
    }

    public override void CallExit()
    {
        base.CallExit();
    }

    public override void CallUpdate()
    {
        base.CallUpdate();

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            SetWanderPoint(GetWanderPoint());
        }
    }

    public override void Initialize(GameObject gameObject, Enemy enemy)
    {
        base.Initialize(gameObject, enemy);
    }

    private Vector3 GetWanderPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection.y = 0f;

        Vector3 randomPoint = enemy.transform.position + randomDirection;

        NavMeshHit hit;
        Vector3 finalPosition = enemy.transform.position;

        if (NavMesh.SamplePosition(randomPoint, out hit, radius, 1))
        {
            finalPosition = hit.position;
        }

        return finalPosition;
    }

    private void SetWanderPoint(Vector3 point)
    {
        agent.SetDestination(point);
        enemy.transform.LookAt(point);
    }
}
