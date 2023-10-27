using System.Collections;
using System.Collections.Generic;
using Leap.Unity.Interaction;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(InteractionBehaviour))]
public class InteractionSphericalMap : MonoBehaviour
{
    [SerializeField] private float velocity = 10.0f;
    private GameObject cam;
    private GameObject sphericalMap;

    private InteractionBehaviour grabSphericalMapLeap;
    private InteractionHand grabHand;
    private Vector3 initialGrabOffset;
    private Material grabSphereMaterial;
    private GameObject _standardObj;
    private Vector3 _standard;
    
    private Color GrabSphereDefaultColor = new Color(0.2f, 0.6f, 0.2f, 0.9f);
    private Color GrabSphereGrabbedColor = new Color(0.8f, 0.1f, 0.1f, 0.9f);

    // Start is called before the first frame update
    void Start()
    {
        cam = SphericaiWorld.Instance.cam;
        //sphericalMap = SphericaiWorld.Instance.sphericalMap;
    
        _standardObj = GameObject.Find("standard");
        
        grabSphericalMapLeap = GetComponent<InteractionBehaviour>();
        grabSphericalMapLeap.OnGraspBegin += GrabSphereGraspBegin;
        grabSphericalMapLeap.OnGraspStay += GrabSphereGraspStay;
        grabSphericalMapLeap.OnGraspEnd += GrabSphereGraspedEnd;

        //grabSphereMaterial = sphericalMap.GetComponent<MeshRenderer>().material;
    }

    private void updateStandardPos()
    {
        Transform P = _standardObj.transform.parent;
        _standardObj.transform.SetParent(null);
        _standard = _standardObj.transform.position;
        _standardObj.transform.SetParent(P);
    }
    
    private void GrabSphereGraspBegin() 
    {
        Debug.Log("GrabSphereGraspBegin");
        grabHand = grabSphericalMapLeap.graspingController.intHand;
        if (grabHand.NoFistPinchStrength() < 0.6f)
        {
            //initialGrabOffset = grabHand.GetGraspPoint() - grabSphere.transform.position;
            initialGrabOffset = grabHand.GetGraspPoint() - _standard;
            //grabSphereMaterial.DOColor(GrabSphereGrabbedColor, 0.1f);
        }
    }

    private void GrabSphereGraspedEnd() 
    {
        Debug.Log("GrabSphereGraspedEnd");
        grabHand = null;
        //grabSphereMaterial.DOColor(GrabSphereDefaultColor, 0.1f);
    }

    private void GrabSphereGraspStay()
    {
        //if (grabHand.isLeft)
        updateStandardPos();
        TranslateSphericalMap();
        ScaleSphericalMap();
    }

    public void ScaleSphericalMap()
    {
        // TODO: Scale은 Move이후에 진행
    }
    
    public void TranslateSphericalMap()
    {
        var grabPos = grabHand.GetGraspPoint();
        //Debug.DrawLine(grabPos, _standard, Color.green, 3.0f);
        var delta = grabPos - _standard;

        // Camera를 이동하여, Spherical이 회전하는 것처럼 구현
        // Vector3 GrabToCam = new Vector3(
        //     delta.x * CoolMath.SmoothStep(0.1f, 0.2f, Mathf.Abs(delta.x)),
        //     0.0f,
        //     delta.y * CoolMath.SmoothStep(0.1f, 0.2f, Mathf.Abs(delta.y))
        // );
        Vector3 GrabToCam = new Vector3(
            delta.x ,
            0.0f,
            delta.y
        );

        cam.transform.position += GrabToCam * (GrabToCam.magnitude * velocity) * -1.0f;
        cam.GetComponent<CameraController>().CamRangeCheck();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
