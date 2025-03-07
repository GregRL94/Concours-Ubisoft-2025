using UnityEngine;

public class DynamicOutline : MonoBehaviour
{
    private Material m_outlineMaterial;

    private void Awake()
    {
        foreach (Material mat in GetComponent<Renderer>().materials)
        {
            if (mat.HasProperty("_OutlineColor"))
            {
                m_outlineMaterial = mat;
            }
        }
    }

    public void SetOutlineThickness(float thickness)
    {
        m_outlineMaterial.SetFloat("_OutlineThickness", thickness);
    }

    public void SetOutlineColor(Color color)
    {
        m_outlineMaterial.SetColor("_OutlineColor", color);
    }

    public void SetOutlineAlpha(float alpha)
    {
        m_outlineMaterial.SetFloat("_OutlineAlpha", alpha);
    }
}
