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

    private Mesh _foVMesh;

    // Start is called before the first frame update
    void Start()
    {
        _foVMesh = new Mesh{name = "Field Of View Mesh"};
        _foVMeshFilter.mesh = _foVMesh;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        List<Vector3> directions = SetupRaycastsDirections(_visionConeAngle, _numberOfRays, true);
        DrawFieldOfView(directions);
    }

    private List<Vector3> SetupRaycastsDirections(float viewAngleInDeg, int numberOfRays, bool isAngleLocal)
    {
        List<Vector3> directions = new List<Vector3>();

        float startAngle = -viewAngleInDeg / 2;
        float stepAngle = viewAngleInDeg / numberOfRays;
        float currentAngle = startAngle;

        for (int i = 0; i <= numberOfRays; i++)
        {
            Vector3 dir = AngleToDir(currentAngle, isAngleLocal);
            directions.Add(dir);
            currentAngle += stepAngle;
        }
        return directions;
    }

    private void DrawFieldOfView(List<Vector3> raycastsDirections)
    {
        int vertexCount = raycastsDirections.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];
        Vector3 oldDir = Vector3.zero;

        vertices[0] = Vector3.zero;

        for (int i = 0; i < vertexCount-1; i++)
        {
            Ray ray = new Ray(transform.position, raycastsDirections[i]);
            if (Physics.Raycast(ray, out RaycastHit hit, _visionConeRange, _obstaclesMask))
            {
                vertices[i + 1] = transform.InverseTransformPoint(hit.point);
            }
            else
            {
                vertices[i + 1] = transform.InverseTransformDirection(raycastsDirections[i]) * _visionConeRange;
            }

            if (i < vertexCount - 2)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }

            //oldDir = raycastsDirections[i];
        }

        _foVMesh.Clear();
        _foVMesh.vertices = vertices;
        _foVMesh.triangles = triangles;
        _foVMesh.RecalculateNormals();
        _foVMesh.RecalculateBounds();
    }

    public Vector3 AngleToDir(float angleInDeg, bool isAngleLocal)
    {
        angleInDeg = Mathf.Deg2Rad * angleInDeg;
        float dirX = Mathf.Sin(angleInDeg);
        float dirZ = Mathf.Cos(angleInDeg);

        if (isAngleLocal) { return transform.TransformDirection(new Vector3(dirX, 0f, dirZ)); }
        return new Vector3(dirX, 0f, dirZ);
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, _radius);
    }

    public struct EdgeInfo
    {
        public Vector3 PointA;
        public Vector3 PointB;

        public EdgeInfo(Vector3 pointA, Vector3 pointB)
        {
            PointA = pointA;
            PointB = pointB;
        }
    }
}
