using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "Aggressive-Strafing Aggressive", menuName = "Enemy Logic/Aggressive Logic/Strafing Aggressive")]
public class StrafingAggressive : BaseAggresive
{
    private int goLeft;
    private float currentAngleFromPlayer;
    private float angleIncrease = 20f;

    public override void CallEnter()
    {
        base.CallEnter();
        goLeft = Random.Range(0, 2);
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
            currentAngleFromPlayer = Vector3.Angle(base.player.transform.position, enemy.transform.position);
            if (goLeft == 0)
            {
                currentAngleFromPlayer =- 20;
                goLeft = 1;
            }
            else if (goLeft == 1) 
            {
                currentAngleFromPlayer += 20;
                goLeft = 0;
            }

            base.agent.destination = GetDistanceFromStoppingDistanceCircle();
        }
    }

    public override void Initialize(GameObject gameObject, Enemy enemy)
    {
        base.Initialize(gameObject, enemy);
    }

    private Vector3 GetDistanceFromStoppingDistanceCircle()
    {
        float x = base.player.transform.position.x + base.agent.radius * Mathf.Cos(currentAngleFromPlayer * (Mathf.PI / 180));
        float z = base.player.transform.position.z + base.agent.radius * Mathf.Cos(currentAngleFromPlayer * (Mathf.PI / 180));

        return new Vector3(x, 0, z);
    }
}
