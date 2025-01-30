using UnityEngine;

[CreateAssetMenu(fileName = "Aggressive-Aggressive Stationary", menuName = "Enemy Logic/Aggressive Logic/Aggressive Stationary")]
public class StationaryAggressive : BaseAggresive
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
        enemy.transform.LookAt(new Vector3(player.transform.position.x, enemy.transform.position.y, player.transform.position.z));
        base.enemy.OnShoot();
    }

    public override void Initialize(GameObject gameObject, Enemy enemy)
    {
        base.Initialize(gameObject, enemy);
    }
}
