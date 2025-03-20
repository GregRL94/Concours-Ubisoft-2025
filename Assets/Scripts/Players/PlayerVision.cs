using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerVision : MonoBehaviour
{
    [Header("PROXIMITY DETECTION")]
    [SerializeField] private float _proximityDetection;
    [Space]
    [Header("VISION CONE")]
    [SerializeField] private float _visionConeRange;
    [SerializeField] private float _visionConeAngle;
    [Space]
    [Header("DETECTION RAYS")]
    [SerializeField] LayerMask _obstaclesMask;
    [SerializeField] LayerMask _gameAgentsMask;
    [SerializeField, Range(0f, 1f)] private float _rotation;
    [SerializeField] private int _nbRays;
    [SerializeField] private float _offset;
    [SerializeField] private int _elevationSteps;


    private List<Vector3> _detectionPointsList;

    // Start is called before the first frame update
    void Start()
    {
        _detectionPointsList = new List<Vector3>();
        float stepElevationAngle = Mathf.Deg2Rad * (_visionConeAngle / _elevationSteps);

        for (int i = 0; i <= _elevationSteps; i++)
        {
            float currentElevation = i * stepElevationAngle;

            for (int j = 0; j < _nbRays; j++)
            {
                float currentAngle = j * 2 * Mathf.PI * _rotation;
                float detectionPointX = Mathf.Sin(currentElevation) * Mathf.Cos(currentAngle);
                float detectionPointY = Mathf.Sin(currentElevation) * Mathf.Sin(currentAngle);
                float detectionPointZ = Mathf.Acos(currentElevation);

                Vector3 dir = transform.TransformDirection(new Vector3(detectionPointX, detectionPointY, detectionPointZ));

                _detectionPointsList.Add(dir);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
       foreach(Vector3 dir in _detectionPointsList)
        {
            Ray ray = new Ray(transform.position + dir * _offset, dir);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, _visionConeRange, _obstaclesMask))
            {
                Debug.DrawRay(transform.position + dir * _offset, dir, Color.red);
            }
            else
            {
                Debug.DrawRay(transform.position + dir * _offset, dir, Color.green);
            }            
        }
    }

    public void OnDrawGizmos()
    {
       
    }
}
