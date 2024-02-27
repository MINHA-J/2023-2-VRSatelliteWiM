using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

public class Test02_Manager : TestManager
{
    [Header("[Test02] GameObject")] 
    public GameObject portalPlaces;
    
    [Space(10.0f)]
    
    [Header("[Test02] Techique (Auto)")] 
    [SerializeField] private MakeRoi testInteraction;
    [SerializeField] private GameObject controlInteraction_1;
    [SerializeField] private GameObject controlInteraction_2;
    
    // 실험 결과를 저장할 데이터
    // 1) 실험에 총 걸린 시간을 Check하기 위함 ( TryNum, Time )
    private Dictionary<uint, float> totalTime = new Dictionary<uint, float>();
    
    // 2) Try 중 Portal 생성에 걸린 시간을 Check 하기 위함
    private Dictionary<uint, List<float>> creationTime = new Dictionary<uint, List<float>>();
    
    // 3) Try 중 Portal 생성 당시의 시간을 저장하기 위함
    private Dictionary<uint, List<float>> targetASave = new Dictionary<uint, List<float>>();
    private Dictionary<uint, List<float>> targetBSave = new Dictionary<uint, List<float>>();

    // 4) Task 수행 중 Portal 생성 Distance
    private Dictionary<uint, List<float>> targetADistance = new Dictionary<uint, List<float>>();
    private Dictionary<uint, List<float>> targetBDistance = new Dictionary<uint, List<float>>();

    // 5) Task 수행 중 Target 상호작용 시간
    private Dictionary<uint, float> movementTime = new Dictionary<uint, float>();
    
    private float target_xValue = 0.0f;
    private float target_yValue = 0.0f;
    private float target_zValue = 0.0f;

    private Vector3 miniatureVec = new Vector3(0.00200000009f, 1.23000002f, 0.349999994f);

    private void Start()
    {
        DontDestroyOnLoad(this);
        
        LoadSubjectTestData();
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

        UpdateTestPanel();
    }
    
    public override void SetGameObjects()
    {
        base.SetGameObjects();
        
        testInteraction = techniques.transform.GetChild(0).GetComponent<MakeRoi>();
        controlInteraction_1 = techniques.transform.GetChild(1).gameObject;
        controlInteraction_2 = techniques.transform.GetChild(2).gameObject;
    }
    
    public override void ChangeTaskType()
    {
        base.ChangeTaskType();
        initalizeDictionary();
    }
    
    private void initalizeDictionary()
    {
        currentTryNum = 0;
        totalTime.Clear();
        creationTime.Clear();
        targetASave.Clear();
        targetBSave.Clear();
        targetADistance.Clear();
        targetBDistance.Clear();
        
        IsTestRecordEnd = false;
    }
    
    public override void InitalizeThisTry()
    {
        base.InitalizeThisTry();

        switch (currentGroupType)
        {
            case TaskGroupType.TestGroup: // 01,02: Satellite
                Debug.Log("[SET] 실험군 InitalizeThisTry()-Try Setting 완료");
                player.transform.position = new Vector3(0, 0, -0.224000007f);
                MiniatureWorld.Instance.gameObject.transform.position = miniatureVec;
                MiniatureWorld.Instance.RemoveProxies();
                break;

            case TaskGroupType.ControlGroup1: //02: Teleport
                Debug.Log("[SET] 대조군1 InitalizeThisTry()-Try Setting 완료");
                player.transform.position = new Vector3(0, 0, -0.224000007f);
                player.transform.rotation = Quaternion.identity;
                MiniatureWorld.Instance.gameObject.transform.position = new Vector3(0.0f, -10.0f, 0.0f);
                MiniatureWorld.Instance.RemoveProxies();
                break;

            case TaskGroupType.ControlGroup2: //02: Teleport & WiM
                Debug.Log("[SET] 대조군2 InitalizeThisTry()-Try Setting 완료");
                player.transform.position = new Vector3(0, 0, -0.224000007f);
                player.transform.rotation = Quaternion.identity;
                MiniatureWorld.Instance.gameObject.transform.position = miniatureVec;
                MiniatureWorld.Instance.RemoveProxies();
                break;
        }
    }

    public override void SetTargetValue()
    {
        float timeSeed = Time.time * 100f;
        Random.InitState((int)timeSeed);
        
        float xMinValue = 0.0f, xMaxValue = 0.0f, yMinValue = 0.0f, yMaxValue = 0.0f;
        
        switch (currentTestData.targetPosition[(int)currentTryNum])
        {
            case 0: //front, front
                xMinValue = -45.5f; xMaxValue = 45.5f; yMinValue = 20.0f; yMaxValue = 45.5f;
                break;

            case 1: //Back, Back
                xMinValue = -45.5f; xMaxValue = 45.5f; yMinValue = -45.5f; yMaxValue = -20.0f;
                break;

            case 2: //front, Back
                xMinValue = -45.5f; xMaxValue = 45.5f; yMinValue = -45.5f; yMaxValue = 45.5f;
                break;
        }

        // A와 Target Set
        target_xValue = Random.Range(xMinValue, xMaxValue);
        target_yValue = 0.022f;
        target_zValue = Random.Range(yMinValue, yMaxValue);
        indicator_A.transform.position = new Vector3(target_xValue, target_yValue, target_zValue);
        Instantiate(targetObject, new Vector3(target_xValue, 0.15f, target_zValue), quaternion.identity);
        
        timeSeed = Time.time * 100f;
        Random.InitState((int)timeSeed);

        switch (currentTestData.targetPosition[(int)currentTryNum])
        {
            case 0: //front, front
            case 1:
                while (true)
                {
                    target_xValue = Random.Range(xMinValue, xMaxValue);
                    target_yValue = 0.022f;
                    target_zValue = Random.Range(yMinValue, yMaxValue);

                    Vector3 pos = new Vector3(target_xValue, target_yValue, target_zValue);

                    if ((indicator_A.transform.position - pos).sqrMagnitude > 3000.0f)
                    {
                        indicator_B.transform.position = pos;
                        break;
                    }
                }
                break;
            
            case 2:
                if (indicator_A.transform.position.z >= 0)
                {
                    yMinValue = -45.5f; yMaxValue = 0f;
                }
                else
                {
                    yMinValue = 0f; yMaxValue = 45.5f;
                }
                
                while (true)
                {
                    target_xValue = Random.Range(xMinValue, xMaxValue);
                    target_yValue = 0.022f;
                    target_zValue = Random.Range(yMinValue, yMaxValue);

                    Vector3 pos = new Vector3(target_xValue, target_yValue, target_zValue);

                    if ((indicator_A.transform.position - pos).sqrMagnitude > 3000.0f)
                    {
                        indicator_B.transform.position = pos;
                        break;
                    }
                }
                break;
        }
    }
    
    private void SetTimeThisTry(bool set)
    {
        if (set)
        {
            _totalTime = 0.0f;
            IsTickTotalTime = true;
        }
        else
        {
            IsTickTotalTime = false;
            totalTime.Add(currentTryNum, _totalTime);
            IsTestRecordEnd = false;
        }
    }
    
    private void StartTimeSetting()
    {
        _thisTime = 0.0f;
        IsTickThisTime = true;
    }
    
    private void FinishTimeSetting()
    {
        IsTickThisTime = false;
        
        switch (state)
        {
            case TestState.FinishSet_A:
                List<float> portalTime = new List<float>();
                portalTime.Add(_thisTime); //portalTime[0] = A portal time
                creationTime.Add(currentTryNum, portalTime);
                break;

            case TestState.FinishSet_B:
                // 세팅하는데 결리는 Time 측정 종료, 기록
                if (creationTime.TryGetValue(currentTryNum, out List<float> list))
                    list.Add(_thisTime); //portalTime[1] = B portal time
                break;
            
            case TestState.EndThisTry:
                movementTime.Add(currentTryNum, _thisTime);
                break;
        }

    }
    
    public override void SetMeasures()
    {
        switch (state)
        {
            case TestState.NotStarted:
                currentTryNum++;
                InitalizeThisTry();
                break;

            case TestState.SettingTarget_A:
                TestImagePanel.SetActive(false);
                techniques.SetActive(true);
                SetTimeThisTry(true);
                StartTimeSetting(); // Portal을 세팅하는데 결리는 Time 측정 시작
                portalIndex = 0;
                break;

            case TestState.FinishSet_A:
                techniques.SetActive(false);
                FinishTimeSetting(); // Target을 세팅하는데 결리는 Time 측정 종료, 기록
                GoNextTestState(); // 자동으로 다음으로
                
                break;

            case TestState.SettingTarget_B:
                techniques.SetActive(true);
                StartTimeSetting(); // Target을 세팅하는데 결리는 Time 측정 시작
                portalIndex = 1;
                break;

            case TestState.FinishSet_B:
                techniques.SetActive(false);
                FinishTimeSetting(); // Target을 세팅하는데 결리는 Time 측정 종료, 기록
                GoNextTestState(); // 자동으로 다음으로
                
                break;

            case TestState.MoveObject:
                StartTimeSetting();
                break;

            case TestState.EndThisTry:
                // Trigger시에 자동으로 현재 Try의 Total Time 측정 종료, 기록
                FinishTimeSetting(); //TODO: 이게 Object가 Trigger 되면 자동으로 넘어가줘야 할 듯
                SetTimeThisTry(false);
                Question_NasaTLX();
                CheckResult();
                break;
        }
    }
    
    
    // +-------------------- CALL --------------------+ //
    public void SaveTargetSetNum()
    {
        if (state == TestState.SettingTarget_A)
        {
            if (targetASave.TryGetValue(currentTryNum, out List<float> temp))
            {
                temp.Add(_thisTime);
            }
            else
            {
                List<float> timeList = new List<float>();
                timeList.Add(_thisTime);
                targetASave.Add(currentTryNum, timeList);
            }
        }
        else if (state == TestState.SettingTarget_B)
        {
            if (targetBSave.TryGetValue(currentTryNum, out List<float> temp))
            {
                temp.Add(_thisTime);
            }
            else
            {
                List<float> timeList = new List<float>();
                timeList.Add(_thisTime);
                targetBSave.Add(currentTryNum, timeList);
            }
        }
        else return;
    }
    
    public void SaveTargetSetDistance(Vector3 portalPos)
    {
        if (state == TestState.SettingTarget_A)
        {
            float distance = Vector3.Distance(portalPos, indicator_A.transform.position);
            if (targetADistance.TryGetValue(currentTryNum, out List<float> temp))
            {
                temp.Add(distance);
            }
            else
            {
                List<float> value = new List<float>();
                value.Add(distance);
                targetADistance.Add(currentTryNum, value);
            }
        }
        else if (state == TestState.SettingTarget_B)
        {
            float distance = Vector3.Distance(portalPos, indicator_B.transform.position);
            if (targetBDistance.TryGetValue(currentTryNum, out List<float> temp))
            {
                temp.Add(distance);
            }
            else
            {
                List<float> value = new List<float>();
                value.Add(distance);
                targetBDistance.Add(currentTryNum, value);
            }
        }
        else return;
    }
    
        private void CheckResult()
    {
        Debug.Log("[RESULT] Save Data Start");

        uint tryNum = currentTryNum;
        //for (uint tryNum = 0; tryNum < repeatTryNum; tryNum++)
        {
            TaskTry taskResult = new TaskTry();
            currentTry = taskResult;

            if (totalTime.TryGetValue(tryNum, out float value))
            {
                // 1) 실험에 총 걸린 시간
                taskResult.tryNum = (tryNum + 1);
                taskResult.totalTime = value;

                // 2) 포탈 생성에 걸린 시간
                if (creationTime.TryGetValue(tryNum, out List<float> values))
                {
                    taskResult.ACreationTime = values[0];
                    taskResult.BCreationTime = values[1];
                }

                // 3) 각 포탈 생성 횟수와 시간
                if (targetASave.TryGetValue(tryNum, out List<float> A_createList))
                {
                    taskResult.ACreationNum = A_createList.Count;
                    taskResult.ATimeList = A_createList.ToArray();
                }

                if (targetBSave.TryGetValue(tryNum, out List<float> B_createList))
                {
                    taskResult.BCreationNum = B_createList.Count;
                    taskResult.BTimeList = B_createList.ToArray();
                }

                // 4) 각 포탈 생성 거리
                float sumDistance = 0.0f;
                if (targetADistance.TryGetValue(tryNum, out List<float> A_distance))
                {
                    for (int i = 0; i < A_distance.Count; i++)
                        sumDistance += A_distance[i];
                    taskResult.ADistance = sumDistance / (float)A_distance.Count;
                    taskResult.ADistanceList = A_distance.ToArray();
                }

                if (targetBDistance.TryGetValue(tryNum, out List<float> B_distance))
                {
                    sumDistance = 0.0f;
                    for (int i = 0; i < B_distance.Count; i++)
                        sumDistance += B_distance[i];
                    taskResult.BDistance = sumDistance / (float)B_distance.Count;
                    taskResult.BDistanceList = B_distance.ToArray();
                }
                
                // 5) Target 이동 시간
                if (movementTime.TryGetValue(tryNum, out float move))
                {
                    taskResult.moveTime = move;
                }
            }
            // TaskTry struct 세팅 완료, txt 변환
            Save(totalTryNum[(int)currentGroupType], taskResult);
            totalTryNum[(int)currentGroupType]++; // 현재 수행한 기술의 Try수 +1
        }
    }

    public void Save(uint tryNum, TaskTry saveData)
    {
        string name = "Test02_Subject" + subjectNum + "_" + currentGroupType + "_Try_" + tryNum;
        
        //ToJson 부분
        string jsonData = JsonUtility.ToJson(saveData, true);

        string path = Application.dataPath + "/DataSave/Subject" + subjectNum + "/02/" + currentGroupType;
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        File.WriteAllText(path + "/" + name + ".txt", jsonData);
    }
    
    
}
