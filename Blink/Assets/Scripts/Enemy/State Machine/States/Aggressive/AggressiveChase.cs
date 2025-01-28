using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "Aggressive-Aggressive Chase", menuName = "Enemy Logic/Aggressive Logic/Aggressive Chase")]
public class AggressiveChase : BaseAggresive
{
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

        base.agent.isStopped = base.closeToPlayer;
    }
    public override void Initialize(GameObject gameObject, Enemy enemy)
    {
        base.Initialize(gameObject, enemy);
    }
}
