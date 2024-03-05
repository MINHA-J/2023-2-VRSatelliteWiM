using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class HandWiMTeleport : MonoBehaviour
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

    private GameObject _player;
    
    void Start()
    {
        miniatureWorld = MiniatureWorld.Instance;
        _player = GameObject.FindWithTag("Player");
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
                SettingTeleport(transform.position - transform.up * 0.05f, transform.right);

                miniatureWorld.gameObject.transform.position =
                    _player.transform.position +
                    new Vector3(0.00200000009f, 0.888000011f, 0.237000003f);
                miniatureWorld.gameObject.transform.rotation = Quaternion.identity;
                
                // 작업이 끝났으므로 다시 초기화합니다.
                canMake = false;
                timer = 0f;
            }
        }
    }

    private void SettingTeleport(Vector3 pos, Vector3 rot)
    {
        Vector3 position = miniatureWorld.CandidatePos;
        if (position != Vector3.zero)
        {
            Test02_Manager manager_2 = TestManager.Instance.GetTestManager().GetComponent<Test02_Manager>();
            // int index2 = manager_2.portalIndex;
            // miniatureWorld.CreateProxies((uint)index2, position, 200.0f,
            //     manager_2.portalPlaces.transform.position);
            // miniatureWorld.CreateSatellite((uint)index2, miniatureWorld.CandidateBeforePos);
            // index2++;
            _player.transform.position = new Vector3(position.x, 0.0f, position.z);

            // [TASK02] 실험 결과 데이터 저장하기
            manager_2.SaveTargetSetNum();
            manager_2.SaveTargetSetDistance(position);
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
