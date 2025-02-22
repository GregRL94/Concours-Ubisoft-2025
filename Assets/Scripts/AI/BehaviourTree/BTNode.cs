using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BTNode
{
    public enum Status { SUCCESS, RUNNING, FAILURE };
    public Status status;
    public List<BTNode> children = new List<BTNode>();
    public int currentChild = 0;
    public string name;

    public BTNode() { }

    public BTNode(string n)
    {
        name = n;
    }

    public virtual Status Process()
    {
        return children[currentChild].Process();
    }

    public void AddChild(BTNode n)
    {
        children.Add(n);
    }

    public void Reset()
    {
        currentChild = 0;
        foreach (BTNode n in children)
            n.Reset();
        
    }
}
