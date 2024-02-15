using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RenderTextureSphere : MonoBehaviour
{
    public RenderTexture rt;
    public Renderer renderer;

    // Use this for initialization
    void Start () 
    {
        renderer = this.GetComponent<Renderer>();
        renderer.material.SetTexture("_MainTex", rt);
    }
}
