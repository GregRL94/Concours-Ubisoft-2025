using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Experimental.GlobalIllumination;

public class BTAgent : MonoBehaviour
{
    [SerializeField]
    protected string _actionName;
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
            _actionName = tree.name;
            yield return waitForSeconds;
        }
    }
}
