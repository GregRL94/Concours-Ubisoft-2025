using UnityEngine;

public class DynamicOutline : MonoBehaviour
{
    private Material m_outlineMaterial;

    private void Awake()
    {
        foreach (Material mat in GetComponent<Renderer>().materials)
        {
            if (mat.HasProperty("_OutlineColor")) { m_outlineMaterial = mat; }
        }
    }

    public void SetOutlineInvalid()
    {
        SetOutlineThickness(0.1f);
        SetOutlineColor(Color.red);
        SetOutlineAlpha(0.5f);
    }

    public void SetOutlineValid()
    {
        SetOutlineThickness(0.1f);
        SetOutlineColor(Color.green);
        SetOutlineAlpha(0.5f);
    }

    public void SetOutlineDefault()
    {
        SetOutlineThickness(0.1f);
        SetOutlineColor(Color.black);
        SetOutlineAlpha(0f);
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
