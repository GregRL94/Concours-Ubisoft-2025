using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTInverter : BTNode
{
    public BTInverter(string n)
    {
        name = n;
    }

    public override Status Process()
    {
        Status childstatus = children[0].Process();
        if (childstatus == Status.RUNNING) return Status.RUNNING;
        if (childstatus == Status.FAILURE)
            return Status.SUCCESS;
        else
            return Status.FAILURE;

    }


}
