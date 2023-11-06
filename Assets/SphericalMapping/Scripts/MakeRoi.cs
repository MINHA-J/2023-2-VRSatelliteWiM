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
    public GameObject pinPrefab;
    
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
        //miniatureWorld = MiniatureWorld.Instances.First();
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
        if (isShown && paintCursor.DidStartPinch)
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
        // GameObject Roi = Instantiate(pinPrefab);
        // Roi.transform.DOScale(4f, 0.2f);
        // Roi.transform.position = pos;
        // Roi.transform.rotation = Quaternion.Euler(rot);

        Vector3 position = miniatureWorld.CandidatePos;
        if (position != Vector3.zero)
        {
            // 해당 위치와 근접하게 이미 Proxy가 존재한다면, 생성하지 않음
            bool canDo = miniatureWorld.CanDeployProxies(position, 10.0f);
            if (canDo)
            {
                Debug.Log("[MakeRoi.cs] Can Set ROI!");
                miniatureWorld.CreateProxies(index, position, 200.0f, miniatureWorld.transform.position);
                miniatureWorld.CreateSatellite(index, miniatureWorld.CandidateBeforePos);
                index++;
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

        isShown = true;
    }
}
