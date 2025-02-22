using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class BTDependantSequence : BTNode
{
    BehaviourTree dependancy;
    NavMeshAgent agent;
    public BTDependantSequence(string n, BehaviourTree d, NavMeshAgent a)
    {
        name = n;
        dependancy = d;
        agent = a;
    }

    public override Status Process()
    {
        if(dependancy.Process() == Status.FAILURE)
        {
            agent.ResetPath();
            foreach (BTNode n in children)
                n.Reset();
            return Status.FAILURE;
        }

        Status childstatus = children[currentChild].Process();
        if (childstatus == Status.RUNNING) return Status.RUNNING;
        if (childstatus == Status.FAILURE)
            return Status.FAILURE;

        currentChild++;
        if (currentChild >= children.Count)
        {
            currentChild = 0;
            return Status.SUCCESS;
        }

        return Status.RUNNING;
    }


}
