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
        if (other.gameObject.tag == "Enemy")
            return;

        if(other.gameObject.tag == "PlayerBody")
            Debug.Log("hit");

        Destroy(this.gameObject);
    }
}
