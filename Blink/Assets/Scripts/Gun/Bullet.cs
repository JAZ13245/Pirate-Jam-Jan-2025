using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class Bullet : MonoBehaviour
{
    private float lifeTime = 0.0f;
    private float maxLifeTime = 10.0f;
    private Vector3 direction;
    private float distance = 0f;
    private float range;
    private int damage;
    private float speed;
    private Player player;
    private Rigidbody rb;
    public TrailRenderer trail;

    public void SetSpeed(float speed) {  this.speed = speed; }
    public void SetDamage(int damage) {  this.damage = damage; }
    public void SetRange(float range) { this.range = range; }

    private ObjectPool<Bullet> pool;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        trail = GetComponent<TrailRenderer>();
    }

    private void OnEnable()
    {
        lifeTime = 0.0f;
        maxLifeTime = 10.0f;
        distance = 0f;
        rb.linearVelocity = Vector3.zero;
    }

    private void OnDisable()
    {
        trail.enabled = false;
    }

    public void Shoot(Vector3 direction, Player player)
    {
        this.direction = direction;
        this.player = player;
        rb.AddForce(direction * speed, ForceMode.Impulse);
    }

    // Update is called once per frame
    void Update()
    {
        // Move
        lifeTime += Time.deltaTime;

        distance += speed * Time.deltaTime;

        if (distance > range || lifeTime > maxLifeTime)
        {
            //DestroyImmediate(gameObject);
            pool.Release(this);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject);
        if (other.gameObject.tag == "Enemy" || other.gameObject.tag == "Bullet")
            return;

        if(other.gameObject.tag == "PlayerBody")
            player.DamagePlayer(damage);

        //Destroy(this.gameObject);
        pool.Release(this);
    }

    public void SetPool(ObjectPool<Bullet> pool) {  this.pool = pool; }
}
