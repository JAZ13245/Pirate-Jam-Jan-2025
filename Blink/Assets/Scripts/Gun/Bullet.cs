using Unity.VisualScripting;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float lifeTime = 0.0f;
    [SerializeField] private float maxLifeTime = 10.0f;
    private float speed;
    private Vector3 direction;
    private float distance = 0f;
    [SerializeField] private float range = 100f;
    public int damage = 10;
    private Player player;
    private Rigidbody rb;

    public void SetSpeed(float speed)
    {
        this.speed = speed;
    }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Shoot(Vector3 direction, Player player)
    {
        this.direction = direction;
        this.player = player;
    }

    // Update is called once per frame
    void Update()
    {
        // Move
        lifeTime += Time.deltaTime;
        rb.AddForce(direction * speed, ForceMode.Impulse);

        distance += speed * Time.deltaTime;

        if (distance > range || lifeTime > maxLifeTime)
        {
            DestroyImmediate(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Enemy")
            return;

        if(other.gameObject.tag == "PlayerBody")
            player.DamagePlayer(damage);

        Destroy(this.gameObject);
    }
}
