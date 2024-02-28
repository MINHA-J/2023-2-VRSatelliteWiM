using System.Collections.Generic;
using System.IO;
using Unity.Mathematics;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


public class Test01_Manager : TestManager
{
    [FormerlySerializedAs("portalPlace")] [Header("[Test01] GameObject")]
    public Transform portalPlaces;
    
    [Space(10.0f)]
    
    [Header("[Test01] Techique")] 
    [SerializeField] private MakeRoi testInteraction;
    [SerializeField] private MakeRayPortal controlInteraction_1;
    [SerializeField] private GameObject controlInteraction_2;
    
    // 실험 결과를 저장할 데이터
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

    // 5) Task 수행 중 Target 상호작용 시간
    private Dictionary<uint, float> movementTime = new Dictionary<uint, float>();
    
    
    private void Start()
    {
        DontDestroyOnLoad(this);
        
        LoadSubjectTestData();
        SetGameObjects();
        SetTestPanel();
        InitalizeThisTry();

        experimentNum = 1;
    }

    private void Update()
    {
        GetKeyboardCommand(); // 오류 생길 경우 처리
        
        if (state != TestState.NotStarted)
            TickTime();

        // 0, 1, 2 try까지만 확인 (반복 횟수)
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
        controlInteraction_1 = techniques.transform.GetChild(0).GetComponent<MakeRayPortal>();
        controlInteraction_2 = techniques.transform.GetChild(0).gameObject;
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
        portalCreationTime.Clear();
        portalASave.Clear();
        portalBSave.Clear();
        portalADistance.Clear();
        portalBDistance.Clear();
        
        IsTestRecordEnd = false;
    }

    public override void InitalizeThisTry()
    {
        base.InitalizeThisTry();

        // 카메라 위치 재설정
        //player.GetComponent<XROrigin>().MoveCameraToWorldLocation(new Vector3(0, 0.46f, 1.36f));
        
        switch (currentGroupType)
        {
            case TaskGroupType.TestGroup: // 01,02: Satellite
                Debug.Log("[SET] 실험군 InitalizeThisTry()-Try Setting 완료");
                MiniatureWorld.Instance.gameObject.transform.position = new Vector3(0.002f, 1.5f, 1.747f);
                MiniatureWorld.Instance.RemoveProxies();
                break;

            case TaskGroupType.ControlGroup1: //01: Parabolic Ray, 02: Teleport
                Debug.Log("[SET] 대조군1 InitalizeThisTry()-Try Setting 완료");
                MiniatureWorld.Instance.gameObject.transform.position = new Vector3(0.0f, -10.0f, 0.0f);
                MiniatureWorld.Instance.RemoveProxies();
                break;

            case TaskGroupType.ControlGroup2: //01: Poros, 02: Teleport & WiM
                Debug.Log("[SET] 대조군2 InitalizeThisTry()-Try Setting 완료");
                MiniatureWorld.Instance.gameObject.transform.position = new Vector3(0.0f, -10.0f, 0.0f);
                MiniatureWorld.Instance.RemoveProxies();
                break;
        }
    }

    public void SaveTargetSetNum()
    {
        if (state == TestState.SettingTarget_A)
        {
            if (portalASave.TryGetValue(currentTryNum, out List<float> temp))
            {
                temp.Add(_thisTime);
            }
            else
            {
                List<float> timeList = new List<float>();
                timeList.Add(_thisTime);
                portalASave.Add(currentTryNum, timeList);
            }
        }
        else if (state == TestState.SettingTarget_B)
        {
            if (portalBSave.TryGetValue(currentTryNum, out List<float> temp))
            {
                temp.Add(_thisTime);
            }
            else
            {
                List<float> timeList = new List<float>();
                timeList.Add(_thisTime);
                portalBSave.Add(currentTryNum, timeList);
            }
        }
        else return;
    }
    
    public void SaveTargetSetDistance(Vector3 portalPos)
    {
        if (state == TestState.SettingTarget_A)
        {
            float distance = Vector3.Distance(portalPos, indicator_A.transform.position);
            if (portalADistance.TryGetValue(currentTryNum, out List<float> temp))
            {
                temp.Add(distance);
            }
            else
            {
                List<float> value = new List<float>();
                value.Add(distance);
                portalADistance.Add(currentTryNum, value);
            }
        }
        else if (state == TestState.SettingTarget_B)
        {
            float distance = Vector3.Distance(portalPos, indicator_B.transform.position);
            if (portalBDistance.TryGetValue(currentTryNum, out List<float> temp))
            {
                temp.Add(distance);
            }
            else
            {
                List<float> value = new List<float>();
                value.Add(distance);
                portalBDistance.Add(currentTryNum, value);
            }
        }
        else return;
    }

    public override void SetTargetValue()
    {

        float target_xValue = 0.0f;
        float target_yValue = 0.0f;
        float target_zValue = 0.0f;        
        
        float timeSeed = Time.time * 100f;
        Random.InitState((int)timeSeed);
        
        List<float> xCandidate = new List<float>() { -4.8f, 0.0f, 4.8f };

        float minValue = 0.0f, maxValue = 0.0f;
        switch (currentTestData.targetPosition[(int)currentTryNum])
        {
            case 0: //Middle
                minValue = 11.5f;
                maxValue = 12.0f;
                break;

            case 1: //Back
                minValue = 14.20f;
                maxValue = 15.25f;
                break;

            case 2: //Both
                minValue = 11.5f;
                maxValue = 15.25f;
                break;
        }

        // A와 Target Set
        int tempA = Random.Range(0, 3);
        target_xValue = xCandidate[tempA];
        target_yValue = 0.022f;
        target_zValue = Random.Range(minValue, maxValue);
        indicator_A.transform.position = new Vector3(target_xValue, target_yValue, target_zValue);
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
        target_zValue = Random.Range(minValue, maxValue);
        indicator_B.transform.position = new Vector3(target_xValue, target_yValue, target_zValue);
    }

    public  override GameObject GetTestManager()
    {
        return this.gameObject;
    }
    
    public override void SetTestPanel()
    {
        switch (state)
        {
            case TestState.NotStarted:
                Debug.Log("실험자" + subjectNum + ", " + 
                          experimentNum + " test/" + currentGroupType + "/" + currentTryNum + "번째 Try.");
                TitleTextUI.text = "Start Task";
                ContentsTextUI.text = "파란구역 내에 있는 물체를 \n빨간구역으로 옮기세요.\n총" + currentTryNum + "/" + repeatTryNum;
                ButtonTextUI.text = "OK";
                break;

            case TestState.SettingTarget_A:
                TitleTextUI.text = "Set Portal to Blue";
                ContentsTextUI.text = "파란구역으로 Portal을 생성하세요. \n 원하는 대로 Portal이 세팅되었다면 클릭";
                ButtonTextUI.text = "CLICK";
                break;

            case TestState.FinishSet_A:
                TitleTextUI.text = "Finish Portal to Blue";
                ContentsTextUI.text = "파란구역에 Portal Setting 완료. \n 클릭";
                ButtonTextUI.text = "CLICK";
                break;

            case TestState.SettingTarget_B:
                TitleTextUI.text = "Set Portal to Red";
                ContentsTextUI.text = "빨간구역으로 Portal을 생성하세요. \n 원하는 대로 Portal이 세팅되었다면 클릭";
                ButtonTextUI.text = "CLICK";
                break;

            case TestState.FinishSet_B:
                TitleTextUI.text = "Finish Portal to Red";
                ContentsTextUI.text = "빨간구역에 Portal Setting 완료. \n 클릭";
                ButtonTextUI.text = "CLICK";
                break;

            case TestState.MoveObject:
                TitleTextUI.text = "Move Object " + (currentTryNum) + " try";
                ContentsTextUI.text = "Target 물체를 옮기세요. \n 완료하였다면 클릭";
                ButtonTextUI.text = "CLICK";
                break;
            
            case TestState.EndThisTry:
                TitleTextUI.text = "Finish " + (currentTryNum) + " try";
                ContentsTextUI.text = "작업을 완료하였습니다. \n 다음으로 가기 위해 클릭";
                ButtonTextUI.text = "DONE";
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
                FinishTimeSetting(); // Portal을 세팅하는데 결리는 Time 측정 종료, 기록
                GoNextTestState(); // 자동으로 다음으로
                
                break;

            case TestState.SettingTarget_B:
                techniques.SetActive(true);
                StartTimeSetting(); // Portal을 세팅하는데 결리는 Time 측정 시작
                portalIndex = 1;
                break;

            case TestState.FinishSet_B:
                techniques.SetActive(false);
                FinishTimeSetting(); // Portal을 세팅하는데 결리는 Time 측정 종료, 기록
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
                portalCreationTime.Add(currentTryNum, portalTime);
                break;

            case TestState.FinishSet_B:
                // Portal을 세팅하는데 결리는 Time 측정 종료, 기록
                if (portalCreationTime.TryGetValue(currentTryNum, out List<float> list))
                    list.Add(_thisTime); //portalTime[1] = B portal time
                break;
            
            case TestState.EndThisTry:
                movementTime.Add(currentTryNum, _thisTime);
                break;
        }

    }

    private void CheckResult()
    {
        Debug.Log("[RESULT] Save Data Start");

        uint tryNum = currentTryNum;
        //for (uint tryNum = 0; tryNum < repeatTryNum; tryNum++)
        {
            TaskTry taskResult = new TaskTry();
            currentTry = taskResult;

            if (totalTime.TryGetValue(tryNum, out float time))
            {
                // 1) 실험에 총 걸린 시간
                taskResult.tryNum = (tryNum + 1);
                taskResult.totalTime = time;

                // 2) 포탈 생성에 걸린 시간
                if (portalCreationTime.TryGetValue(tryNum, out List<float> creationTime))
                {
                    taskResult.ACreationTime = creationTime[0];
                    taskResult.BCreationTime = creationTime[1];
                }

                // 3) 각 포탈 생성 횟수와 시간
                if (portalASave.TryGetValue(tryNum, out List<float> A_createList))
                {
                    taskResult.ACreationNum = A_createList.Count;
                    taskResult.ATimeList = A_createList.ToArray();
                }

                if (portalBSave.TryGetValue(tryNum, out List<float> B_createList))
                {
                    taskResult.BCreationNum = B_createList.Count;
                    taskResult.BTimeList = B_createList.ToArray();
                }

                // 4) 각 포탈 생성 거리
                float sumDistance = 0.0f;
                if (portalADistance.TryGetValue(tryNum, out List<float> A_distance))
                {
                    for (int i = 0; i < A_distance.Count; i++)
                        sumDistance += A_distance[i];
                    taskResult.ADistance = sumDistance / (float)A_distance.Count;
                    taskResult.ADistanceList = A_distance.ToArray();
                }

                if (portalBDistance.TryGetValue(tryNum, out List<float> B_distance))
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
            //Save(totalTryNum[(int)currentGroupType], taskResult);
            Save(currentTryNum, taskResult);
            totalTryNum[(int)currentGroupType]++; // 현재 수행한 기술의 Try수 +1
        }
    }

    public void Save(uint tryNum, TaskTry saveData)
    {
        string name = "Test01_Subject" + subjectNum + "_Try_" + tryNum + "_" + currentGroupType;
        
        //ToJson 부분
        string jsonData = JsonUtility.ToJson(saveData, true);

        string path = Application.dataPath + "/DataSave/Subject" + subjectNum + "/01";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        File.WriteAllText(path + "/" + name + ".txt", jsonData);

        //FromJson 부분
        //string fromJsonData = File.ReadAllText(path + "/" + name + ".txt");

        //TaskTryList TaskTryFromJson = new TaskTryList();
        //TaskTryFromJson = JsonUtility.FromJson<TaskTryList>(fromJsonData);
        

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
