using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BTAgent : MonoBehaviour
{
    public BehaviourTree tree;
    public NavMeshAgent agent;

    public enum ActionState { IDLE, WORKING };
    public ActionState state = ActionState.IDLE;

    public BTNode.Status treeStatus = BTNode.Status.RUNNING;

    WaitForSeconds waitForSeconds;

    // Start is called before the first frame update
    public virtual void Start()
    {
        agent = this.GetComponent<NavMeshAgent>();
        tree = new BehaviourTree();
        waitForSeconds = new WaitForSeconds(Random.Range(0.1f, 0.3f));
        StartCoroutine("Behave");
    }

    IEnumerator Behave()
    {
        while (true)
        {
            treeStatus = tree.Process();
            yield return waitForSeconds;
        }
    }
}
