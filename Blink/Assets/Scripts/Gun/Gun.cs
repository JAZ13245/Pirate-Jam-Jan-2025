using System.Collections;
using System.Drawing.Text;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] private GunType type;
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] private GameObject bullet;
    [SerializeField] private GameObject bulletTrail;
    [SerializeField] private LayerMask hitMasks;
    private bool addBulletSpread = false;
    private float shootDelay = 2f;
    private float bulletSpeed = 5f;
    private int bulletDamage = 10;
    private float bulletRange = 10.0f;
    private int maxAmmo = -1;
    private float reloadTime = 3;
    private Vector3 bulletSpreadVariance = new Vector3(0.1f, 0.1f, 0.1f);

    private BulletManager bulletManager;
    TrailRenderer trails;

    private Player player;
    int count = 0;

    private enum GunType { pistol, machineGun, shotgun }

    private int currentAmmo;
    private float lastShootTime;

    private delegate void ShootDelegate(Player player, PlayerCharacter playerBody);
    ShootDelegate shoot;

    private void Start()
    {
        bulletManager = BulletManager.Instance;
        trails = bulletTrail.GetComponent<TrailRenderer>();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

        switch (type)
        {
            case GunType.pistol:
                shootDelay = 2f;
                bulletSpeed = 5f;
                bulletDamage = 10;
                // -1 means that the enemy never has to reload
                maxAmmo = -1;
                addBulletSpread = false;
                bulletSpreadVariance = new Vector3(0.1f, 0.1f, 0.1f);
                shoot = RegularShoot;

                break;

            case GunType.machineGun:
                shootDelay = 0.1f;
                bulletSpeed = 20;
                bulletDamage = 10;
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

    private void TestShoot(Player player, PlayerCharacter playerBody)
    {
        if ((lastShootTime + shootDelay < Time.time) && currentAmmo != 0)
        {
            /*
            Vector3 direction = GetDirection(playerBody);
            
            if (Physics.Raycast(bulletSpawnPoint.position, direction, out RaycastHit hit, float.MaxValue, hitMasks))
            {
                TrailRenderer trail = Instantiate(trails, bulletSpawnPoint.position, Quaternion.identity);

                StartCoroutine(SpawnTrail(trail, hit));

                lastShootTime = Time.time;

                if (currentAmmo > 0)
                    currentAmmo--;
            }
            */
            

            StartCoroutine(SimulateBullet(playerBody));
        }
        else if (currentAmmo == 0)
        {
            Invoke("Reload", reloadTime);
        }
    }

    private IEnumerator SimulateBullet(PlayerCharacter playerBody)
    {
        float step = 0.02f;
        Vector3 direction = GetDirection(playerBody);
        Vector3 startPosition = bulletSpawnPoint.position;
        lastShootTime = Time.time;
        float travelTime = bulletRange;

        Bullet spawnedBullet = bulletManager.pool.Get();
        spawnedBullet.transform.position = startPosition;

        if (currentAmmo > 0)
            currentAmmo--;

        while (travelTime > 0)
        {
            travelTime -= step;
            yield return new WaitForSeconds(step);

            //Debug.DrawLine(startPosition, direction * (bulletSpeed * step));

            if (Physics.Raycast(startPosition, direction, out RaycastHit hit, bulletSpeed * step, hitMasks))
            {
                //Debug.Log(hit.collider.tag);

                if (hit.collider.CompareTag("PlayerBody"))
                {
                    player.DamagePlayer(bulletDamage);
                    Debug.Log(hit.collider.bounds);
                }

                //bulletManager.pool.Release(bullet);
                break;
            }

            startPosition += direction * (bulletSpeed * step);
            spawnedBullet.transform.position = startPosition;

        }
    }

    private IEnumerator SpawnTrail(TrailRenderer trail, RaycastHit hit)
    {
        float time = 0;
        Vector3 startPosition = trail.transform.position;

        while(time < 1)
        {
            trail.transform.position = Vector3.Lerp(startPosition, hit.point, time);
            time += Time.deltaTime / trail.time;
            yield return null;
        }
        /*
        //player.DamagePlayer(10);
        if (hit.collider.CompareTag("PlayerBody"))
        {
            Debug.Log("hit player!");
        }
        */
        if (hit.collider.CompareTag("PlayerBody"))
        {
            Debug.Log("hit player!");
        }
        Destroy(trail.gameObject, trail.time);
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
