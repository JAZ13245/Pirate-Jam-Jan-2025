using UnityEngine;

public class DamageEnemy : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("HIT");
        if(other.tag == "Enemy")
        {
            Destroy(other.gameObject);
        }
    }
}
