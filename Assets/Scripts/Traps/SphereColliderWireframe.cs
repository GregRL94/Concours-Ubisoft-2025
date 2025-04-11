using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class SphereColliderWireframe : MonoBehaviour
{
    public Color color = Color.black;
    public int circleSegments = 30;
    public bool drawInPlayMode = true;
    public float lineWidth = 0.08f;

    private SphereCollider sphereCollider;
    private LineRenderer lineRenderer;

    void Start()
    {
        sphereCollider = GetComponent<SphereCollider>();

        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = color;
        lineRenderer.endColor = color;
        lineRenderer.loop = true;
        UpdateLineWidth();
    }

    void Update()
    {
        if (drawInPlayMode)
        {
            if (gameObject.GetComponent<Collider>().providesContacts)
            {
                lineRenderer.enabled = false;
            }
            else
            {
                DrawWireframe();
            }
        }

    }

    void OnValidate()
    {
        if (lineRenderer != null)
        {
            UpdateLineWidth();
        }
    }

    void UpdateLineWidth()
    {
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            DrawWireframeGizmos();
        }
    }

    void DrawWireframe()
    {
        float scaledRadius = sphereCollider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
        Vector3 center = transform.position + sphereCollider.center;

        lineRenderer.positionCount = circleSegments + 1;
        Vector3[] positions = new Vector3[circleSegments + 1];

        DrawWireCircumference(center, scaledRadius, positions);
        lineRenderer.SetPositions(positions);
    }

    void DrawWireframeGizmos()
    {
        if (sphereCollider == null)
            sphereCollider = GetComponent<SphereCollider>();

        if (sphereCollider == null) return;

        Gizmos.color = color;
        float scaledRadius = sphereCollider.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z);
        Vector3 center = transform.position + sphereCollider.center;
        DrawWireCircumferenceGizmos(center, scaledRadius);
    }

    void DrawWireCircumference(Vector3 position, float radius, Vector3[] positions)
    {
        for (int i = 0; i <= circleSegments; i++)
        {
            float angle = i * (2 * Mathf.PI / circleSegments);
            positions[i] = position + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
        }
    }

    void DrawWireCircumferenceGizmos(Vector3 position, float radius)
    {
        Vector3 lastPoint = position + new Vector3(radius, 0, 0);
        for (int i = 1; i <= circleSegments; i++)
        {
            float angle = i * (2 * Mathf.PI / circleSegments);
            Vector3 newPoint = position + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(lastPoint, newPoint);
            lastPoint = newPoint;
        }
    }
}
