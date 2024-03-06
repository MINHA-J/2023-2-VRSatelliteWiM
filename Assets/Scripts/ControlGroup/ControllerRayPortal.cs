using System;
using UnityEngine;
using DG.Tweening;
using Leap.Unity.Preview.HandRays;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class ControllerRayPortal : MonoBehaviour
{
    [Header("Basic")]
    public GameObject rightHand;

    [Header("Setting")]
    [SerializeField] private MiniatureWorld miniatureWorld;
    [SerializeField] private float timer  = 0.0f;
    private float duration  = 1.5f;
    private XRController _xrController;
    private XRRayInteractor _xrRay;
    
    private Vector3 _targetPos;
    private Vector3 _beforeHandPos;
    private MarkNode _markNode;

    private bool isPressed = false;
    private bool isSetEnd = false;

    private Vector3[] contactPoints;
    
    private void Start()
    {
        miniatureWorld = MiniatureWorld.Instance;

        _xrController = rightHand.GetComponent<XRController>();
        _xrRay = rightHand.GetComponent<XRRayInteractor>();
    }

    private void Update()
    {
        UpdatePressAButton();
        UpdateSettingROI();
    }

    private void UpdatePressAButton()
    {
        if (_xrController.inputDevice.TryGetFeatureValue(CommonUsages.primaryButton, out bool button))
        {
            isPressed = button;
            if (button)
            {
                timer += Time.deltaTime;
            }
            else
            {
                timer = 0.0f;
                isSetEnd = false;
            }
        }
    }

    private void UpdateSettingROI()
    {
        if (timer >= duration)
        {
            if (!isSetEnd) // 한번만 실행되도록...
            {
                _xrRay.TryGetCurrent3DRaycastHit(out RaycastHit hit);
                Vector3 contactVector = hit.point;
                
                _targetPos = contactVector - transform.up * 0.05f;
                SettingRoi(_targetPos, transform.right);

                _markNode = miniatureWorld.GetFirstMarkNode();
                _beforeHandPos = rightHand.transform.position;
                isSetEnd = true;
            }
            
            Vector3 dirVector = (rightHand.transform.position - _beforeHandPos).normalized;
            float dirMagnitude = dirVector.sqrMagnitude;
            if (isPressed &&  dirMagnitude > 0.9f)
            {
                // handRayRenderer.SetContactVector(_targetPos); 
                // TODO: Set하고 있는 portal의 위치로 Ray의 끝이 고정되도록 하고 싶었는데, 의도한 대로 동작이 되지 않는군
                if (dirVector.y < -0.1f)
                {
                    //Debug.Log("[DEBUG] Is Pinching & Moving * DOWN * ..." + dirVector);
                    ScaleMarkedSpace(-dirMagnitude);
                }
                else if (dirVector.y > 0.1f)
                {
                    //Debug.Log("[DEBUG] Is Pinching & Moving * UP * ..." + dirVector);
                    ScaleMarkedSpace(dirMagnitude);
                }
            }
        }
    }
    
    private void SettingRoi(Vector3 pos, Vector3 rot)
    {
        //Vector3 pos = miniatureWorld.CandidatePos;
        //if (pos != Vector3.zero)

        // 해당 위치와 근접하게 이미 Proxy가 존재한다면, 생성하지 않음
        bool canDo = miniatureWorld.CanDeployProxies(pos, 2.0f);
        if (canDo)
        {
            switch (TestManager.Instance.experimentNum)
            {
                case 1:
                    //Debug.Log("[MakeRoi.cs] Can Set ROI!");
                    // [TASK01] 실험군
                    //miniatureWorld.CreateProxies(index, position, 200.0f, miniatureWorld.transform.position);
                    Test01_Manager manager = TestManager.Instance.GetTestManager().GetComponent<Test01_Manager>();

                    int index1 = manager.portalIndex;
                    Vector3 place1 = manager.portalPlaces.transform.GetChild(index1).position;

                    //miniatureWorld.RemoveProxies();
                    miniatureWorld.CreateProxies((uint)index1, pos, 200.0f, place1);
                    miniatureWorld.CreateSatellite((uint)index1, miniatureWorld.CandidateBeforePos);
                    //index++;

                    // [TASK01] 실험군
                    manager.SaveTargetSetNum();
                    manager.SaveTargetSetDistance(pos);
                    break;

                case 2:
                    // 2번 실험에서는 이거 안 씁니다
                    break;
            }

        }
    }

    private void ScaleMarkedSpace(float delta)
    {
        delta = Time.deltaTime * delta;
        _markNode.transform.localScale += new Vector3(delta, delta, delta);
        _markNode.transform.localScale.Clamp(MiniatureWorld.MinMarkSize, MiniatureWorld.MaxMarkSize);
    }

    private void OnEnable()
    {
        rightHand.SetActive(true);
    }


    private void OnDisable()
    {
        rightHand.SetActive(false);
    }
}
