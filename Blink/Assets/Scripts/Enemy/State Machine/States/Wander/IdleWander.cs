using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "Wander-Idle Wander", menuName = "Enemy Logic/Wander Logic/Idle Wander")]
public class IdleWander : BaseWander
{
    private NavMeshAgent agent;
    private bool isWander;
    private MonoBehaviour mono;

    public override void CallEnter()
    {
        base.CallEnter();
        agent = enemy.agent;
        isWander = true;
        mono = enemy.GetComponent<MonoBehaviour>();
        mono.StartCoroutine(RotateEnemy());
    }

    public override void CallExit()
    {
        base.CallExit();
        isWander = false;
    }

    public override void CallUpdate()
    {
        base.CallUpdate();
        //Debug.Log(enemy.transform.rotation);
    }

    public override void Initialize(GameObject gameObject, Enemy enemy)
    {
        base.Initialize(gameObject, enemy);
    }

    private IEnumerator RotateEnemy()
    {
        while (isWander)
        {
            yield return new WaitForSeconds(5);
            Vector3 rotationAngle = new Vector3(0, Random.Range(0f, 360f), 0);
            enemy.transform.Rotate(rotationAngle);
        }
    }
}
