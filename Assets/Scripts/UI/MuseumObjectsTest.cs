using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuseumObjectsTest : MonoBehaviour
{
    public enum ObjectType
    {
        PAINTING,
        JEWELRY,
        VASE,
        SCULPTURE
    }

    [SerializeField]
    protected ObjectType _objectType;

    public ObjectType MuseumObjectType => _objectType;
}
