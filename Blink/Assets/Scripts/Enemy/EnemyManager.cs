using NUnit.Framework;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    private List<Enemy> enemies = new List<Enemy>();
    private List<KillCharacter> characters = new List<KillCharacter>();
    private Player player;

    private void Start()
    {
        foreach (Transform child in transform)
        {
            if(child.tag == "Enemy")
            {
                Enemy enemy = child.GetComponent<Enemy>();
                enemies.Add(enemy);
            }
            else
                characters.Add(child.GetComponent<KillCharacter>());
        }

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }

    public void SetAllEnemiesToAttack()
    {
        foreach(Enemy enemy in enemies)
        {
            enemy.stateMachine.ChangeState(enemy.aggresiveState);
        }
    }

    public void EnemyDied(Enemy enemy)
    {
        enemies.Remove(enemy);
    }

    public void CharacterDie(KillCharacter character)
    {
        if(characters.Contains(character))
            characters.Remove(character);
    }

    private void Update()
    {
        if (enemies.Count == 0 && characters.Count == 0)
            player.Win();
    }
}
