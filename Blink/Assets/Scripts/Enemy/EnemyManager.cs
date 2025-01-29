using NUnit.Framework;
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class EnemyManager : MonoBehaviour
{
    public List<Transform> enemies;

    public void SetAllEnemiesToAttack()
    {
        foreach(Transform child in transform)
        {
            Enemy enemy = child.GetComponent<Enemy>();
            enemy.stateMachine.ChangeState(enemy.aggresiveState);
        }
    }
}
