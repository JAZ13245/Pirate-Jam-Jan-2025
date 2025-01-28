using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "Wander-Idle Wander", menuName = "Enemy Logic/Wander Logic/Idle Wander")]
public class IdleWander : BaseWander
{
    private NavMeshAgent agent;
    private bool isWander;
    private MonoBehaviour mono;
    private Vector3 newRotation;

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
        Quaternion rotationLerp = Quaternion.Lerp(enemy.transform.rotation, Quaternion.Euler(newRotation), Time.deltaTime);
        enemy.transform.rotation = rotationLerp;
    }

    public override void Initialize(GameObject gameObject, Enemy enemy)
    {
        base.Initialize(gameObject, enemy);
    }

    private IEnumerator RotateEnemy()
    {
        while (isWander)
        {
            Vector3 rotationAngle = new Vector3(0, Random.Range(0f, 360f), 0);
            newRotation = rotationAngle;
            yield return new WaitForSeconds(5);
        }
    }
}
