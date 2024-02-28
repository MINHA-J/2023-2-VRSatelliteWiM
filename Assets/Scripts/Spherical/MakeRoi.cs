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
    
    [Header("Setting")]
    [SerializeField]private const float upAngleThreashold = 60f;
    [SerializeField]private const float headAngleThreashold = 60f;
    [SerializeField]private MiniatureWorld miniatureWorld;
    [SerializeField] private uint index = 0;
    private float timer  = 0.0f;
    private float duration  = 2.0f;

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
            Show();
        else
            Hide();

        // 활성화된 상태에서, Pinch가 감지될 경우 활성화됨
        if (isShown && paintCursor.DidStartPinch && MiniatureWorld.Instance.Manipulation.canSetROI)
            canMake = true;

        // 3초간 지속될 경우, ROI를 지정할 수 있다
        if (canMake)
        {
            timer += Time.deltaTime;
            if (timer >= duration)
            {
                // 3초가 지났으므로 원하는 작업을 수행합니다.
                SettingRoi(transform.position - transform.up * 0.05f, transform.right);

                // 작업이 끝났으므로 다시 초기화합니다.
                canMake = false;
                timer = 0f;
            }
        }

    }

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
                        Test01_Manager manager_1 = TestManager.Instance.GetTestManager().GetComponent<Test01_Manager>();

                        int index1 = manager_1.portalIndex;
                        Vector3 place1 = manager_1.portalPlaces.transform.GetChild(index1).position;
                        miniatureWorld.CreateProxies((uint)index1, position, 200.0f, place1);
                        miniatureWorld.CreateSatellite((uint)index1, miniatureWorld.CandidateBeforePos);
                        //index++;

                        // [TASK01] 실험군
                        manager_1.SaveTargetSetNum();
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
