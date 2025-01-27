using Unity.VisualScripting;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float lifeTime = 0.0f;
    [SerializeField] private float maxLifeTime = 10.0f;
    private float speed = 5f;
    private Vector3 direction;
    private float distance = 0f;
    [SerializeField] private float range = 100f;

    public void Shoot(Vector3 direction)
    {
        this.direction = direction;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Move
        lifeTime += Time.deltaTime;
        this.transform.Translate(direction * speed * Time.deltaTime);

        distance += speed * Time.deltaTime;

        if (distance > range || lifeTime > maxLifeTime)
        {
            DestroyImmediate(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "PlayerBody")
            Debug.Log("hit");
        Debug.Log(other.gameObject.tag);
        //Destroy(this.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "PlayerBody")
            Debug.Log("hit");
        Debug.Log(collision.gameObject.tag);
        //Destroy(this.gameObject);
    }
}
