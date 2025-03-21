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
    [SerializeField] MeshFilter _foVMeshFilter;
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

    private Mesh _foVMesh;

    // Start is called before the first frame update
    void Start()
    {
        _foVMesh = new Mesh();
        _foVMesh.name = "FoV Mesh";
    }

    // Update is called once per frame
    void Update()
    {
        float startAngle = Mathf.Deg2Rad * (-_visionConeAngle / 2);
        float stepAngle = Mathf.Deg2Rad * (_visionConeAngle / _numberOfRays);
        float currentAngle = startAngle;

        for (int i = 0; i <= _numberOfRays; i++)
        {
            float posX = Mathf.Sin(currentAngle);
            float posZ = Mathf.Cos(currentAngle);

            currentAngle += stepAngle;

            Vector3 dir = transform.TransformDirection(new Vector3(posX, 0f, posZ));
            Debug.DrawLine(transform.position, transform.position + dir * _visionConeRange, Color.green);
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

    private void DrawFieldOfView(List<Vector3> raycastsDirections)
    {
        int vertexCount = raycastsDirections.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;

        for (int i = 0; i < vertexCount-1; i++)
        {
            Ray ray = new Ray(transform.position, raycastsDirections[i]);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, _visionConeRange, _obstaclesMask))
            {
                vertices[i+1] = hit.point;
            }
            else
            {
                vertices[i+1] = raycastsDirections[i] * _visionConeRange;
            }

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }            
        }

        _foVMesh.Clear();
        _foVMesh.vertices = vertices;
        _foVMesh.triangles = triangles;
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _radius);
    }
}
