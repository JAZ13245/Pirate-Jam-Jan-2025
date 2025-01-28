using UnityEngine;
using UnityEngine.Pool;

public class BulletManager : MonoBehaviour
{
    private static BulletManager _instance;
    public static BulletManager Instance { get { return _instance; } }

    public ObjectPool<Bullet> pool;

    [SerializeField] private GameObject bulletPrefab;
    private Bullet bulletScript;

    private void Awake()
    {
        if (_instance != null && _instance != this)
            Destroy(this.gameObject);
        else
            _instance = this;
    }

    private void Start()
    {
        bulletScript = bulletPrefab.GetComponent<Bullet>();
        pool = new ObjectPool<Bullet>(CreateBullet, OnTakeBulletFromPool, OnReturnBulletToPool, OnDestroyBullet, true, 500, 500);
    }

    private Bullet CreateBullet()
    {
        Bullet bullet = Instantiate(bulletScript, Vector3.zero, Quaternion.identity);

        bullet.SetPool(pool);

        return bullet;
    }

    private void OnTakeBulletFromPool(Bullet bullet)
    {
        bullet.transform.position = Vector3.zero;
        bullet.transform.rotation = Quaternion.identity;

        bullet.gameObject.SetActive(true);
    }

    private void OnReturnBulletToPool(Bullet bullet)
    {
        bullet.gameObject.SetActive(false);
    }

    private void OnDestroyBullet(Bullet bullet)
    {
        Destroy(bullet.gameObject);
    }
}
