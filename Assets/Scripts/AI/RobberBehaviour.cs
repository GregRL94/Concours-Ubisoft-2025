using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RobberBehaviour : BTAgent
{
    [Header("Metrics")]
    [Tooltip("Robber base speed")]
    private float _vBase;
    private float _currentSpeed = 0;
    [Tooltip("Robber circular vision")]
    private float _radialVision;
    private float _currentVision = 0;
    [Tooltip("Robber stealing time")]
    private float _stealTime;
    [Tooltip("Robber stealing range")]
    private float _stealRange;

    [Header("Target Game Object")]
    public GameObject painting;
    public GameObject car;

    public GameObject backDoor;
    public GameObject frontDoor;

    [Header("DEBUG")]
    [SerializeField]
    private bool _paintingStealed;

    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        BTSequence steal = new BTSequence("Steal Something");
        
        BTLeaf goToPainting = new BTLeaf("Go To Painting", GoToPainting);
        BTLeaf hasGotPainting = new BTLeaf("Go To Painting", HasGotPainting);
        BTLeaf goToFrontDoor = new BTLeaf("Go To Car", GoToFrontDoor);
        BTLeaf goToBackDoor = new BTLeaf("Go To Car", GoToBackDoor);
        BTLeaf goToCar = new BTLeaf("Go To Car", GoToCar);

        BTSelector goToDoor = new BTSelector("Go To Open Door");

        BTInverter invertMoney = new BTInverter("Invert Painting");
        invertMoney.AddChild(hasGotPainting);

        goToDoor.AddChild(goToFrontDoor);
        goToDoor.AddChild(goToBackDoor);

        steal.AddChild(hasGotPainting);
        steal.AddChild(goToDoor);
        steal.AddChild(goToPainting);
        steal.AddChild(goToCar);
        tree.AddChild(steal);

    }

    public BTNode.Status HasGotPainting()
    {
        if (_paintingStealed)
            return BTNode.Status.FAILURE;
        return BTNode.Status.SUCCESS;
    }
    public BTNode.Status GoToPainting()
    {
        BTNode.Status s = GoToLocation(painting.transform.position);
        if (s == BTNode.Status.SUCCESS)
        {
            Debug.Log("PAINTING STEALED !");
            painting.transform.parent = this.gameObject.transform;
            _paintingStealed = true;
        }
        return s;
    }

    public BTNode.Status GoToCar()
    {
        BTNode.Status s = GoToLocation(car.transform.position);
        if (s == BTNode.Status.SUCCESS)
        {
            Debug.Log("Flee success !");
            painting.SetActive(false);
        }
        return s;
    }

    public BTNode.Status GoToBackDoor()
    {
        return GoToDoor(backDoor);
    }

    public BTNode.Status GoToFrontDoor()
    {
        return GoToDoor(frontDoor);
    }

    public BTNode.Status GoToDoor(GameObject door)
    {
        BTNode.Status s = GoToLocation(door.transform.position);
        if (s == BTNode.Status.SUCCESS)
        {
            if (!door.GetComponent<Lock>().isLocked)
            {
                door.SetActive(false);
                return BTNode.Status.SUCCESS;
            }
            return BTNode.Status.FAILURE;
        }
        else
            return s;
    }
}
