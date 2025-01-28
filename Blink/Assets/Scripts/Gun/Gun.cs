using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] protected bool addBulletSpread = false;
    [SerializeField] protected Vector3 bulletSpreadVariance = new Vector3(0.1f, 0.1f, 0.1f);
    [SerializeField] protected Transform bulletSpawnPoint;
    [SerializeField] protected GameObject bullet;
    [SerializeField] protected float shootDelay = 2f;

    protected float lastShootTime;

    public virtual void Shoot(Player player, PlayerCharacter playerBody)
    {
        if (lastShootTime + shootDelay < Time.time)
        {
            Vector3 direction = GetDirection(playerBody);

            GameObject currentBullet = Instantiate(bullet, bulletSpawnPoint.position, Quaternion.identity);
            currentBullet.GetComponent<Bullet>().Shoot(direction, player);
            lastShootTime = Time.time;
        }
    }

    protected Vector3 GetDirection(PlayerCharacter playerBody)
    {
        Vector3 direction = playerBody.transform.position - bulletSpawnPoint.position;

        if (addBulletSpread)
        {
            direction += new Vector3(Random.Range(-bulletSpreadVariance.x, bulletSpreadVariance.x),
                Random.Range(-bulletSpreadVariance.y, bulletSpreadVariance.y),
                Random.Range(-bulletSpreadVariance.z, bulletSpreadVariance.z));

            direction.Normalize();
        }

        return direction;
    }

}
