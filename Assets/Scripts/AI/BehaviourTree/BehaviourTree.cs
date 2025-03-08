using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BehaviourTree : BTNode
{
    public BehaviourTree()
    {
        name = "Tree";
    }

    public BehaviourTree(string n)
    {
        name = n;
    }

    public override Status Process()
    {
        if (children.Count == 0) return Status.SUCCESS;
        name = children[currentChild].name;
        return children[currentChild].Process();
    }

}
