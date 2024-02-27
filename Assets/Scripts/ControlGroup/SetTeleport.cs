using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Leap.Unity.Preview.HandRays;
using UnityEngine;

public class SetTeleport : MonoBehaviour
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
    [SerializeField]private HandRayRenderer handRayRenderer;
    [SerializeField]private uint index = 0;
    private float timer  = 0.0f;
    private float duration  = 1.5f;

    private Vector3 _targetPos;
    private Vector3 _beforePinchPos;
    private GameObject _player;
    
    void Start()
    {
        handRayRenderer = GetComponent<HandRayRenderer>();
        _player = GameObject.FindWithTag("Player").gameObject;
    }
    
    void Update()
    {
        transform.position = Player.Instance.InteractionHandRight.position;
        transform.rotation = Player.Instance.InteractionHandRight.rotation;
        
        UpdateCursorVisible();
        
        if (isShown && paintCursor.DidStartPinch && handRayRenderer.IsContact())
            canMake = true;
        
        // 2초간 지속될 경우, Teleport를 수행할 수 있다
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
                    _targetPos = handRayRenderer.GetContactVector() - transform.up * 0.05f;

                    TeleportToTarget(_targetPos);
                    
                    _beforePinchPos = paintCursor.transform.position;
                    isSetEnd = true;
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

    private void TeleportToTarget(Vector3 target)
    {
        _player.transform.position = target;
        
        // [TASK02] 실험 결과 데이터 저장하기
        Test02_Manager manager_2 = TestManager.Instance.GetTestManager().GetComponent<Test02_Manager>();

        manager_2.SaveTargetSetNum();
        manager_2.SaveTargetSetDistance(target);
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
