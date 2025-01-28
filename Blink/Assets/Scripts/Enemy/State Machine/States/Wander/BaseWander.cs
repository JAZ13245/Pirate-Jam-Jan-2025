using UnityEngine;

public class BaseWander : ScriptableObject
{
    protected Enemy enemy;
    protected Transform transform;
    protected GameObject gameObject;

    public virtual void Initialize(GameObject gameObject, Enemy enemy)
    {
        this.gameObject = gameObject;
        transform = gameObject.transform;
        this.enemy = enemy;
    }

    public virtual void CallEnter() { }
    public virtual void CallExit() { }
    public virtual void CallUpdate() {
        if (enemy.canSeePlayer)
            enemy.stateMachine.ChangeState(enemy.alertState);
    }

}
