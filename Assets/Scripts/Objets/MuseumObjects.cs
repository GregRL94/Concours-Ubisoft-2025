using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuseumObjects : MonoBehaviour
{
    public enum ObjectType
    {
        PAINTING,
        JEWELRY
    }

    [SerializeField]
    protected ObjectType _objectType;

    public ObjectType MuseumObjectType => _objectType;
}
