using UnityEngine;

public class WanderingState : BaseState
{
    public WanderingState(Enemy enemy, StateMachine stateMachine) : base(enemy, stateMachine)
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
        Debug.Log("wandering");
        base.Update();
    }
}
