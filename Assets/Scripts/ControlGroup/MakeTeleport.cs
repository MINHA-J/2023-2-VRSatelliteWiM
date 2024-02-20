using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class MakeTeleport : MonoBehaviour
{
    [Header("Basic")]
    public Leap.Unity.PinchDetector pinchDetector;
    public Leap.Unity.Examples.PaintCursor paintCursor;
    
    [SerializeField] private bool isShown;
    [SerializeField] private bool canMake = false;
    
    [Header("Setting")]
    [SerializeField] private MiniatureWorld miniatureWorld;
    [SerializeField] private uint index = 0;
    [SerializeField] private float duration  = 2.0f;
    
    private const float upAngleThreashold = 60f;
    private const float headAngleThreashold = 60f;
    private float timer  = 0.0f;

    
    void Start()
    {
        miniatureWorld = MiniatureWorld.Instance;
    }

    // Update is called once per frame
    void Update()
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
