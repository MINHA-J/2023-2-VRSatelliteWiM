using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using Leap.Unity;

public class MakeRoi : MonoBehaviour
{
    /// <summary>
    /// The pinchDetector associated with this cursor
    /// </summary>
    [Header("Basic")]
    public Leap.Unity.PinchDetector pinchDetector;
    public Leap.Unity.Examples.PaintCursor paintCursor;
    
    [SerializeField] private bool isShown;
    [SerializeField] private bool canMake = false;
    [SerializeField] private bool isSetEnd = false;
    
    [Header("Setting")]
    [SerializeField]private const float upAngleThreashold = 60f;
    [SerializeField]private const float headAngleThreashold = 60f;
    [SerializeField]private MiniatureWorld miniatureWorld;
    [SerializeField] private uint index = 0;
    private float timer  = 0.0f;
    private float duration  = 2.0f;
    
    private Vector3 _targetPos;
    private Vector3 _beforePinchPos;
    private MarkNode _markNode;
    private Satellite _satellite;

    private Test01_Manager manager_1;
    private float _correctionTimer = 0.0f;


    private void Start()
    {
        miniatureWorld = MiniatureWorld.Instance;
    }
    
    private void Update()
    {
        transform.position = Player.Instance.InteractionHandRight.position;
        transform.rotation = Player.Instance.InteractionHandRight.rotation;
        
        // 왼손의 손바닥이 하늘을 가르킬 경우 활성화 O
        float upAngle = Vector3.Angle(transform.forward, Vector3.up);
        float headAngle = Vector3.Angle(transform.up, Player.Instance.MainCamera.transform.forward);
        if (upAngle < upAngleThreashold && headAngle < headAngleThreashold)
            Hide();
        else
            Show();

        // 활성화된 상태에서, Pinch가 감지될 경우 활성화됨
        if (isShown && paintCursor.DidStartPinch && MiniatureWorld.Instance.Manipulation.canSetROI)
            canMake = true;

        // 3초간 지속될 경우, ROI를 지정할 수 있다
        UpdateSettingROI();
    }

    private void UpdateSettingROI()
    {
        if (canMake)
        {
            timer += Time.deltaTime;
            if (timer >= duration)
            {
                if (!isSetEnd)
                {
                    // 3초가 지났으므로 원하는 작업을 수행합니다.
                    _targetPos = transform.position - transform.up * 0.05f;
                    SettingRoi(_targetPos, transform.right);
                    
                    _markNode = miniatureWorld.GetFirstMarkNode();
                    _satellite = _markNode.GetComponent<MarkNode>().Satellite;
                    _beforePinchPos = paintCursor.transform.position;
                    isSetEnd = true;
                }

                Vector3 dirVector = (paintCursor.transform.position - _beforePinchPos).normalized;
                float dirMagnitude = dirVector.sqrMagnitude;
                if (paintCursor.IsPinching &&  dirMagnitude > 0.9f)
                {
                    // handRayRenderer.SetContactVector(_targetPos); 
                    // TODO: Set하고 있는 portal의 위치로 Ray의 끝이 고정되도록 하고 싶었는데, 의도한 대로 동작이 되지 않는군
                    if (dirVector.y < -0.1f)
                    {
                        //Debug.Log("[DEBUG] Is Pinching & Moving * DOWN * ..." + dirVector);
                        ScaleMarkedSpace(-dirMagnitude);
                        _correctionTimer += Time.deltaTime;
                    }
                    else if (dirVector.y > 0.1f)
                    {
                        //Debug.Log("[DEBUG] Is Pinching & Moving * UP * ..." + dirVector);
                        ScaleMarkedSpace(dirMagnitude);
                        _correctionTimer += Time.deltaTime;
                    }
                    // else if (dirVector.x < -0.1f || dirVector.z < -0.1f)
                    // {
                    //     TranslateMarkedSpace(-dirMagnitude);
                    //     
                    // }
                    // else if (dirVector.x > 0.1f || dirVector.z > 0.1f)
                    // {
                    //     TranslateMarkedSpace(dirMagnitude);
                    // }
                    //_beforePinchPos = paintCursor.transform.position;
                }
                
                if (paintCursor.DidEndPinch)
                {
                    manager_1.SaveCorrectionValue(_correctionTimer);
                        
                    // 작업이 끝났으므로 다시 초기화합니다.
                    canMake = false;
                    timer = 0f;
                    _correctionTimer = 0.0f;
                    isSetEnd = false;
                }
            }
        }
    }

    private void ScaleMarkedSpace(float delta)
    {
        delta = Time.deltaTime * delta;
        _markNode.transform.localScale += new Vector3(delta, delta, delta);
        _markNode.transform.localScale.Clamp(MiniatureWorld.MinMarkSize, MiniatureWorld.MaxMarkSize);


        SyncToSatellite(delta);
    }

    private void SyncToSatellite(float delta)
    {
        Vector3 now = this.transform.localPosition;
        Vector3 move = new Vector3(now.x, now.y + delta, now.z);
        move.Clamp(0.5f, 1.0f);
        _satellite.transform.localPosition = move;
    }

    private void TranslateMarkedSpace(float delta)
    {
        delta = Time.deltaTime * delta;
        Satellite satellite = _markNode.GetComponent<MarkNode>().Satellite;
        satellite.transform.position += new Vector3(delta, delta, delta);
        satellite.TranslateMarkedSpace();
    }

    // private void OnDisable()
    // {
    //     miniatureWorld.gameObject.GetComponent<MiniatureManipulation>().TempEyes.SetActive(false);
    //     miniatureWorld.gameObject.SetActive(false);
    // }
    //
    // private void OnEnable()
    // {
    //     miniatureWorld.gameObject.GetComponent<MiniatureManipulation>().TempEyes.SetActive(true);
    //     miniatureWorld.gameObject.SetActive(true);
    // }

    private void SettingRoi(Vector3 pos, Vector3 rot)
    {
        Vector3 position = miniatureWorld.CandidatePos;
        if (position != Vector3.zero)
        {
            // 해당 위치와 근접하게 이미 Proxy가 존재한다면, 생성하지 않음
            bool canDo = miniatureWorld.CanDeployProxies(position, 2.0f);
            if (canDo)
            {
                switch (TestManager.Instance.experimentNum)
                {
                    case 1:
                        //Debug.Log("[MakeRoi.cs] Can Set ROI!");
                        // [TASK01] 실험군
                        //miniatureWorld.CreateProxies(index, position, 200.0f, miniatureWorld.transform.position);
                        manager_1 = TestManager.Instance.GetTestManager().GetComponent<Test01_Manager>();

                        int index1 = manager_1.portalIndex;
                        Vector3 place1 = manager_1.portalPlaces.transform.GetChild(index1).position;
                        miniatureWorld.CreateProxies((uint)index1, position, 200.0f, place1);
                        miniatureWorld.CreateSatellite((uint)index1, miniatureWorld.CandidateBeforePos);
                        //index++;

                        // [TASK01] 실험군
                        manager_1.SaveCreationValue();
                        manager_1.SaveTargetSetDistance(position);
                        break;
                    
                    case 2:
                        Test02_Manager manager_2 = TestManager.Instance.GetTestManager().GetComponent<Test02_Manager>();
                        int index2 = manager_2.portalIndex;
                        Vector3 place2 = manager_2.portalPlaces.transform.GetChild(index2).position;
                        miniatureWorld.CreateProxies((uint)index2, position, 200.0f, place2);
                        miniatureWorld.CreateSatellite((uint)index2, miniatureWorld.CandidateBeforePos);
                        //index2++;

                        // TODO: [TASK02] 실험 결과 데이터 저장하기
                        manager_2.SaveTargetSetNum();
                        manager_2.SaveTargetSetDistance(position);
                        break;
                }
            }
        }
    }
    
    public void Hide()
    {
        if (!isShown) return;
        foreach (Transform child in transform)
        {
            child.transform.DOScale(0, 0.2f);
            child.gameObject.SetActive(false);
        }
        // transform.GetChild(0).transform.DOScale(0, 0.2f);
        // transform.GetChild(0).gameObject.SetActive(false);

        isShown = false;
    }

    public void Show()
    {
        if (isShown) return;
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(true);
            child.transform.DOScale(1f, 0.2f);
        }
        // transform.GetChild(0).gameObject.SetActive(true);
        // transform.GetChild(0).transform.DOScale(1f, 0.2f);

        isShown = true;
    }
}
