using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Leap.Unity;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine.PlayerLoop;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

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

public class TestManager : MonoBehaviour
{
    [Header("----+ Test Information +----")]
    public int subjectNum; //실험자 번호
    public int taskNum = 1;

    [Header("----+ Test Setting +----")] public TaskType currentType = TaskType.TestGroup;
    public uint MaxTaskTryNum = 1; //실험 최대 시도 횟수
    public GameObject TestPanel;
    public GameObject targetObject;
    [Header("Technique")] public MakeRoi testInteraction;
    public MakeRayPortal controlInteraction;
    public GameObject indicator_A;
    public GameObject indicator_B;
    
    [Header("----+ Test ING +----")] public uint taskTryNum = 0; //실험 시도 횟수
    public TestState state = TestState.NotStarted;
    public float _totalTime = 0.0f;
    public float _thisTime = 0.0f;

    [HideInInspector] public TextMeshProUGUI TitleTextUI;
    [HideInInspector] public TextMeshProUGUI ContentsTextUI;
    [HideInInspector] public Text ButtonTextUI;

    // 시간을 재거나, 기록을 위한 Boolean
    [HideInInspector] public bool IsTickTotalTime = false;
    [HideInInspector] public bool IsTickThisTime = false;
    [HideInInspector] public bool IsTestRecordEnd = false;

    private static TestManager instance;
    public static TestManager Instance
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
    
    public void SetGameObjects()
    {
        TitleTextUI = TestPanel.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        ContentsTextUI = TestPanel.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
        ButtonTextUI = TestPanel.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>();
    }

    public  virtual GameObject GetTestManager()
    {
        return this.gameObject;
    }
    
    /// <summary>
    /// Test Panel의 Button을 눌러 Test의 Task를 진행합니다.
    /// </summary>
    [ContextMenu("GoNextState")]
    public void PressButtonUI()
    {
        state = (TestState)(Convert.ToInt32(state + 1) % System.Enum.GetValues(typeof(TestState)).Length);
        SetMeasuresByTestState();
        SetByTestState();
    }

    public virtual void SetByTestState()
    {
        switch (state)
        {
            case TestState.NotStarted:
                TitleTextUI.text = "Start Task";
                ContentsTextUI.text = "파란구역 내에 있는 물체를 \n빨간구역으로 옮기세요.\n총" + taskTryNum + "/" + MaxTaskTryNum;
                ButtonTextUI.text = "YES";
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
                TitleTextUI.text = "Finish " + (taskTryNum) + " try";
                ContentsTextUI.text = "다음으로 진행합니다. \n클릭";
                ButtonTextUI.text = "CLICK";
                break;
        }
    }

    public virtual void SetMeasuresByTestState()
    {
        switch (state)
        {
            case TestState.NotStarted:
                break;

            case TestState.SettingPortal_A:
                break;

            case TestState.FinishPortalSet_A:
                break;

            case TestState.SettingPortal_B:
                break;

            case TestState.FinishPortalSet_B:
                break;

            case TestState.MoveObject:
                break;
        }
    }
    public virtual void ChangeTaskType()
    {
        currentType = (TaskType)(Convert.ToInt32(currentType + 1) % System.Enum.GetValues(typeof(TaskType)).Length);
        ShowInteraction(currentType);
    }
    
    [ContextMenu("SetTarget")]
    public void InitalizeThisTry()
    {
        GameObject[] tempObj = GameObject.FindGameObjectsWithTag("Target");
        if (tempObj.Length > 0)
            foreach (var obj in tempObj)
            {
                Destroy(obj);
            }

        SetTargetValue();
        ShowInteraction(currentType);

        switch (currentType)
        {

            case TaskType.TestGroup:
                Debug.Log("[TEST01] 실험군 Try Setting 완료");
                MiniatureWorld.Instance.gameObject.transform.position = new Vector3(0.002f, 1.11f, 1.963f);
                MiniatureWorld.Instance.RemoveProxies();
                break;

            case TaskType.ControlGroup:
                Debug.Log("[TEST01]대조군 Try Setting 완료");
                MiniatureWorld.Instance.gameObject.transform.position = new Vector3(0.0f, -10.0f, 0.0f);
                MiniatureWorld.Instance.RemoveProxies();
                break;

        }
    }

    private void ShowInteraction(TaskType type)
    {
        switch (type)
        {
            case TaskType.TestGroup: //실험군
                testInteraction.gameObject.SetActive(true);
                controlInteraction.gameObject.SetActive(false);
                break;

            case TaskType.ControlGroup: //대조군
                testInteraction.gameObject.SetActive(false);
                controlInteraction.gameObject.SetActive(true);
                break;
        }
    }

    private void SetTargetValue()
    {
        float target_xValue = 0.0f;
        float target_yValue = 0.0f;
        float target_zValue = 0.0f;
        target_xValue = 0.0f;
        target_xValue = 0.0f;
        float timeSeed = Time.time * 100f;
        Random.InitState((int)timeSeed);

        List<float> xCandidate = new List<float>() { -4.8f, 0.0f, 4.8f };

        // A와 Target Set
        int tempA = Random.Range(0, 3);
        target_xValue = xCandidate[tempA];
        target_yValue = 0.022f;
        target_zValue = Random.Range(8.6f, 14.8f);
        indicator_A.transform.position = new Vector3(target_xValue, target_yValue, target_zValue);
        //targetObject.transform.position = new Vector3(target_xValue, 0.15f, target_zValue);
        Instantiate(targetObject, new Vector3(target_xValue, 0.15f, target_zValue), quaternion.identity);


        // B Set
        timeSeed = Time.time * 100f;
        Random.InitState((int)timeSeed);

        int tempB = -1;
        do
        {
            tempB = Random.Range(0, 3);
        } while (tempB == tempA);

        target_xValue = xCandidate[tempB];
        target_yValue = 0.022f;
        target_zValue = Random.Range(8.6f, 14.8f);
        indicator_B.transform.position = new Vector3(target_xValue, target_yValue, target_zValue);
    }

    public void TickTime()
    {
        if (IsTickTotalTime)
            _totalTime += Time.deltaTime;

        if (IsTickThisTime)
            _thisTime += Time.deltaTime;

    }

}
