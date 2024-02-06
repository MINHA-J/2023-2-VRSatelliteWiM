using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Leap.Unity;
using System.Runtime.Serialization.Formatters.Binary;
using TMPro;
using Unity.Mathematics;
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

public class Test01_Manager : MonoBehaviour
{
    [Header("Set Before Test")]
    public int subjectNum; //실험자 번호
    private int taskNum = 1;
    [SerializeField] private TaskType currentType = TaskType.TestGroup;
    
    [Header("Set Test Interaction")] 
    public MakeRoi testInteraction;
    public MakeRayPortal controlInteraction;
    
    [Header("Set GameObject Before Test")] 
    public Transform portalPlace;
    public GameObject targetObject;
    public GameObject indicator_A;
    public GameObject indicator_B;
    public TextMeshProUGUI TitleTextUI;
    public TextMeshProUGUI ContentsTextUI;
    public Text ButtonTextUI;

    [Header("Doing Test")]     
    [SerializeField] private uint taskTryNum = 0;  //실험 시도 횟수
    [SerializeField] private TestState state = TestState.NotStarted;
    [SerializeField] private float _totalTime = 0.0f;
    [SerializeField] private float _thisTime = 0.0f;
    
    private uint MaxTaskTryNum = 1;  //실험 최대 시도 횟수
    private bool IsTickTotalTime = false;
    private bool IsTickThisTime = false;
    private bool IsTestEnd = false;
    private float target_xValue = 0.0f;
    private float target_yValue = 0.0f;
    private float target_zValue = 0.0f;

    // 1) 실험에 총 걸린 시간을 Check하기 위함 ( TryNum, Time )
    private Dictionary<uint, float> totalTime = new Dictionary<uint, float>();
    
    // 2) Try 중 Portal 생성에 걸린 시간을 Check 하기 위함
    private Dictionary<uint, List<float>> portalCreationTime = new Dictionary<uint, List<float>>();
    
    // 3) Try 중 Portal 생성 당시의 시간을 저장하기 위함
    private Dictionary<uint, List<float>> portalASave = new Dictionary<uint, List<float>>();
    private Dictionary<uint, List<float>> portalBSave = new Dictionary<uint, List<float>>();

    // 4) Task 수행 중 Portal 생성 Distance
    private Dictionary<uint, List<float>> portalADistance = new Dictionary<uint, List<float>>();
    private Dictionary<uint, List<float>> portalBDistance = new Dictionary<uint, List<float>>();

    
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
        SetByTestState();
        InitalizeThisTry();
    }

    private void Update()
    {
        if (IsTickTotalTime)
            _totalTime += Time.deltaTime;

        if (IsTickThisTime)
            _thisTime += Time.deltaTime;

        // 0, 1, 2 try까지만 확인
        if (!IsTestEnd && taskTryNum >= MaxTaskTryNum)
        {
            CheckResult();
            ChangeTaskType();
            IsTestEnd = true;
        }
    }

    private void ChangeTaskType()
    {
        currentType = (TaskType)(Convert.ToInt32(currentType + 1) % System.Enum.GetValues(typeof(TaskType)).Length);
        ShowInteraction(currentType);
        initalizeDictionary();
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
    private void initalizeDictionary()
    {
        taskTryNum = 0;
        totalTime.Clear();
        portalCreationTime.Clear();
        portalASave.Clear();
        portalBSave.Clear();
        portalADistance.Clear();
        portalBDistance.Clear();
        
        IsTestEnd = false;
    }

    private void SetTargetValue()
    {
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

    /// <summary>
    /// Test Panel의 Button을 눌러 Test의 Task를 진행합니다.
    /// </summary>
    public void PressButtonUI()
    {
        state = (TestState)(Convert.ToInt32(state + 1) % System.Enum.GetValues(typeof(TestState)).Length);
        
        SetMeasuresByTestState();
        SetByTestState();
    }

    public void SavePortalNum()
    {
        if (state == TestState.SettingPortal_A)
        {
            if (portalASave.TryGetValue(taskTryNum, out List<float> temp))
            {
                temp.Add(_thisTime);
            }
            else
            {
                List<float> timeList = new List<float>();
                timeList.Add(_thisTime);
                portalASave.Add(taskTryNum, timeList);
            }
        }
        else if (state == TestState.SettingPortal_B)
        {
            if (portalBSave.TryGetValue(taskTryNum, out List<float> temp))
            {
                temp.Add(_thisTime);
            }
            else
            {
                List<float> timeList = new List<float>();
                timeList.Add(_thisTime);
                portalBSave.Add(taskTryNum, timeList);
            }
        }
        else return;
    }

    public void SavePortalDistance(Vector3 portalPos)
    {
        if (state == TestState.SettingPortal_A)
        {
            float distance = Vector3.Distance(portalPos, indicator_A.transform.position);
            if (portalADistance.TryGetValue(taskTryNum, out List<float> temp))
            {
                temp.Add(distance);
            }
            else
            {
                List<float> value = new List<float>();
                value.Add(distance);
                portalADistance.Add(taskTryNum, value);
            }
        }
        else if (state == TestState.SettingPortal_B)
        {
            float distance = Vector3.Distance(portalPos, indicator_B.transform.position);
            if (portalBDistance.TryGetValue(taskTryNum, out List<float> temp))
            {
                temp.Add(distance);
            }
            else
            {
                List<float> value = new List<float>();
                value.Add(distance);
                portalBDistance.Add(taskTryNum, value);
            }
        }
        else return;
    }

    private void SetByTestState()
    {
        switch (state)
        {
            case TestState.NotStarted:
                TitleTextUI.text = "Start Task";
                ContentsTextUI.text = "파란구역 내에 있는 물체를 \n빨간구역으로 옮기세요.\n총" + (MaxTaskTryNum - taskTryNum) + "회 남았습니다.";
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

    private void SetMeasuresByTestState()
    {
        switch (state)
        {
            case TestState.NotStarted:
                InitalizeThisTry();
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
                portalTime.Add(_thisTime); //portalTime[0] = A portal time
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
                    list.Add(_thisTime); //portalTime[1] = B portal time
                break;  

            case TestState.MoveObject:
                // Trigger시에 자동으로 현재 Try의 Total Time 측정 종료, 기록
                IsTickTotalTime = false;
                totalTime.Add(taskTryNum, _totalTime);
                taskTryNum++;
                break;
        }
    }

    [ContextMenu("SetTarget")]
    public void InitalizeThisTry()
    {
        GameObject[] tempObj = GameObject.FindGameObjectsWithTag("Target");
        if (tempObj.Length > 0)
            foreach (var obj in tempObj) { Destroy(obj); }
        
        SetTargetValue();
        ShowInteraction(currentType);

        switch (currentType)
        {

            case TaskType.TestGroup:
                Debug.Log("실험군 Try Setting 완료");
                MiniatureWorld.Instance.gameObject.transform.position = new Vector3(0.002f, 1.11f, 1.963f);
                MiniatureWorld.Instance.RemoveProxies();
                break;
            
            case TaskType.ControlGroup:
                Debug.Log("대조군 Try Setting 완료");
                MiniatureWorld.Instance.gameObject.transform.position = new Vector3(0.0f, -10.0f, 0.0f);
                MiniatureWorld.Instance.RemoveProxies();
                break;


        }
    }

    private void CheckResult()
    {
        Debug.Log("[RESULT] Save Data Start");

        for (uint tryNum = 0; tryNum < MaxTaskTryNum; tryNum++)
        {
            TaskTry taskResult = new TaskTry();

            
            if (totalTime.TryGetValue(tryNum, out float time))
            {
                // 1) 실험에 총 걸린 시간
                taskResult.tryNum = (tryNum + 1);
                taskResult.totalTime = time;

                // 2) 포탈 생성에 걸린 시간
                if (portalCreationTime.TryGetValue(tryNum, out List<float> creationTime))
                {
                    taskResult.portalACreationTime = creationTime[0];
                    taskResult.portalBCreationTime = creationTime[1];
                }

                // 3) 각 포탈 생성 횟수와 시간
                if (portalASave.TryGetValue(tryNum, out List<float> A_createList))
                    taskResult.portalACreationNum = A_createList.Count;
                if (portalBSave.TryGetValue(tryNum, out List<float> B_createList))
                    taskResult.portalBCreationNum = B_createList.Count;

                // 4) 각 포탈 생성 거리
                float sumDistance = 0.0f;
                if (portalADistance.TryGetValue(tryNum, out List<float> A_distance))
                {
                    for (int i = 0; i < A_distance.Count; i++)
                        sumDistance += A_distance[i];
                    taskResult.portalADistance = sumDistance / (float)A_distance.Count;
                }

                if (portalBDistance.TryGetValue(tryNum, out List<float> B_distance))
                {
                    sumDistance = 0.0f;
                    for (int i = 0; i < B_distance.Count; i++)
                        sumDistance += B_distance[i];
                    taskResult.portalBDistance = sumDistance / (float)B_distance.Count;
                }
            }
            Save(tryNum + 1, taskResult);
        }
    }

    public void Save(uint tryNum, TaskTry saveData)
    {
        
        string name = "Test01_Subject" + subjectNum + "_" + currentType + "_Try_" + tryNum;
        
        //ToJson 부분
        string jsonData = JsonUtility.ToJson(saveData, true);

        string path = Application.dataPath + "/DataSave";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        File.WriteAllText(path + "/" + name + ".txt", jsonData);

        //FromJson 부분
        string fromJsonData = File.ReadAllText(path + "/" + name + ".txt");

        TaskTryList TaskTryFromJson = new TaskTryList();
        TaskTryFromJson = JsonUtility.FromJson<TaskTryList>(fromJsonData);
        

        //Binary 형태로 저장함
        // if(!System.IO.Directory.Exists(Application.persistentDataPath + "/Save"))
        // {
        //     System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/Save");
        // }
        //
        // string path = Application.persistentDataPath + "/Save/s_" + subjectNum;
        // System.IO.File.Create(path).Close();
        // System.IO.FileStream fStream = System.IO.File.Open(path, System.IO.FileMode.OpenOrCreate);
        //
        // BinaryFormatter formatter = new BinaryFormatter();
        // formatter.Serialize(fStream, saveData);
        // fStream.Close();
    }
    
}
