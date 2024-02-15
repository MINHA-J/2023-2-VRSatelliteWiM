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
    
    private Vector3 _curPos, _lastPos;
    private float _vel = 2.0f;
    private float _rotateSpeed = 30f;
    private float _graspedTime = 0.0f;
    private float _notGraspedTime = 0.0f;
    

    public enum SatelliteState
    {
        Idle,
        Grasped
    }

    private void Awake()
    {
        MiniWorld = MiniatureWorld.Instance;
        _interaction = GetComponent<InteractionBehaviour>();
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
        transform.GetChild(0).GetComponent<RingLOD>().Color = p.Color;
    }

    private void ScaleMarkedSpace(float delta) 
    {
        //var grabVec = (grabHand.GetGraspPoint() - transform.position) - initialGrabOffset;
        //var baseVec = grabSphere.transform.position - transform.position;

        //var delta = grabVec.magnitude/ baseVec.magnitude - 1;
        //var delta = this.transform.localPosition.magnitude  - 1;

        //Debug.Log(delta);
        //delta = Mathf.Sign(delta) * CoolMath.SmoothStep(0.05f, 0.3f, delta);
        delta = Time.deltaTime * delta * mark.Radius * _vel; 
        mark.transform.localScale += new Vector3(delta, delta, delta);
        mark.transform.localScale.Clamp(MiniatureWorld.MinMarkSize, MiniatureWorld.MaxMarkSize);
    }
    
    private void TranslateMarkedSpace(Vector3 delta) 
    {
        //delta = grabPos - grabSphere.transform.position;
        //delta /= mark.Radius;
        //delta.x = Mathf.Sign(delta.x) * CoolMath.SmoothStep(0.1f, 0.2f, Mathf.Abs(delta.x));
        //delta.x *= 1;
        //delta.z = delta.y;
        delta.y = 0.0f;
        //delta = delta * 5.0f * Time.deltaTime;
        //mark.transform.position += delta;
        delta = delta * mark.Radius * MiniWorld.RadiusRatio * Time.deltaTime * _vel;
        mark.transform.position += delta;
        //Vector3 moveVector = mark.transform.position + (delta * Time.deltaTime);
        //mark.transform.DOMove(moveVector, 0.01f, false);
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
                if (Math.Abs(_curPos.y - _lastPos.y) > 0.005f)
                {
                    //Debug.Log("Scale It!");
                    ScaleMarkedSpace((_curPos.y - _lastPos.y) * MiniWorld.RadiusRatio);
                    _lastPos = _curPos;
                }

                if ((Math.Abs(_curPos.x - _lastPos.x) > 0.005f) || (Math.Abs(_curPos.z - _lastPos.z) > 0.005f))
                {
                    //Debug.Log("Translate It!");
                    //Debug.Log(_curPos.x - _lastPos.x);
                    //Debug.Log(_curPos.y - _lastPos.y);
                    TranslateMarkedSpace((_curPos - _lastPos));
                    _lastPos = _curPos;
                }

                // Spherical WorldMap과 일정 거리 이상 떨어질 경우, Remove
                //Debug.Log(this.transform.localPosition.magnitude * 10000);
                if (this.transform.localPosition.magnitude * 10000 > 15000)
                {
                    MiniWorld.RemoveSatellite(index);
                }
                
                break;

        }
    }
}
