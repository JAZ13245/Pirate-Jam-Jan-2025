using UnityEngine;

public class KillCharacter : MonoBehaviour
{
    private float height;
    [SerializeField, Range(1, 10), Tooltip("The number of blood blobs to spawn when this enemy explodes.")] private int bloodBlobSpawnCount;
    [SerializeField, Range(8f, 20f), Tooltip("The minimum velocity that this blood blob can have when it is created.")] private float minGenVelocity;
    [SerializeField, Range(8f, 20f), Tooltip("The maximum velocity that this blood blob can have when it is created.")] private float maxGenVelocity;
    [SerializeField] private GameObject bloodBlobPrefab;
    private EnemyManager enemyManager;

    private void Start()
    {
        height = GetComponent<CapsuleCollider>().height;
        enemyManager = GameObject.FindGameObjectWithTag("EnemyManager").GetComponent<EnemyManager>();
    }

    public void Kill()
    {
        Explode(this.transform.up);
        enemyManager.CharacterDie(this);
        Destroy(gameObject);
    }

    private void Explode(Vector3 direction = default)
    {
        Vector3 spawnPoint = new Vector3(transform.position.x, transform.position.y + height / 2, transform.position.z);
        for (int i = 0; i < bloodBlobSpawnCount; i++)
        {
            GameObject bloodBlob = Instantiate(bloodBlobPrefab, spawnPoint, Quaternion.identity);

            Vector3 launchDirection;
            if (direction.magnitude == 0)
            {
                launchDirection = Random.onUnitSphere * Random.Range(minGenVelocity, maxGenVelocity);
            }
            else
            {
                if (i == 0)
                {
                    launchDirection = -direction * maxGenVelocity;
                }
                else
                {
                    launchDirection = Random.onUnitSphere * Random.Range(minGenVelocity, maxGenVelocity);
                    if (Vector3.Dot(launchDirection, direction) < 0)
                    {
                        launchDirection *= -1;
                    }
                }
            }

            bloodBlob.GetComponent<Rigidbody>().linearVelocity = launchDirection;
        }
    }
}
