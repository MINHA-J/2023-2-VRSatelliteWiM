using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Leap.Unity;
using Leap.Unity.Interaction;
using UnityEngine;
using UnityEngine.Serialization;

public class Satellite : MonoBehaviour
{
    public SatelliteState state = SatelliteState.Idle;
    public Vector3 initPos;
    public float mapRange = 35.0f;

    [SerializeField] private MiniatureWorld MiniWorld;
    [SerializeField] private uint index;
    [SerializeField] private MarkNode mark;
    [SerializeField] private ProxyNode proxy;
    [SerializeField] private Color color;

    private InteractionBehaviour _interaction;
    private LineRenderer _lineRenderer;
    private GameObject _totalWorldROI;
    
    private Vector3 _curPos, _lastPos;
    private float _vel = 2.0f;
    private float _rotateSpeed = 30f;
    private float _graspedTime = 0.0f;
    private float _notGraspedTime = 0.0f;

    private Vector3 _downRayVector;
    

    public enum SatelliteState
    {
        Idle,
        Grasped
    }

    private void Awake()
    {
        MiniWorld = MiniatureWorld.Instance;
        _interaction = GetComponent<InteractionBehaviour>();
        _lineRenderer = GetComponent<LineRenderer>();
        _totalWorldROI = MiniWorld.ROI.gameObject;
        
        _curPos = transform.localPosition;
        _lastPos = transform.localPosition;

        state = SatelliteState.Idle;
    }

    public void SetSatelliteIndex(uint i)
    {
        index = i;
    }
    
    public void SetProxies(MarkNode m, ProxyNode p)
    {
        mark = m;
        proxy = p;
        color = p.Color;
        GetComponent<MeshRenderer>().materials[0].SetColor("_Color", p.Color);

        m.Satellite = this;
        
        // Satellite의 Color
        transform.GetChild(0).GetComponent<RingLOD>().Color = p.Color;
        
        // Satellite Ray의 Color
        _lineRenderer.startColor = p.Color;
        _lineRenderer.endColor = p.Color;
        
    }

    private void ScaleMarkedSpace(float delta) 
    {
        delta = Time.deltaTime * delta * mark.Radius * _vel; 
        mark.transform.localScale = new Vector3(mark.Radius, mark.Radius, mark.Radius);
        mark.transform.localScale.Clamp(MiniatureWorld.MinMarkSize, MiniatureWorld.MaxMarkSize);
    }
    
    public void TranslateMarkedSpace() 
    {
        // pointInWorld를 miniature에서의 로컬 좌표로 변환합니다.
        Vector3 pointInWorldMiniature = MiniWorld.transform.InverseTransformPoint(_downRayVector);
        // miniature에서의 로컬 좌표를 World ROI에서의 로컬 좌표로 변환합니다.

        // World ROI에서의 로컬 좌표를 world 좌표로 변환합니다.
        Vector3 pointInWorldRoi = _totalWorldROI.transform.TransformPoint(pointInWorldMiniature);
        
        mark.transform.position = pointInWorldRoi;
    }

    private void UpdateDownRay()
    {
        // Satellite에서 miniature를 향하는 Ray
        RaycastHit raycastHit1, raycastHit2;
        Ray ray1 = new Ray(transform.position, transform.forward); //model이 회전해서 forward vector

        Vector3 rayPoint = Vector3.zero;
        
        // Camera로부터 나오는 Ray와 miniature world 간의 교차점을 구함
        if (Physics.Raycast(ray1, out raycastHit1, 3.0f))
        {
            Debug.DrawLine(ray1.origin, raycastHit1.point, Color.green);
            // Debug.Log(this.transform.localPosition.sqrMagnitude);
            
            // Sphere 내부에 생긴 경우와 외부로 나간 경우 처리를 위함
            if (this.transform.localPosition.sqrMagnitude < 0.25f)
            {
                rayPoint = raycastHit1.point;
                // Update Line Renderer
                _lineRenderer.SetPosition(0, this.transform.position);
                _lineRenderer.SetPosition(1, rayPoint);
                _downRayVector = rayPoint;
            }
            else if (raycastHit1.transform.CompareTag("Miniature"))
            {
                //canSetROI = true;
                Vector3 origin = raycastHit1.point + ray1.direction * 0.01f;
                Ray ray2 = new Ray(origin, ray1.direction);
                if (Physics.Raycast(ray2, out raycastHit2))
                {
                    Debug.DrawLine(ray2.origin, raycastHit2.point, Color.blue);
                    rayPoint = raycastHit2.point;

                    // Update Line Renderer
                    _lineRenderer.SetPosition(0, this.transform.position);
                    _lineRenderer.SetPosition(1, rayPoint);
                    _downRayVector = rayPoint;
                }
            }
            //else canSetROI = false;
        }
        //else canSetROI = false;

        //return rayPoint;
    }
    
    void SetViewToMark()
    {
        SphericaiWorld.Instance.cam.transform.position = new Vector3(
            mark.transform.position.x,
            SphericaiWorld.Instance.cam.transform.position.y,
            mark.transform.position.z);
    }
    
    void UpdateState()
    {
        if (!_interaction.isGrasped)
        { 
            // 1초간 쥐고 있지 않은 상태면 Satellite 
            _graspedTime = 0.0f;
            _notGraspedTime += Time.deltaTime;
            if (_notGraspedTime >= 1.0f)
            {
                state = SatelliteState.Idle;
            }

        }
        else
        {
            // 2초간 쥔 상태를 유지하면
            _notGraspedTime = 0.0f;
            _graspedTime += Time.deltaTime;
            if (_graspedTime >= 1.5f)
            {
                state = SatelliteState.Grasped;
            }
        }
    }
    
    private void Update()
    {
        // 언제나 Spherical WorldMap을 항하며, 공전함
        this.transform.forward = -MiniWorld.transform.up;
        _curPos = this.transform.localPosition;

        UpdateState();
        UpdateDownRay();
        
        switch (state)
        {
            case SatelliteState.Idle:
                //transform.RotateAround(SphericaiWorld.Instance.transform.position, Vector3.down, _rotateSpeed * Time.deltaTime);
                break;

            case SatelliteState.Grasped:
                _curPos = transform.localPosition;
                //SetViewToMark();
                
                //[FIX] z를 y로 수정
                //Debug.Log(Math.Abs(_curPos.z - _lastPos.z));
                if (Math.Abs(_curPos.y - _lastPos.y) >= 0.001f)
                {
                    //Debug.Log("Scale It!");
                    ScaleMarkedSpace((_curPos.y - _lastPos.y) * MiniWorld.RadiusRatio);
                }

                //if ((Math.Abs(_curPos.x - _lastPos.x) > 0.005f) || (Math.Abs(_curPos.z - _lastPos.z) > 0.005f))
                {
                    //Debug.Log("Translate It!");
                    TranslateMarkedSpace();
                    //_lastPos = _curPos;
                }

                // Spherical WorldMap과 일정 거리 이상 떨어질 경우, Remove
                //Debug.Log(this.transform.localPosition.magnitude * 10000);
                if (this.transform.localPosition.magnitude * 10000 > 15000)
                {
                    MiniWorld.RemoveSatellite(index);
                }
                
                break;

        }

        _lastPos = _curPos;
        
    }
}
