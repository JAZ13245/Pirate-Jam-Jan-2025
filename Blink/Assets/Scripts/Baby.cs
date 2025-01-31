using UnityEngine;

public class Baby : MonoBehaviour
{
    [SerializeField] private Player player;
    [SerializeField] private PlayerCharacter playerCharacter;
    [SerializeField] private float detectionRadius = 5f;
    
    private void Update()
    {
        if (playerCharacter != null && Vector3.Distance(transform.position, playerCharacter.transform.position) <= detectionRadius)
        {
            player.Rouge();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
