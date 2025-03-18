using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerVision : MonoBehaviour
{
    [SerializeField] private float _proximityVisionRadius;

    [Header("Vision cone")]
    [SerializeField] private float azimutResolution;
    [SerializeField] private float elevationResolution;
    [SerializeField] private float visionConeRange;
    [SerializeField] private float visionConeRadius;

    private float currentElevation;
    private float currentAzimut;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < 10; i++)
        {
            for (int j = 0; j < 10; j++)
            {

            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
