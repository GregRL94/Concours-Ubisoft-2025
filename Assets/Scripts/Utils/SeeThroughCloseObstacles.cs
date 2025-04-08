using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeeThroughCloseObstacles : MonoBehaviour
{
    [SerializeField, Range(0f, 10f)] private float _seeTroughRange;

    private void Start()
    {
        SphereCollider collider = gameObject.AddComponent<SphereCollider>();
        collider.radius = _seeTroughRange;
        collider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        DynamicMaterialSetter dynamicMaterialSetter = other.GetComponent<DynamicMaterialSetter>();
        if (dynamicMaterialSetter == null) { return; }

        dynamicMaterialSetter.SetSeeThrough(true);
    }

    private void OnTriggerExit(Collider other)
    {
        DynamicMaterialSetter dynamicMaterialSetter = other.GetComponent<DynamicMaterialSetter>();
        if (dynamicMaterialSetter == null) { return; }

        dynamicMaterialSetter.SetSeeThrough(false);
    }
}
