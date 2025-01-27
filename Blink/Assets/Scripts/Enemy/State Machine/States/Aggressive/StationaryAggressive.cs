using UnityEngine;

[CreateAssetMenu(fileName = "Aggressive-Aggressive Stationary", menuName = "Enemy Logic/Aggressive Logic/Aggressive Stationary")]
public class StationaryAggressive : BaseAggresive
{
    public override void CallEnter()
    {
        base.CallEnter();
        Debug.Log("aggressive state entered");
    }

    public override void CallExit()
    {
        base.CallExit();
    }

    public override void CallUpdate()
    {
        enemy.transform.LookAt(base.player.transform.position);
    }

    public override void Initialize(GameObject gameObject, Enemy enemy)
    {
        base.Initialize(gameObject, enemy);
    }
}
