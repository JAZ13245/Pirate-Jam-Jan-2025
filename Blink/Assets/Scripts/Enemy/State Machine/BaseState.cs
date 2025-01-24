using UnityEngine;

public class BaseState
{
    protected Enemy enemy;
    protected StateMachine stateMachine;

    public BaseState(Enemy enemy, StateMachine stateMachine)
    {
        this.enemy = enemy;
        this.stateMachine = stateMachine;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
    public virtual void Update() { }

}
