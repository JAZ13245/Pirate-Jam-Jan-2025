using UnityEngine;

[CreateAssetMenu(fileName = "Aggressive-Aggressive Stationary", menuName = "Enemy Logic/Aggressive Logic/Aggressive Stationary")]
public class StationaryAggressive : BaseAggresive
{
    private PlayerCharacter player;
    public override void CallEnter()
    {
        base.CallEnter();
        player = enemy.player.GetComponent<Player>().GetPlayerCharacter;
    }

    public override void CallExit()
    {
        base.CallExit();
    }

    public override void CallUpdate()
    {
        base.CallUpdate();
        enemy.transform.LookAt(player.transform.position);
    }

    public override void Initialize(GameObject gameObject, Enemy enemy)
    {
        base.Initialize(gameObject, enemy);
    }
}
