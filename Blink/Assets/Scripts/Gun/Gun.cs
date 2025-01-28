using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] private GunType type;
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] private GameObject bullet;
    private bool addBulletSpread = false;
    private float shootDelay = 2f;
    private float bulletSpeed = 5f;
    private int bulletDamage = 10;
    private int maxAmmo = -1;
    private float reloadTime = 3;
    private Vector3 bulletSpreadVariance = new Vector3(0.1f, 0.1f, 0.1f);

    private enum GunType { pistol, machineGun, shotgun }

    private int currentAmmo;


    private void Start()
    {
        switch (type)
        {
            case GunType.pistol:
                shootDelay = 2f;
                bulletSpeed = 3;
                bulletDamage = 10;
                // -1 means that the enemy never has to reload
                maxAmmo = -1;
                addBulletSpread = false;
                bulletSpreadVariance = new Vector3(0.1f, 0.1f, 0.1f);

                break;

            case GunType.machineGun:
                shootDelay = 0.1f;
                bulletSpeed = 5;
                bulletDamage = 10;
                maxAmmo = 20;
                reloadTime = 3;
                addBulletSpread = true;
                bulletSpreadVariance = new Vector3(0.5f, 0.5f, 0.5f);

                break;

            case GunType.shotgun:
                break;
        }
        currentAmmo = maxAmmo;
    }

    private float lastShootTime;

    public void Shoot(Player player, PlayerCharacter playerBody)
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
            Invoke("Reload", reloadTime);
        }
    }

    private Vector3 GetDirection(PlayerCharacter playerBody)
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

    private Bullet SpawnBullet()
    {
        // Bad practice - make sure to fix this
        GameObject currentBullet = Instantiate(bullet, bulletSpawnPoint.position, Quaternion.identity);
        Bullet bulletScript = currentBullet.GetComponent<Bullet>();
        bulletScript.SetSpeed(bulletSpeed);
        bulletScript.damage = bulletDamage;
        return bulletScript;
    }

    private void Reload()
    {
        currentAmmo = maxAmmo;
    }

}
