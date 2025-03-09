using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerActions : MonoBehaviour
{
    [Header("TRAPS CATALOG")]
    [SerializeField] private GameObject m_alarmTrap;
    [SerializeField] private GameObject m_pushTrap;
    [SerializeField] private GameObject m_captureTrap;
    [Space]
    [Header("TRAPS OUTLINE COLORS")]
    [SerializeField] private Color m_validPositionColor;
    [SerializeField] private Color m_invalidPositionColor;
    [SerializeField] private Color m_defaultColor;

    private Dictionary<AbilitiesEnum, GameObject> m_TrapsDictionnary;
    private GameObject m_currentTrap;

    private void Start()
    {
        m_TrapsDictionnary = new Dictionary<AbilitiesEnum, GameObject> {
            [AbilitiesEnum.ALARM_TRAP] = m_alarmTrap,
            [AbilitiesEnum.PUSH_TRAP] = m_pushTrap,
            [AbilitiesEnum.CAPTURE_TRAP] = m_captureTrap        
        };
    }

    public void PreviewTrap(AbilitiesEnum trap, Vector3 previewPosition, bool dropable)
    {
        if (m_currentTrap != null)
        {
            m_currentTrap.transform.position = previewPosition;
        }
        else
        {
            m_currentTrap = Instantiate(m_TrapsDictionnary[trap], previewPosition, Quaternion.identity, null);
        }        

        try
        {
            DynamicOutline outline = m_currentTrap.GetComponent<DynamicOutline>();
            Color outlineColor = dropable ? m_validPositionColor : m_invalidPositionColor;

            outline.SetOutlineColor(outlineColor);
        }
        catch (NullReferenceException)
        {
            Debug.Log("Could not retrieve DynamicOutline script");
        }
    }

    public bool DropTrap()
    {
        if (m_currentTrap)
        {
            try
            {
                DynamicOutline outline = m_currentTrap.GetComponent<DynamicOutline>();

                outline.SetOutlineColor(m_defaultColor);
                outline.SetOutlineAlpha(0f);
            }
            catch (NullReferenceException)
            {
                Debug.Log("Could not retrieve DynamicOutline script");
            }

            m_currentTrap = null;
            return true;
        }
        return false;
    }

    public void OnAbilityDeselection()
    {
        if (m_currentTrap != null)
        {
            Destroy(m_currentTrap);
            m_currentTrap = null;
        }
    }
}
