using UnityEngine;

public class AlertState : BaseState
{
    public AlertState(Enemy enemy, StateMachine stateMachine) : base(enemy, stateMachine)
    {

    }
    public override void Enter()
    {
        base.Enter();
        enemy.enemyManager.SetAllEnemiesToAttack();
        enemy.stateMachine.ChangeState(enemy.aggresiveState);
    }

    public override void Exit()
    {
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
    }
}
