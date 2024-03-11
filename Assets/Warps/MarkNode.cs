using System.Collections.Generic;
using Leap.Unity.Interaction;
using UnityEngine;
using DG.Tweening;

public class MarkNode : WarpNode {
    public enum MarkState
    {
        Normal,
        Changing
    }

    public MarkHand LeftHand;
    public MarkHand RightHand;

    public MarkState State { get; private set; }
    private float spotlightRatio = 1.0f;
    
    public Color Color;
    private RingLOD ring;
    private Material sphereMaterial;

    private Transform debugSphere;
    public GameObject spotlight;
    private Light light;
    private SphereCollider collider;
    
    public GameObject HighlightPrefab;

    public Satellite Satellite;
    
    void Start() {
        State = MarkState.Normal;
        ring = GetComponentInChildren<RingLOD>();
        var sphere = GetComponentInChildren<MarkingSphereShaderSwitcher>().gameObject;
        sphereMaterial = sphere.GetComponent<Renderer>().material;

        collider = GetComponent<SphereCollider>();
        
        GameObject spot = Resources.Load("Prefabs/Spot Light", typeof(GameObject)) as GameObject;
        spotlight = Instantiate(spot);
        
        light = spotlight.GetComponent<Light>();
    }

    private void UpdateSpotLight()
    {
        // Color
        light.color = Color;
        
        // Spot range
        float angle = Mathf.Atan(Radius / spotlight.transform.position.y) * Mathf.Rad2Deg;
        light.spotAngle = angle;
        light.innerSpotAngle = angle;
        //Debug.Log(Radius);
        
        // position
        Vector3 newVec = new Vector3(transform.position.x, spotlight.transform.position.y, transform.position.z);
        spotlight.transform.position = newVec;
    }

    new void Update() 
    {
        base.Update();
        Radius = transform.localScale.x * spotlightRatio;
        ring.Color = Color;

        UpdateSpotLight();
    }

    public IEnumerable<GameObject> ContainedItems() {
        // Hacky fix because Physics.OverlapSphere does stupid things
        foreach(var obj in HighlightData.GetAll()) {
            var renderer = obj.GetComponent<Renderer>();
            if(renderer == null) {
                continue;
            }

            if(this.Contains(renderer.bounds.center)) {
                yield return obj;
            }
        }
    }

    public void Highlight(List<string> query) {
        // Hacky fix because Physics.OverlapSphere does stupid things
        foreach(var obj in HighlightData.GetAll(query)) {
            var renderer = obj.GetComponent<Renderer>();
            if(renderer == null) {
                continue;
            }

            if(!this.Contains(renderer.bounds.center)) {
                continue;
            }

            var size = renderer.bounds.extents.Average() * 1.5f;
        
            var highlight = Instantiate(HighlightPrefab, renderer.bounds.center, Quaternion.identity);
            highlight.transform.localScale = new Vector3(size, size, size);
            highlight.name = obj.name + " highlight";
        }
    }

    [ContextMenu("Highlight all book2")]
    public void HighlightTest() {
        Highlight(new List<string>() {"Book2"});
    }

    [ContextMenu("Flash")]
    public void Flash(int count = 2) {
        var flashColor = new Color(1f, 1f, 1f, 1f);
        sphereMaterial.DOColor(flashColor, "_OutsideColor", 0.1f)
            .SetOptions(true)
            .SetLoops(count * 2, LoopType.Yoyo)
            .SetEase(Ease.InCubic);
    }
}
