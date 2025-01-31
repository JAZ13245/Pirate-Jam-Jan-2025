using System.Collections;
using System.Drawing.Text;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] private GunType type;
    [SerializeField] private GameObject gun;
    [SerializeField] private GameObject bullet;
    private Transform bulletSpawnPoint;
    private bool addBulletSpread = false;
    private float shootDelay = 2f;
    private float bulletSpeed = 5f;
    private int bulletDamage = 10;
    private float bulletRange = 10.0f;
    private int maxAmmo = -1;
    private float reloadTime = 3;
    private Vector3 bulletSpreadVariance = new Vector3(0.1f, 0.1f, 0.1f);

    private BulletManager bulletManager;

    private Player player;

    private enum GunType { pistol, machineGun, shotgun }

    private int currentAmmo;
    private float lastShootTime;

    private delegate void ShootDelegate(Player player, PlayerCharacter playerBody);
    ShootDelegate shoot;

    private void Start()
    {
        bulletManager = BulletManager.Instance;
        bulletSpawnPoint = gun.transform;
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

        switch (type)
        {
            case GunType.pistol:
                shootDelay = 2f;
                bulletSpeed = 5f;
                bulletDamage = 5;
                // -1 means that the enemy never has to reload
                maxAmmo = -1;
                addBulletSpread = false;
                bulletSpreadVariance = new Vector3(0.1f, 0.1f, 0.1f);
                shoot = RegularShoot;

                break;

            case GunType.machineGun:
                shootDelay = 0.1f;
                bulletSpeed = 20;
                bulletDamage = 5;
                maxAmmo = 10;
                reloadTime = 3;
                addBulletSpread = true;
                bulletSpreadVariance = new Vector3(0.5f, 0.5f, 0.5f);
                shoot = RegularShoot;

                break;

            case GunType.shotgun:
                shootDelay = 2f;
                bulletSpeed = 5;
                bulletDamage = 3;
                maxAmmo = 1;
                reloadTime = 3;
                addBulletSpread = true;
                bulletSpreadVariance = new Vector3(0.9f, 0.9f, 0.9f);
                shoot = ShotgunShoot;
                break;
        }
        currentAmmo = maxAmmo;
    }

    public void Shoot(Player player, PlayerCharacter playerBody)
    {
        shoot(player, playerBody);
    }

    private void RegularShoot(Player player, PlayerCharacter playerBody)
    {
        if ((lastShootTime + shootDelay < Time.time) && currentAmmo != 0)
        {
            SpawnBullet().Shoot(GetDirection(playerBody), player);
            lastShootTime = Time.time;

            if(currentAmmo > 0)
                currentAmmo--;
        }
        else if(currentAmmo == 0)
        {
            Invoke("Reload", reloadTime);
        }

    }

    private void ShotgunShoot(Player player, PlayerCharacter playerBody) 
    {
        if ((lastShootTime + shootDelay < Time.time) && currentAmmo != 0)
        {
            for(int i = 0; i < 5; i++)
                SpawnBullet().Shoot(GetDirection(playerBody), player);

            lastShootTime = Time.time;

            if (currentAmmo > 0)
                currentAmmo--;
        }
        else if (currentAmmo == 0)
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
        Bullet bulletScript = bulletManager.pool.Get();

        bulletScript.transform.position = bulletSpawnPoint.position;
        bulletScript.SetSpeed(bulletSpeed);
        bulletScript.SetDamage(bulletDamage);
        bulletScript.SetRange(bulletRange);
        bulletScript.trail.enabled = true;
        return bulletScript;
    }

    private void Reload()
    {
        currentAmmo = maxAmmo;
    }

}
