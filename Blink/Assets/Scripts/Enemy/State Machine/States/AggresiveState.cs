using UnityEngine;
using UnityEngine.AI;

public class AggresiveState : BaseState
{

    public AggresiveState(Enemy enemy, StateMachine stateMachine) : base(enemy, stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        enemy.BaseAggresiveInstance.CallEnter();
    }

    public override void Exit()
    {
        base.Exit();
        enemy.BaseAggresiveInstance.CallExit();
    }

    public override void Update()
    {
        base.Update();
        enemy.BaseAggresiveInstance.CallUpdate();
    }
}
