using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniatureWorldROI : MonoBehaviour
{
    public Color Color;
    private Material sphereMaterial;
    
    // Start is called before the first frame update
    void Start()
    {
        var sphere = GetComponentInChildren<MarkingSphereShaderSwitcher>().gameObject;
        sphereMaterial = sphere.GetComponent<Renderer>().material;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
