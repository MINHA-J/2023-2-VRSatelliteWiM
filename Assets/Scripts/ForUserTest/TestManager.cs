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
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public enum TaskType
{
    TestGroup,      //실험군
    ControlGroup1,  //대조군1
    ControlGroup2   //대조군2
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
    public int experimentNum = 1;   // 1(Near)번 or 2(Far)번 실험

    [Header("----+ Test Setting +----")] 
    public TaskType currentType = TaskType.TestGroup;
    public uint repeatTryNum = 3; //실험 최대 시도 횟수
    public GameObject TestPanel;
    public GameObject targetObject;
    [HideInInspector] public GameObject player;
    
    [Header("Technique")] 
    public GameObject techniques;
    //public MakeRoi testInteraction;
    //public MakeRayPortal controlInteraction;
    public GameObject indicator_A;
    public GameObject indicator_B;
    
    [Header("----+ Test ING +----")] 
    public uint currentTryNum = 0; //실험 시도 횟수
    public TestState state = TestState.NotStarted;
    public float _totalTime = 0.0f;
    public float _thisTime = 0.0f;
    public TaskTry currentTry;
    
    [HideInInspector] public TextMeshProUGUI TitleTextUI;
    [HideInInspector] public TextMeshProUGUI ContentsTextUI;
    [HideInInspector] public Text ButtonTextUI;

    // 시간을 재거나, 기록을 위한 Boolean
    [HideInInspector] public bool IsTickTotalTime = false;
    [HideInInspector] public bool IsTickThisTime = false;
    [HideInInspector] public bool IsTestRecordEnd = false;

    private GameObject _nasaTlxUI;
    
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
        if (instance == null)
        { 
            instance = this;
        }
        else
        {
        }
    }
    

    public virtual void SetGameObjects()
    {
        player = GameObject.FindWithTag("Player");
        
        TitleTextUI = TestPanel.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        ContentsTextUI = TestPanel.transform.GetChild(0).GetChild(1).GetComponent<TextMeshProUGUI>();
        ButtonTextUI = TestPanel.transform.GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetComponent<Text>();
    }

    public  virtual GameObject GetTestManager()
    {
        return this.gameObject;
    }

    public void GetKeyboardCommand()
    {
        // 오류 생길 경우 처리하자

        // 1) F1 로 Test state 넘기기
        if (Input.GetKeyDown(KeyCode.F1))
            GoNextTestState();

        // 2)

    }

    /// <summary>
    /// Test Panel의 Button을 눌러 Test의 Task를 진행합니다.
    /// </summary>
    [ContextMenu("GoNextState")]
    public void GoNextTestState()
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
                Debug.Log("실험자" + subjectNum + ", " + 
                          experimentNum + " test/" + currentType + "/" + currentTryNum + "번째 Try.");
                TitleTextUI.text = "Start Task";
                ContentsTextUI.text = "파란구역 내에 있는 물체를 \n빨간구역으로 옮기세요.\n총" + currentTryNum + "/" + repeatTryNum;
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
                TitleTextUI.text = "Finish " + (currentTryNum) + " try";
                ContentsTextUI.text = "다음으로 진행합니다. \n클릭";
                ButtonTextUI.text = "CLICK";
                break;
        }
    }
    
    public void Question_NasaTLX()
    {
        Debug.Log("[RESULT] NASA TLX 조사를 시작합니다");
        TestPanel.SetActive(false);

        GameObject _UIprefab = Resources.Load<GameObject>("Prefabs/NASA XTL Question UI");
        _nasaTlxUI = Instantiate(_UIprefab, 
            player.transform.position + Vector3.forward * 0.3f + Vector3.up * 1.5f,
            Quaternion.identity);
        // player.SetActive(false);
        // SceneManager.LoadScene("After_NASA_TLX", LoadSceneMode.Additive);
    }
    
    public void BackToTask()
    {
        Debug.Log("[RESULT] 다시 설문으로");
        TestPanel.SetActive(true);
        Destroy(_nasaTlxUI);
    }
    
    public virtual void SetMeasuresByTestState()
    {

    }
    
    public virtual void ChangeTaskType()
    {
        switch (experimentNum)
        {
            case 1:        
                currentType = (TaskType)(Convert.ToInt32(currentType + 1) % System.Enum.GetValues(typeof(TaskType)).Length);
                break;
            
            case 2:
                currentType = (TaskType)(Convert.ToInt32(currentType + 1) % 2);
                break;
        }
        
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

        techniques.SetActive(false);
        
        switch (currentType)
        {

            case TaskType.TestGroup:
                Debug.Log("[TEST01] 실험군 Try Setting 완료");
                MiniatureWorld.Instance.gameObject.transform.position = new Vector3(0.002f, 1.5f, 1.747f);
                MiniatureWorld.Instance.RemoveProxies();
                break;

            case TaskType.ControlGroup1:
                Debug.Log("[TEST01]대조군 Try Setting 완료");
                MiniatureWorld.Instance.gameObject.transform.position = new Vector3(0.0f, -10.0f, 0.0f);
                MiniatureWorld.Instance.RemoveProxies();
                break;

        }
    }

    public void ShowInteraction(TaskType type)
    {
        for (int index = 0; index < techniques.transform.childCount; index++)
        {
            techniques.transform.GetChild(index).gameObject.SetActive(false);
        }
        
        techniques.transform.GetChild((int)type).gameObject.SetActive(true);
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
