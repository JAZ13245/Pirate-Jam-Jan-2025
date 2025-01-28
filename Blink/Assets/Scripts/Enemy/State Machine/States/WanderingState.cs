using UnityEngine;

public class WanderingState : BaseState
{

    public WanderingState(Enemy enemy, StateMachine stateMachine) : base(enemy, stateMachine)
    {
    }

    public override void Enter()
    {
        base.Enter();
        enemy.BaseWanderInstance.CallEnter();
    }

    public override void Exit()
    {
        base.Exit();
        enemy.BaseWanderInstance.CallExit();
    }

    public override void Update()
    {
        base.Update();
        enemy.BaseWanderInstance.CallUpdate();
    }

}
