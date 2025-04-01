using UnityEngine;

public class DynamicOutline : MonoBehaviour
{
    [SerializeField] private Renderer m_optionalTargetMesh;
    private Material m_outlineMaterial;

    private void Awake()
    {
        Renderer renderer = m_optionalTargetMesh != null ? m_optionalTargetMesh : GetComponent<Renderer>();
        m_outlineMaterial = renderer.material;

        //foreach (Material mat in renderer.materials)
        //{
        //    if (mat.HasProperty("_OutlineColor")) { m_outlineMaterial = mat; }
        //}
    }

    public void SetOutlineInvalid()
    {
        m_outlineMaterial.color = Color.red;
        //SetOutlineThickness(0.1f);
        //SetOutlineColor(Color.red);
        //SetOutlineAlpha(0.5f);
    }

    public void SetOutlineValid()
    {
        m_outlineMaterial.color = Color.green;
        //SetOutlineThickness(0.1f);
        //SetOutlineColor(Color.green);
        //SetOutlineAlpha(0.5f);
    }

    public void SetOutlineDefault()
    {
        m_outlineMaterial.color = new Color(1f, 1f, 1f, 0f);
        //SetOutlineThickness(0.1f);
        //SetOutlineColor(Color.black);
        //SetOutlineAlpha(0f);
    }

    private void SetOutlineThickness(float thickness)
    {
        m_outlineMaterial.SetFloat("_OutlineThickness", thickness);
    }

    private void SetOutlineColor(Color color)
    {
        m_outlineMaterial.SetColor("_OutlineColor", color);
    }

    private void SetOutlineAlpha(float alpha)
    {
        m_outlineMaterial.SetFloat("_OutlineAlpha", alpha);
    }
}
