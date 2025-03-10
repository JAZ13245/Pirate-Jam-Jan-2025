using UnityEngine;

public class DamageEnemy : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Enemy")
        {
            Enemy enemy = other.GetComponent<Enemy>();
            enemy.stateMachine.ChangeState(enemy.deathState);
        }

        if(other.tag == "Character")
        {
            KillCharacter character = other.GetComponent<KillCharacter>();
            character.Kill();
        }
    }
}
