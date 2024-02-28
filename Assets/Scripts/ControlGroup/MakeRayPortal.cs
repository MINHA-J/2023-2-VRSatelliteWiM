using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Leap.Unity.Preview.HandRays;
using Unity.VisualScripting;

public class MakeRayPortal : MonoBehaviour
{
    /// <summary>
    /// The pinchDetector associated with this cursor
    /// </summary>
    [Header("Basic")]
    public Leap.Unity.PinchDetector pinchDetector;
    public Leap.Unity.Examples.PaintCursor paintCursor;
    
    
    [SerializeField] private bool isShown;  //Pinch 활성화/비활성화
    [SerializeField] private bool canMake = false;
    [SerializeField] private bool isSetEnd = false;
    
    
    [Header("Setting")]
    [SerializeField]private const float upAngleThreashold = 60f;
    [SerializeField]private const float headAngleThreashold = 60f;
    [SerializeField]private MiniatureWorld miniatureWorld;
    [SerializeField]private HandRayRenderer handRayRenderer;
    [SerializeField]private uint index = 0;
    private float timer  = 0.0f;
    private float duration  = 1.5f;

    private Vector3 _targetPos;
    private Vector3 _beforePinchPos;
    private MarkNode _markNode;
    
    void Start()
    {
        miniatureWorld = MiniatureWorld.Instance;
        handRayRenderer = GetComponent<HandRayRenderer>();
    }
    
    void Update()
    {
        transform.position = Player.Instance.InteractionHandRight.position;
        transform.rotation = Player.Instance.InteractionHandRight.rotation;
        
        UpdateCursorVisible();
        
        // Debug.Log("isShown: " + isShown +
        //           ", StartPinch: " + paintCursor.DidStartPinch +
        //           ", IsContact: " + handRayRenderer.IsContact());
        if (isShown && paintCursor.DidStartPinch && handRayRenderer.IsContact())
            canMake = true;
        
        // 2초간 지속될 경우, ROI를 지정할 수 있다
        UpdateSettingROI();
    }

    private void UpdateSettingROI()
    {
        if (canMake)
        {
            timer += Time.deltaTime;
            if (timer >= duration)
            {
                if (!isSetEnd) // 한번만 실행되도록...
                {
                    //Debug.Log("[DEBUG] ROI 설정. Portal Set -MakeRayPortal.cs");
                    _targetPos = handRayRenderer.GetContactVector() - transform.up * 0.05f;
                    SettingRoi(_targetPos, transform.right);

                    _markNode = miniatureWorld.GetFirstMarkNode();
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
                    }
                    else if (dirVector.y > 0.1f)
                    {
                        //Debug.Log("[DEBUG] Is Pinching & Moving * UP * ..." + dirVector);
                        ScaleMarkedSpace(dirMagnitude);
                    }
                }

                if (paintCursor.DidEndPinch)
                {
                    // 작업이 끝났으므로 다시 초기화합니다.
                    canMake = false;
                    timer = 0f;
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
                    Vector3 place = manager.portalPlaces.transform.GetChild((int)index % 2).position;
                    
                    miniatureWorld.RemoveProxies();
                    miniatureWorld.CreateProxies(index, pos, 200.0f, place);
                    //miniatureWorld.CreateSatellite(index, miniatureWorld.CandidateBeforePos);
                    index++;

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

    private void UpdateCursorVisible()
    {
        // 왼손의 손바닥이 하늘을 가르치지 않는다면
        float upAngle = Vector3.Angle(transform.forward, Vector3.up);
        float headAngle = Vector3.Angle(transform.up, Player.Instance.MainCamera.transform.forward);
        if (upAngle < upAngleThreashold && headAngle < headAngleThreashold)
            Hide();
        else
            Show();
    }
    
    public void Hide()
    {
        if (!isShown) return;
        // foreach (Transform child in transform)
        // {
        //     child.transform.DOScale(0, 0.2f);
        //     child.gameObject.SetActive(false);
        // }
        transform.GetChild(0).transform.DOScale(0, 0.2f);
        transform.GetChild(0).gameObject.SetActive(false);

        isShown = false;
    }

    public void Show()
    {
        if (isShown) return;
        // foreach (Transform child in transform)
        // {
        //     child.gameObject.SetActive(true);
        //     child.transform.DOScale(1f, 0.2f);
        // }
        transform.GetChild(0).gameObject.SetActive(true);
        transform.GetChild(0).transform.DOScale(1f, 0.2f);

        isShown = true;
    }
}
