using UnityEngine;

public class Pistol : Gun
{
    public override void Shoot(Player player, PlayerCharacter playerBody)
    {
        base.Shoot(player, playerBody);
        if (base.lastShootTime + shootDelay < Time.time)
        {

            Vector3 direction = GetDirection(playerBody);

            GameObject currentBullet = Instantiate(bullet, bulletSpawnPoint.position, Quaternion.identity);
            currentBullet.GetComponent<Bullet>().Shoot(direction, player);
            base.lastShootTime = Time.time;
        }
    }

    public override Vector3 GetDirection(PlayerCharacter playerBody)
    {
        return base.GetDirection(playerBody);
    }
}
