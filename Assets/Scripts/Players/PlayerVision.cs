using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerVision : MonoBehaviour
{
    [Header("LAYER MASKS")]
    [SerializeField] LayerMask _obstaclesMask;
    [SerializeField] LayerMask _gameAgentsMask;
    [Space]
    [Header("PROXIMITY DETECTION")]
    [SerializeField, Range(0f, 5f)] private float _radius;
    [Space]
    [Header("VISION CONE")]
    [SerializeField, Range(0f, 20f)] private float _visionConeRange;
    [SerializeField, Range(0f, 360f)] private float _visionConeAngle;
    [Space]
    [Header("DETECTION RAYS")]
    [SerializeField] int _numberOfRays;
    [SerializeField] private float _offset;


    private List<Vector3> _detectionPointsList;

    // Start is called before the first frame update
    void Start()
    {
        _detectionPointsList = new List<Vector3>();
        float startAngle = Mathf.Deg2Rad * (-_visionConeAngle / 2);
        float stepAngle = Mathf.Deg2Rad * (_visionConeAngle / _numberOfRays);
        float currentAngle = startAngle;

        for (int i = 0; i <= _numberOfRays; i++)
        {
            float posX = Mathf.Sin(currentAngle);
            float posZ = Mathf.Cos(currentAngle);

            currentAngle += stepAngle;

            Vector3 dir = transform.TransformDirection(new Vector3(posX, 0f, posZ));
            _detectionPointsList.Add(dir);
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach (Vector3 dir in _detectionPointsList)
        {
            Ray ray = new Ray(transform.position + dir * _offset, dir);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, _visionConeRange, _obstaclesMask))
            {
                Debug.DrawRay(transform.position + dir * _offset, dir * _visionConeRange, Color.red);
            }
            else
            {
                Debug.DrawRay(transform.position + dir * _offset, dir * _visionConeRange, Color.green);
            }
        }
    }

    private List<Vector3> SetupRaycastsDirections()
    {
        List<Vector3> directions = new List<Vector3>();

        float startAngle = Mathf.Deg2Rad * (-_visionConeAngle / 2);
        float stepAngle = Mathf.Deg2Rad * (_visionConeAngle / _numberOfRays);
        float currentAngle = startAngle;

        for (int i = 0; i <= _numberOfRays; i++)
        {
            float posX = Mathf.Sin(currentAngle);
            float posZ = Mathf.Cos(currentAngle);

            currentAngle += stepAngle;

            Vector3 dir = transform.TransformDirection(new Vector3(posX, 0f, posZ));
            directions.Add(dir);
        }

        return directions;
    }

    public void OnDrawGizmos()
    {
        float startAngle = Mathf.Deg2Rad * (-_visionConeAngle / 2);
        float stepAngle = Mathf.Deg2Rad * (_visionConeAngle / _numberOfRays);
        float currentAngle = startAngle;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _radius);

        //for (int i = 0; i <= _numberOfRays; i++)
        //{
        //    float posX = Mathf.Sin(currentAngle);
        //    float posZ = Mathf.Cos(currentAngle);

        //    currentAngle += stepAngle;

        //    Vector3 dir = transform.TransformDirection(new Vector3(posX, 0f, posZ));
        //    Gizmos.DrawLine(transform.position + dir * _offset, transform.position + dir * _visionConeRange);
        //}
    }
}
