using UnityEngine;

public class DeathState : BaseState
{
    public DeathState(Enemy enemy, StateMachine stateMachine) : base(enemy, stateMachine)
    {

    }

    public override void Enter()
    {
        base.Enter();
        enemy.enemyManager.SetAllEnemiesToAttack();
    }

    public override void Exit()
    {
        enemy.KillEnemy();
        base.Exit();
    }

    public override void Update()
    {
        base.Update();
    }
}
