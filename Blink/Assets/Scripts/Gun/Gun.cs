using System.Collections;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [SerializeField] private bool addBulletSpread = false;
    [SerializeField] private Vector3 bulletSpreadVariance = new Vector3(0.1f, 0.1f, 0.1f);
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] private GameObject bullet;
    [SerializeField] private float shootDelay = 2f;
    [SerializeField] LayerMask player;

    private float lastShootTime;

    public void Shoot(PlayerCharacter player)
    {
        if (lastShootTime + shootDelay < Time.time) 
        { 

            Vector3 direction = GetDirection(player);

            GameObject currentBullet = Instantiate(bullet, bulletSpawnPoint.position, Quaternion.identity);
            currentBullet.GetComponent<Bullet>().Shoot(direction);
            lastShootTime = Time.time;
        }
    }

    private Vector3 GetDirection(PlayerCharacter player)
    {
        Vector3 direction = player.transform.position - bulletSpawnPoint.position;

        if(addBulletSpread)
        {
            direction += new Vector3(Random.Range(-bulletSpreadVariance.x, bulletSpreadVariance.x), 
                Random.Range(-bulletSpreadVariance.y, bulletSpreadVariance.y), 
                Random.Range(-bulletSpreadVariance.z, bulletSpreadVariance.z));

            direction.Normalize();
        }

        return direction;
    }

}
