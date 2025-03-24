using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PlayerVision : MonoBehaviour
{
    public struct FoVCastInfo
    {
        public bool Hit;
        public Vector3 Point;
        public float Dist;
        public float Angle;

        public FoVCastInfo(bool hit, Vector3 point, float dist, float angle)
        {
            Hit = hit;
            Point = point;
            Dist = dist;
            Angle = angle;
        }
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

    [Header("LAYER MASKS")]
    [SerializeField] LayerMask _obstaclesMask;
    [SerializeField] LayerMask _gameAgentsMask;
    [Space]
    [Header("PROXIMITY DETECTION")]
    [SerializeField, Range(0f, 5f)] private float _proximityRadius;
    [SerializeField] private MeshFilter _proximityVisualization;
    [Space]
    [Header("VISION CONE")]
    [SerializeField] MeshFilter _foVMeshFilter;
    [SerializeField, Range(0f, 20f)] private float _visionConeRange;
    [SerializeField, Range(0f, 360f)] private float _visionConeAngle;
    [Space]
    [Header("DETECTION RAYS")]
    [SerializeField] int _numberOfRays;
    [SerializeField, Range(0.1f, 1f)] float _edgeResolveDistThreshold;
    [SerializeField, Range(0f, 10f)] int _edgeResolveIterations;

    private GameObject _foVGameObj;
    private Mesh _foVMesh;
    private SphereCollider _foVCollider;

    private List<GameObject> _elligibleObjects;
    private List<GameObject> _previousVisibleObjects;

    // Start is called before the first frame update
    void Start()
    {
        _foVMesh = new Mesh{name = "Field Of View Mesh"};
        _foVMeshFilter.mesh = _foVMesh;
        _foVGameObj = _foVMeshFilter.gameObject;

        _foVCollider = _foVGameObj.AddComponent<SphereCollider>();
        _foVCollider.radius = _visionConeRange;
        _foVCollider.isTrigger = true;

        _proximityVisualization.gameObject.transform.localScale = new Vector3(2 * _proximityRadius, 0.1f, 2 * _proximityRadius);

        _elligibleObjects = new List<GameObject>();
        _previousVisibleObjects = new List<GameObject>();
    }

    void LateUpdate()
    {
        DrawFieldOfView(_visionConeAngle, _numberOfRays);
        List<GameObject> currentVisibleObjectsList = FindVisibleObjects();
        ShowHideObjects(currentVisibleObjectsList);
        _previousVisibleObjects = currentVisibleObjectsList;
    }

    private List<GameObject> FindVisibleObjects()
    {
        List<GameObject> visibleObjects = new List<GameObject>();

        foreach(GameObject obj in _elligibleObjects)
        {
            if (Vector3.Distance(transform.position, obj.transform.position) <= _proximityRadius)
            {
                visibleObjects.Add(obj);
                continue;
            }

            Vector3 dir = obj.transform.position - transform.position;
            float angle = Vector3.Angle(transform.forward, dir);
            if (angle <= _visionConeAngle / 2)
            {
                if (!Physics.Raycast(transform.position, dir, Vector3.Distance(transform.position, obj.transform.position), _obstaclesMask))
                {
                    Debug.DrawLine(transform.position, obj.transform.position, Color.green);
                    visibleObjects.Add(obj);
                }
            }
        }
        return visibleObjects;
    }

    private void ShowHideObjects(List<GameObject> newVisibleObjectsList)
    {
        DynamicMeshDisplay meshDisplay;

        foreach (GameObject obj in newVisibleObjectsList)
        {
            meshDisplay = obj.GetComponent<DynamicMeshDisplay>();
            if (meshDisplay != null)
            {
                meshDisplay.OnGetVision(true);
            }
        }

        foreach(GameObject obj in _previousVisibleObjects)
        {
            meshDisplay = obj.GetComponent<DynamicMeshDisplay>();
            if (!newVisibleObjectsList.Contains(obj))
            {
                meshDisplay = obj.GetComponent<DynamicMeshDisplay>();
                if(meshDisplay != null)
                {
                    Debug.Log("Hiding Mesh for: " + obj.name);
                    meshDisplay.OnGetVision(false);
                }
            }
        }
    }

    private void DrawFieldOfView(float viewAngleInDeg, int numberOfRays)
    {
        List<Vector3> hitPoints = new List<Vector3>();

        float startAngle = -viewAngleInDeg / 2;
        float stepAngle = viewAngleInDeg / numberOfRays;
        float currentAngle = startAngle;
        FoVCastInfo oldFoVCast = new FoVCastInfo();

        for (int i = 0; i <= numberOfRays; i++)
        {
            FoVCastInfo currentFoVCast = FoVCast(currentAngle);

            if (i > 0)
            {
                bool edgeDistThresholdExceeded = Mathf.Abs(currentFoVCast.Dist - oldFoVCast.Dist) > _edgeResolveDistThreshold;

                if ((oldFoVCast.Hit != currentFoVCast.Hit) || (oldFoVCast.Hit && currentFoVCast.Hit && edgeDistThresholdExceeded))
                {
                    EdgeInfo edge = FindEdge(oldFoVCast, currentFoVCast);
                    if (edge.PointA != Vector3.zero)
                    {
                        hitPoints.Add(edge.PointA);
                    }
                    if (edge.PointB != Vector3.zero)
                    {
                        hitPoints.Add(edge.PointB);
                    }
                }
            }
            hitPoints.Add(currentFoVCast.Point);
            currentAngle += stepAngle;
            oldFoVCast = currentFoVCast;
        }

        int vertexCount = hitPoints.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        int[] triangles = new int[(vertexCount - 2) * 3];

        vertices[0] = Vector3.zero;

        for (int i = 0; i < vertexCount-1; i++)
        {
            vertices[i + 1] = transform.InverseTransformPoint(hitPoints[i]);

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
        _foVMesh.RecalculateNormals();
        _foVMesh.RecalculateBounds();
    }

    public Vector3 AngleToDir(float angleInDeg, bool isAngleLocal)
    {
        angleInDeg = Mathf.Deg2Rad * angleInDeg;
        float dirX = Mathf.Sin(angleInDeg);
        float dirZ = Mathf.Cos(angleInDeg);

        if (isAngleLocal)
        {
            return transform.TransformDirection(new Vector3(dirX, 0f, dirZ));
        }

        return new Vector3(dirX, 0f, dirZ);
    }

    private FoVCastInfo FoVCast(float localAngle)
    {
        Vector3 dir = AngleToDir(localAngle, true);
        Ray ray = new Ray(transform.position, dir);

        if (Physics.Raycast(ray, out RaycastHit hit, _visionConeRange, _obstaclesMask))
        {
            return new FoVCastInfo(true, hit.point, hit.distance, localAngle);
        }
        return new FoVCastInfo(false, transform.position + dir * _visionConeRange, _visionConeRange, localAngle);
    }

    private EdgeInfo FindEdge(FoVCastInfo minFoVCast, FoVCastInfo maxFoVCast)
    {
        float minAngle = minFoVCast.Angle;
        float maxAngle = maxFoVCast.Angle;

        Vector3 minPoint = Vector3.zero;
        Vector3 maxPoint = Vector3.zero;

        for (int i = 0; i < _edgeResolveIterations; i++)
        {
            float angle = (minAngle + maxAngle) / 2;
            FoVCastInfo resolveCast = FoVCast(angle);
            bool edgeDistThresholdExceeded = Mathf.Abs(minFoVCast.Dist - resolveCast.Dist) > _edgeResolveDistThreshold;

            if (resolveCast.Hit == minFoVCast.Hit && !edgeDistThresholdExceeded)
            {
                minAngle = angle;
                minPoint = resolveCast.Point;
            }
            else
            {
                maxAngle = angle;
                maxPoint = resolveCast.Point;
            }
        }

        return new EdgeInfo(minPoint, maxPoint);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_elligibleObjects.Contains(other.gameObject))
        {
            _elligibleObjects.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (_elligibleObjects.Contains(other.gameObject))
        {
            _elligibleObjects.Remove(other.gameObject);
        }
    }
}
