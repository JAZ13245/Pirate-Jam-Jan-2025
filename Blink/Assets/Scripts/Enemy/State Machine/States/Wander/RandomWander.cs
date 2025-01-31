using TMPro;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "Wander-Random Wander", menuName = "Enemy Logic/Wander Logic/Random Wander")]
public class RandomWander : BaseWander
{
    private NavMeshAgent agent;
    private float radius = 20f;
    private Vector3 wanderPoint;

    public override void CallEnter()
    {
        base.CallEnter();
        agent = enemy.agent;
        wanderPoint = GetWanderPoint();
        SetWanderPoint(wanderPoint);
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
            wanderPoint = GetWanderPoint();
            SetWanderPoint(wanderPoint);
        }

        Quaternion targetRotation = Quaternion.LookRotation(wanderPoint - enemy.transform.position);
        Quaternion rotationLerp = Quaternion.Lerp(enemy.transform.rotation, targetRotation, 5 * Time.deltaTime);
        enemy.transform.rotation = rotationLerp;
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
    }
}
