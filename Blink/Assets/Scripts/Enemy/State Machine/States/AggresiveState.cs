using UnityEngine;

public class AggresiveState : BaseState
{
    public AggresiveState(Enemy enemy, StateMachine stateMachine) : base(enemy, stateMachine)
    {

    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        Debug.Log("aggressive");
        base.Update();
    }
}
