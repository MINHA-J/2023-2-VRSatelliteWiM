using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Test02_Manager : TestManager
{
    [Header("[Test02] GameObject")] 
    public GameObject portalPlaces;
    
    [Space(10.0f)]
    
    [Header("[Test02] Techique (Auto)")] 
    [SerializeField] private MakeRoi testInteraction;
    [SerializeField] private GameObject controlInteraction_1;
    
    // 실험 결과를 저장할 데이터

    private void Start()
    {
        DontDestroyOnLoad(this);
        
        SetGameObjects();
        SetTestPanel();
        InitalizeThisTry();

        experimentNum = 2;
    }
    
    private void Update()
    {
        GetKeyboardCommand(); // 오류 생길 경우 처리
        
        if (state != TestState.NotStarted)
            TickTime();

        // 0, 1, 2 try까지만 확인
        // 다음 technique로 넘어갑니다
        if (!IsTestRecordEnd && currentTryNum >= repeatTryNum)
        {
            // TODO: 한 technique 끝나고 물어볼 설문 진행
            //CheckResult();
            
            ChangeTaskType();
            IsTestRecordEnd = true;
        }
    }
    
    
    public override void SetGameObjects()
    {
        base.SetGameObjects();
        testInteraction = techniques.transform.GetChild(0).GetComponent<MakeRoi>();
    }
}
