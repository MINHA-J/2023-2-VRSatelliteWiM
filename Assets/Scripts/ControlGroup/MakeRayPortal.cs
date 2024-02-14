using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Leap.Unity.Preview.HandRays;

public class MakeRayPortal : MonoBehaviour
{
    /// <summary>
    /// The pinchDetector associated with this cursor
    /// </summary>
    [Header("Basic")]
    public Leap.Unity.PinchDetector pinchDetector;
    public Leap.Unity.Examples.PaintCursor paintCursor;
    
    
    [SerializeField] private bool isShown;
    [SerializeField] private bool canMake = false;
    
    
    [Header("Setting")]
    [SerializeField]private const float upAngleThreashold = 60f;
    [SerializeField]private const float headAngleThreashold = 60f;
    [SerializeField]private MiniatureWorld miniatureWorld;
    [SerializeField]private HandRayRenderer handRayRenderer;
    [SerializeField]private uint index = 0;
    private float timer  = 0.0f;
    private float duration  = 1.5f;
    
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
        if (canMake)
        {
            timer += Time.deltaTime;
            if (timer >= duration)
            {
                Debug.Log("[DEBUG] ROI 설정. Portal Set -MakeRayPortal.cs");
                Vector3 targetPos = handRayRenderer.GetContactVector() - transform.up * 0.05f;
                SettingRoi(targetPos, transform.right);

                // 작업이 끝났으므로 다시 초기화합니다.
                canMake = false;
                timer = 0f;
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
            //Debug.Log("[MakeRoi.cs] Can Set ROI!");
            // [TASK01] 실험군
            //miniatureWorld.CreateProxies(index, position, 200.0f, miniatureWorld.transform.position);
            miniatureWorld.RemoveProxies();
            miniatureWorld.CreateProxies(index, pos, 200.0f, Test01_Manager.Instance.portalPlace.position);
            //miniatureWorld.CreateSatellite(index, miniatureWorld.CandidateBeforePos);
            index++;

            // [TASK01] 실험군
            Test01_Manager.Instance.SavePortalNum();
            Test01_Manager.Instance.SavePortalDistance(pos);
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
