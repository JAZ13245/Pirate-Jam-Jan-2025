using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "Aggressive-Strafing Aggressive", menuName = "Enemy Logic/Aggressive Logic/Strafing Aggressive")]
public class StrafingAggressive : BaseAggresive
{
    private float currentAngleFromPlayer;

    public override void CallEnter()
    {
        base.CallEnter();
    }

    public override void CallExit()
    {
        base.CallExit();
    }

    public override void CallUpdate()
    {
        base.CallUpdate();

        if (base.closeToPlayer)
        {
            if(Vector3.Distance(enemy.transform.position, base.agent.destination) <= agent.stoppingDistance)
                base.agent.destination = GetDistanceFromStoppingDistanceCircle();
        }
    }

    public override void Initialize(GameObject gameObject, Enemy enemy)
    {
        base.Initialize(gameObject, enemy);
    }

    private Vector3 GetDistanceFromStoppingDistanceCircle()
    {
        //Debug.Log("called");
        int position = Random.Range(0, 14);
        float x = base.player.transform.position.x + agent.stoppingDistance * Mathf.Cos(2 * Mathf.PI * 10 / 14);
        float z = base.player.transform.position.z + agent.stoppingDistance * Mathf.Sin(2 * Mathf.PI * 10 / 14); ;

        return new Vector3(x, enemy.transform.position.y, z);
    }
}
