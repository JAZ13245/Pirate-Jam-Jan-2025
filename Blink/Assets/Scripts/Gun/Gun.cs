using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] protected bool addBulletSpread = false;
    [SerializeField] protected Vector3 bulletSpreadVariance = new Vector3(0.1f, 0.1f, 0.1f);
    [SerializeField] protected Transform bulletSpawnPoint;
    [SerializeField] protected GameObject bullet;
    [SerializeField] protected float shootDelay = 2f;
    [SerializeField] protected float bulletSpeed = 5f;
    [SerializeField] protected int bulletDamage = 10;
    [SerializeField] protected int maxAmmo = -1;

    protected int currentAmmo;

    private void Start()
    {
        currentAmmo = maxAmmo;
    }

    protected float lastShootTime;

    public virtual void Shoot(Player player, PlayerCharacter playerBody)
    {
        if ((lastShootTime + shootDelay < Time.time) && currentAmmo != 0)
        {
            Vector3 direction = GetDirection(playerBody);

            SpawnBullet().Shoot(direction, player);
            lastShootTime = Time.time;

            if(currentAmmo > 0)
            {
                currentAmmo--;
            }
        }
        else if(currentAmmo == 0)
        {
            Invoke("Reload", 3f);
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

    protected Bullet SpawnBullet()
    {
        // Bad practice - make sure to fix this
        GameObject currentBullet = Instantiate(bullet, bulletSpawnPoint.position, Quaternion.identity);
        Bullet bulletScript = currentBullet.GetComponent<Bullet>();
        bulletScript.SetSpeed(bulletSpeed);
        bulletScript.damage = bulletDamage;
        return bulletScript;
    }

    protected void Reload()
    {
        currentAmmo = maxAmmo;
    }

}
