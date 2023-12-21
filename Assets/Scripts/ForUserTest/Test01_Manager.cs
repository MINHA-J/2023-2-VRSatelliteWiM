using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using Leap.Unity;
using TMPro;
using UnityEngine.UI;

public enum TaskType
{
    TestGroup,      //실험군
    ControlGroup    //대조군
}

public enum TestState
{
    NotStarted = 0,
    SettingPortal_A = 1,
    FinishPortalSet_A = 2,
    SettingPortal_B = 3,
    FinishPortalSet_B = 4,
    MoveObject = 5
}

public class Test01_Manager : MonoBehaviour
{
    [Header("Set Before Test")]
    public int subjectNum;                              //실험자 번호
    [SerializeField] private int taskNum = 1;
    public TaskType currentType;
    
    [Header("Set GameObject Before Test")] 
    public Transform portalPlace;
    public TextMeshProUGUI TitleTextUI;
    public TextMeshProUGUI ContentsTextUI;
    public Text ButtonTextUI;

    [Header("Doing Test")]     
    [SerializeField] private uint taskTryNum = 0;  //실험 시도 횟수
    [SerializeField] private TestState state = TestState.NotStarted;
    [SerializeField] private float _totalTime = 0.0f;
    [SerializeField] private float _thisTime = 0.0f;
    [SerializeField] private bool IsTickTotalTime = false;
    [SerializeField] private bool IsTickThisTime = false;

    // 1) 실험에 총 걸린 시간을 Check하기 위함 ( TryNum, Time )
    private Dictionary<uint, float> totalTime = new Dictionary<uint, float>();
    // 2) Task 수행 중 Portal 생성 갯수
    private int portalCreationNum;
    // 3) Try 중 Portal 생성에 걸린 시간을 Check 하기 위함
    private Dictionary<uint, List<float>> portalCreationTime = new Dictionary<uint, List<float>>();
    
    [Serializable]
    private struct TaskResultFormat
    {
        
    }

    private static Test01_Manager instance;

    public static Test01_Manager Instance
    {
        get
        {
            if (instance == null)
            {
                return null;
            }

            return instance;
        }
    }

    void Awake()
    {
        if (null == instance)
        { 
            instance = this;
        }
        else
        {
        }
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        if (IsTickTotalTime)
            _totalTime += Time.deltaTime;

        if (IsTickThisTime)
            _thisTime += Time.deltaTime;
    }

    /// <summary>
    /// Test Panel의 Button을 눌러 Test의 Task를 진행합니다.
    /// </summary>
    public void PressButtonUI()
    {
        state = (TestState)(Convert.ToInt32(state + 1) % 6);
        SetByTestState();
        SetMeasuresByTestState();
    }

    private void SetByTestState()
    {
        switch (state)
        {
            case TestState.NotStarted:
                TitleTextUI.text = "Start Task";
                ContentsTextUI.text = "파란구역 내에 있는 물체를 \n빨간구역으로 옮기세요.\n총" + (3 - taskTryNum) + "회 남았습니다.";
                ButtonTextUI.text = "YES";
                // 전체 Time 측정 시작
                // 
                break;
            
            case TestState.SettingPortal_A:
                TitleTextUI.text = "Set Portal to Blue";
                ContentsTextUI.text = "파란구역으로 Portal을 생성하세요. \n 원하는 대로 Portal이 세팅되었다면 클릭";
                ButtonTextUI.text = "CLICK";
                break;
            
            case TestState.FinishPortalSet_A:
                TitleTextUI.text = "Finish Portal to Blue";
                ContentsTextUI.text = "파란구역의 Portal에서 물건을 꺼내놓으세요. \n완료 후 클릭";
                ButtonTextUI.text = "CLICK";
                break;
            
            case TestState.SettingPortal_B:
                TitleTextUI.text = "Set Portal to Red";
                ContentsTextUI.text = "빨간구역으로 Portal을 생성하세요. \n 원하는 대로 Portal이 세팅되었다면 클릭";
                ButtonTextUI.text = "CLICK";
                break;

            case TestState.FinishPortalSet_B:
                TitleTextUI.text = "Finish Portal to Red";
                ContentsTextUI.text = "빨간구역의 Portal을 통해 물건을 옮겨놓으세요. \n완료 후 클릭";
                ButtonTextUI.text = "CLICK";                
                break;  

            case TestState.MoveObject:
                TitleTextUI.text = "Finish " + taskTryNum + " try";
                ContentsTextUI.text = "다음으로 진행합니다. \n클릭";
                ButtonTextUI.text = "CLICK";                   
                break;
        }
    }

    private void SetMeasuresByTestState()
    {
        switch (state)
        {
            case TestState.NotStarted:
                CheckResult();
                taskTryNum++;
                break;

            case TestState.SettingPortal_A:
                // 현재 Try의 Total Time 측정 시작
                _totalTime = 0.0f;
                IsTickTotalTime = true;
                // Portal을 세팅하는데 결리는 Time 측정 시작
                _thisTime = 0.0f;
                IsTickThisTime = true;
                break;

            case TestState.FinishPortalSet_A:
                // Portal을 세팅하는데 결리는 Time 측정 종료, 기록
                IsTickThisTime = false;
                List<float> portalTime = new List<float>();
                portalTime.Add(_thisTime);
                portalCreationTime.Add(taskTryNum, portalTime);
                break;
            
            case TestState.SettingPortal_B:
                // Portal을 세팅하는데 결리는 Time 측정 시작
                _thisTime = 0.0f;
                IsTickThisTime = true;
                break;
            
            case TestState.FinishPortalSet_B:
                // Portal을 세팅하는데 결리는 Time 측정 종료, 기록
                if (portalCreationTime.TryGetValue(taskTryNum, out List<float> list))
                    list.Add(_thisTime);
                break;  

            case TestState.MoveObject:
                // Trigger시에 자동으로 현재 Try의 Total Time 측정 종료, 기록
                IsTickTotalTime = false;
                totalTime.Add(taskTryNum, _totalTime);
                break;
        }
    }

    private void InitalizeThisTry()
    {
        switch (currentType)
        {
            case TaskType.ControlGroup:
                
                break;
            
            case TaskType.TestGroup:
                break;
        }

        taskTryNum++;
        SetWorldThisTry();
    }

    private void SetWorldThisTry()
    {
        
    }

    private void CheckResult()
    {
        foreach (var pair in totalTime)
        {
            Debug.Log("[RESULT] " + (pair.Key+1) + "번째 Task의 Total Time: " + pair.Value);
        }

        foreach (var pair in portalCreationTime)
        {
            Debug.Log("[RESULT] " + (pair.Key+1) + "번째 Task의 1번째 Portal Time" + pair.Value[0]);
            Debug.Log("[RESULT] " + (pair.Key+1) + "번째 Task의 2번째 Portal Time" + pair.Value[1]);
        }
    }
}
