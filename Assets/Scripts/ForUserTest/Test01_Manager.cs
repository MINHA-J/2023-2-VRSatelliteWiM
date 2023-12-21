using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using Leap.Unity;

public enum TaskType
{
    TestGroup,      //실험군
    ControlGroup    //대조군
}

public enum TestState
{
    NotStarted,
    SettingPortal_A,
    FinishPortalSet_A,
    SettingPortal_B,
    FinishPortalSet_B,
    FinishObjectTask
}

public class Test01_Manager : MonoBehaviour
{
    [Header("Set Before Test")]
    public int subjectNum;                              //실험자 번호
    [SerializeField] private int taskNum = 1;
    public TaskType currentType;
    
    [Header("Set GameObject Before Test")] 
    public Transform portalPlace;
    public HandModelBase RHandModel;
    public HandModelBase LHandModel;
    
    [Header("Doing Test")]     
    public uint taskTryNum = 0;  //실험 시도 횟수
    [SerializeField] private TestState state = TestState.NotStarted;
    [SerializeField] private bool IsPortalSetted = false;
    [SerializeField] private bool IsThisTryEnd = false; // 실험의 Try가 종료되었나요?

    public Time time;
    
    // 1) 실험에 총 걸린 시간을 Check하기 위함 ( TaskNum, {TaskStartTime, TaskendTime} )
    private Dictionary<uint, List<float>> TotalTime = new Dictionary<uint, List<float>>();
    // 2) Task 수행 중 Portal 생성 갯수
    private int portalCreationNum;
    // Save List
    private List<float> Time;
    
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

    

}
