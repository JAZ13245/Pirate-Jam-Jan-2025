using UnityEngine;

public class Baby : MonoBehaviour
{
    [SerializeField] Player player;
    private void OnTriggerEnter(Collider other)
    {
        player.Rouge();
    }

}
