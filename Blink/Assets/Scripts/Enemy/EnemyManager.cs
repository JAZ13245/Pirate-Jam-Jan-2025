using NUnit.Framework;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    private List<Enemy> enemies = new List<Enemy>();
    private Player player;

    private void Start()
    {
        foreach (Transform child in transform)
        {
            Enemy enemy = child.GetComponent<Enemy>();
            enemies.Add(enemy);
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

    private void Update()
    {
        if (enemies.Count == 0)
            player.Win();
    }
}
