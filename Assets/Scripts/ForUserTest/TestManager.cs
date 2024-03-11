using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using Leap.Unity;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using Unity.Mathematics;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine.PlayerLoop;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public enum TaskGroupType
{
    TestGroup,      //실험군   - Satellte(WiM & Portal)
    ControlGroup1,  //대조군1  - Hand Ray & Portal
    ControlGroup2,  //대조군2  - Hand Portal
    ControlGroup3   //대조군3  - Controller Ray & Portal
}

public enum TestState
{
    NotStarted = 0,
    SettingTarget_A = 1,
    FinishSet_A = 2,
    SettingTarget_B = 3,
    FinishSet_B = 4,
    MoveObject = 5,
    EndThisTry = 6
}

public class TestManager : MonoBehaviour
{
    [Header("----+ Test Information +----")]
    public int subjectNum; //실험자 번호
    public int experimentNum = 1; // 1(Near)번 or 2(Far)번 실험
    public TestData currentTestData = new TestData();
    public string fileName = "subject";
    public bool doShowImages = true;
    
    [FormerlySerializedAs("currentType")] [Header("----+ Test Setting +----")]
    public TaskGroupType currentGroupType = TaskGroupType.TestGroup;

    public uint repeatTryNum = 3; //실험 최대 시도 횟수
    public GameObject TestPanel;
    public GameObject TestImagePanel;
    public GameObject targetObject;
    [HideInInspector] public GameObject player;
    [HideInInspector] public uint maxPortalNum = 2;

    [Header("Technique")] public GameObject techniques;

    //public MakeRoi testInteraction;
    //public MakeRayPortal controlInteraction;
    public GameObject indicator_A;
    public GameObject indicator_B;

    [Header("----+ Test ING +----")] public uint currentTryNum = 0; //실험 시도 횟수
    public TestState state = TestState.NotStarted;
    public float _totalTime = 0.0f;
    public float _thisTime = 0.0f;
    public TaskTry currentTry;
    public int portalIndex = 0; // A, B 중 어디를 위한 Portal일까

    // 각 기술 시도 횟수 저장을 위한 데이터
    public uint[] totalTryNum = { 0, 0, 0, 0 };

    [HideInInspector] public TextMeshProUGUI TitleTextUI;
    [HideInInspector] public TextMeshProUGUI ContentsTextUI;
    [HideInInspector] public Text ButtonTextUI;

    // 시간을 재거나, 기록을 위한 Boolean
    [HideInInspector] public bool IsTickTotalTime = false;
    [HideInInspector] public bool IsTickThisTime = false;
    [HideInInspector] public bool IsTestRecordEnd = false;

    private GameObject _nasaTlxUI;
    private GameObject _choice;

    [Serializable]
    public struct TestData
    {
        public int subjectNum;
        public List<TaskGroupType> taskType;
        public List<uint> targetPosition;
    }

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

    public void LoadSubjectTestData()
    {
        CSVImporter csvWave = new CSVImporter();
        if (!csvWave.OpenFile("Data/"+fileName))
        {
            Debug.Log("Read File Error");
            return;
        }

        csvWave.ReadHeader();
        string line = csvWave.Readline();
        
        while (line != null)
        {
            string[] elems = line.Split(',');

            if (elems[0] == "" || int.Parse(elems[0]) > subjectNum)
            {
                break;
            }

            if (int.Parse(elems[0]) == subjectNum)
            {
                currentTestData.subjectNum = int.Parse(elems[0]);
                currentTestData.taskType.Add((TaskGroupType)Enum.Parse(typeof(TaskGroupType), elems[1]));
                currentTestData.targetPosition.Add(uint.Parse(elems[2]));
            }
            
            line = csvWave.Readline();
        }

        Debug.Log("[DATA] Load Subject Data");
    }

    public void UpdateTestPanel()
    {
        Vector3 newPos = player.transform.position + player.transform.forward * 0.3f;
        TestPanel.transform.position =
            new Vector3(newPos.x, newPos.y + 1.26f, newPos.z) + player.transform.right * 0.3f;
        
        newPos = player.transform.position + player.transform.forward * 7.0f;
        TestImagePanel.transform.position = new Vector3(newPos.x, 1.43f, newPos.z);
        
        TestPanel.transform.LookAt(new Vector3(
            player.transform.position.x,
            TestPanel.transform.position.y,
            player.transform.position.z));
        TestImagePanel.transform.LookAt(new Vector3(
            player.transform.position.x,
            TestImagePanel.transform.position.y,
            player.transform.position.z));
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
        SetMeasures();
        SetTestPanel();
    }

    public virtual void SetTestPanel()
    {
        switch (state)
        {
            case TestState.NotStarted:
                Debug.Log("실험자" + subjectNum + ", " +
                          experimentNum + " test/" + currentGroupType + "/" + currentTryNum + "번째 Try.");
                TitleTextUI.text = "Start Task";
                ContentsTextUI.text = "파란구역 내에 있는 물체를 \n빨간구역으로 옮기세요.\n총" + currentTryNum + "/" + repeatTryNum;
                ButtonTextUI.text = "YES";
                break;

            case TestState.SettingTarget_A:
                TitleTextUI.text = "Set Portal/Teleport to Blue";
                ContentsTextUI.text = "파란구역으로 Portal을 생성하세요. \n 원하는 대로 Portal이 세팅되었다면 클릭";
                ButtonTextUI.text = "CLICK";
                break;

            case TestState.FinishSet_A:
                TitleTextUI.text = "Finish Portal to Blue";
                ContentsTextUI.text = "파란구역의 Portal에서 물건을 꺼내놓으세요. \n완료 후 클릭";
                ButtonTextUI.text = "CLICK";
                break;

            case TestState.SettingTarget_B:
                TitleTextUI.text = "Set Portal to Red";
                ContentsTextUI.text = "빨간구역으로 Portal을 생성하세요. \n 원하는 대로 Portal이 세팅되었다면 클릭";
                ButtonTextUI.text = "CLICK";
                break;

            case TestState.FinishSet_B:
                TitleTextUI.text = "Finish Portal to Red";
                ContentsTextUI.text = "빨간구역의 Portal을 통해 물건을 옮겨놓으세요. \n완료 후 클릭";
                ButtonTextUI.text = "CLICK";
                break;

            case TestState.MoveObject:
                TitleTextUI.text = "Object 옮기기";
                ContentsTextUI.text = "빨간구역에 물건을 옮겨놓으세요. \n완료 후 클릭";
                ButtonTextUI.text = "CLICK";
                break;

            case TestState.EndThisTry:
                TitleTextUI.text = "Finish " + (currentTryNum) + " try";
                ContentsTextUI.text = "다음으로 진행합니다. \n클릭";
                ButtonTextUI.text = "CLICK";
                break;
        }
    }

    public virtual void SetMeasures()
    {

    }

    public void Question_NasaTLX()
    {
        Debug.Log("[RESULT] NASA TLX 조사를 시작합니다");
        TestPanel.SetActive(false);

        GameObject _UIprefab = Resources.Load<GameObject>("Prefabs/NASA XTL Question UI");
        _nasaTlxUI = Instantiate(_UIprefab, 
            player.transform.position + Vector3.forward * 0.4f + Vector3.up * 1.2f,
            Quaternion.identity);
        // player.SetActive(false);
        // SceneManager.LoadScene("After_NASA_TLX", LoadSceneMode.Additive);
    }

    public void Question_Choice()
    {
        Destroy(_nasaTlxUI); // 이전에 수행한 NasaTLX 삭제하기
        Debug.Log("[RESULT] 객관식 설문 조사를 시작합니다");
        TestPanel.SetActive(false);
        
        GameObject _UIprefab = Resources.Load<GameObject>("Prefabs/Choice Question UI");
        _choice = Instantiate(_UIprefab, 
            player.transform.position + Vector3.forward * 0.4f + Vector3.up * 1.2f,
        Quaternion.identity);

    }
    
    public void BackToTask()
    {
        Debug.Log("[RESULT] 다시 Main Test으로");
        TestPanel.SetActive(true);
        //Destroy(_nasaTlxUI);
        Destroy(_choice);
    }
    

    
    public virtual void ChangeTaskType()
    {
        switch (experimentNum)
        {
            case 1:        
                currentGroupType = (TaskGroupType)(Convert.ToInt32(currentGroupType + 1) % System.Enum.GetValues(typeof(TaskGroupType)).Length);
                break;
            
            case 2:
                currentGroupType = (TaskGroupType)(Convert.ToInt32(currentGroupType + 1) % 2);
                break;
        }
        
        ShowInteraction(currentGroupType);
    }

    public void ShowImage(TaskGroupType groupType)
    {
        TestImagePanel.SetActive(true);
        
        for (int index = 0; index < TestImagePanel.transform.GetChild(1).childCount; index++)
        {
            TestImagePanel.transform.GetChild(1).GetChild(index).gameObject.SetActive(false);
        }
        
        TestImagePanel.transform.GetChild(1).GetChild((int)groupType).gameObject.SetActive(true);
    }
    
    [ContextMenu("SetTarget")]
    public virtual void InitalizeThisTry()
    {
        GameObject[] tempObj = GameObject.FindGameObjectsWithTag("Target");
        if (tempObj.Length > 0)
            foreach (var obj in tempObj)
            {
                Destroy(obj);
            }

        currentGroupType = currentTestData.taskType[(int)currentTryNum]; //Data에 일치하는 순서로
        SetTargetValue();
        ShowInteraction(currentGroupType);

        if (doShowImages && totalTryNum[(int)currentGroupType] % 3 == 0)
            ShowImage(currentGroupType);

        techniques.SetActive(false);
    }

    public void ShowInteraction(TaskGroupType groupType)
    {
        for (int index = 0; index < techniques.transform.childCount; index++)
        {
            techniques.transform.GetChild(index).gameObject.SetActive(false);
        }
        
        techniques.transform.GetChild((int)groupType).gameObject.SetActive(true);
    }

    public void HideInteraction()
    {
        for (int index = 0; index < techniques.transform.childCount; index++)
        {
            techniques.transform.GetChild(index).gameObject.SetActive(false);
        }
        
        techniques.SetActive(false);
    }

    public virtual void SetTargetValue()
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
