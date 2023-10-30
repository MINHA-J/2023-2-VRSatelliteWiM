using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniatureWorld : MonoBehaviour
{

    public MiniatureWorldROI ROI;
    public float Radius = 10.0f;
    public bool UseEditorColor = false;
    public Color Color;
    public float ProxyScaleFactor { get; private set; }

    private float originalSize = 0.6f;
    private static List<MiniatureWorld> instances = new List<MiniatureWorld>();
    private ProxySphere sphere;

    public static IEnumerable<MiniatureWorld> Instances
    {
        get => instances;
    }
    
    void Awake()
    {        
        ProxyScaleFactor = 1f;
        if (!UseEditorColor)
        {
            Color = WarpColorPalette.GetColor();
        }

        sphere = GetComponentInChildren<ProxySphere>();
        sphere.Color = Color;

        //transform.DOScale(0f, 0.2f).From();

        //manipulation = GetComponent<Manipulation>();
        //manipulation.OnModeChange += OnManipulationModeChange;

        //PinchTracker.RegisterPinchable(this);
        instances.Add(this);
    }
    
    private void OnDestroy()
    {
        //PinchTracker.DeregisterPinchable(this);
        instances.Remove(this);
    }

    new void Update()
    {
        if (ROI != null)
        {
            //vfxGraph.SetVector3("EndPos", mark.transform.position);
            //vfxGraph.SetFloat("EndRadius", mark.Radius);
            ROI.Color = Color;
        }


        // A bit of a stupid hack because the cone effect requires world-space size of the
        // sphere, yet the whole ProxySphere otherwise operates oblivious to its actual size
        sphere.Radius = Radius;
    }
}
