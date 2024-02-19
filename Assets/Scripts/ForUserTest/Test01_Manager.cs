using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;


public class Test01_Manager : TestManager
{
    [Header("[Test01] GameObject")]
    public Transform portalPlace;

    [Header("[Test01] Techique")] 
    [SerializeField] private MakeRoi testInteraction;
    [SerializeField] private MakeRayPortal controlInteraction_1;
    [SerializeField] private GameObject controlInteraction_2;
    
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

    private void Start()
    {
        DontDestroyOnLoad(this);
        
        SetGameObjects();
        SetByTestState();
        InitalizeThisTry();

        experimentNum = 1;
    }

    private void Update()
    {
        GetKeyboardCommand(); // 오류 생길 경우 처리
        
        if (state != TestState.NotStarted)
            TickTime();

        // 0, 1, 2 try까지만 확인
        // 다음 technique로 넘어갑니다
        if (!IsTestRecordEnd && currentTryNum > repeatTryNum)
        {
            CheckResult();
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

    public void SavePortalNum()
    {
        if (state == TestState.SettingPortal_A)
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
        else if (state == TestState.SettingPortal_B)
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

    public void SavePortalDistance(Vector3 portalPos)
    {
        if (state == TestState.SettingPortal_A)
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
        else if (state == TestState.SettingPortal_B)
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

    public  override GameObject GetTestManager()
    {
        return this.gameObject;
    }
    
    public override void SetByTestState()
    {
        base.SetByTestState();
    }

    public override void SetMeasuresByTestState()
    {
        switch (state)
        {
            case TestState.NotStarted:
                InitalizeThisTry();
                break;

            case TestState.SettingPortal_A:
                techniques.SetActive(true);
                
                // 현재 Try의 Total Time 측정 시작
                _totalTime = 0.0f;
                IsTickTotalTime = true;
                // Portal을 세팅하는데 결리는 Time 측정 시작
                _thisTime = 0.0f;
                IsTickThisTime = true;
                break;

            case TestState.FinishPortalSet_A:
                techniques.SetActive(false);
                
                // Portal을 세팅하는데 결리는 Time 측정 종료, 기록
                IsTickThisTime = false;
                List<float> portalTime = new List<float>();
                portalTime.Add(_thisTime); //portalTime[0] = A portal time
                portalCreationTime.Add(currentTryNum, portalTime);
                break;
            
            case TestState.SettingPortal_B:
                techniques.SetActive(true);
                
                // Portal을 세팅하는데 결리는 Time 측정 시작
                _thisTime = 0.0f;
                IsTickThisTime = true;
                break;
            
            case TestState.FinishPortalSet_B:
                techniques.SetActive(false);
                
                // Portal을 세팅하는데 결리는 Time 측정 종료, 기록
                if (portalCreationTime.TryGetValue(currentTryNum, out List<float> list))
                    list.Add(_thisTime); //portalTime[1] = B portal time
                break;  

            case TestState.MoveObject:
                // Trigger시에 자동으로 현재 Try의 Total Time 측정 종료, 기록
                IsTickTotalTime = false;
                totalTime.Add(currentTryNum, _totalTime);
                IsTestRecordEnd = false;
                
                Question_NasaTLX();
                
                currentTryNum++;
                break;
        }
    }

    private void CheckResult()
    {
        Debug.Log("[TEST01][RESULT] Save Data Start");

        for (uint tryNum = 0; tryNum < repeatTryNum; tryNum++)
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
                    taskResult.portalACreationTime = creationTime[0];
                    taskResult.portalBCreationTime = creationTime[1];
                }

                // 3) 각 포탈 생성 횟수와 시간
                if (portalASave.TryGetValue(tryNum, out List<float> A_createList))
                {
                    taskResult.portalACreationNum = A_createList.Count;
                    taskResult.portalATimeList = A_createList.ToArray();
                }

                if (portalBSave.TryGetValue(tryNum, out List<float> B_createList))
                {
                    taskResult.portalBCreationNum = B_createList.Count;
                    taskResult.portalBTimeList = B_createList.ToArray();
                }

                // 4) 각 포탈 생성 거리
                float sumDistance = 0.0f;
                if (portalADistance.TryGetValue(tryNum, out List<float> A_distance))
                {
                    for (int i = 0; i < A_distance.Count; i++)
                        sumDistance += A_distance[i];
                    taskResult.portalADistance = sumDistance / (float)A_distance.Count;
                    taskResult.portalADistanceList = A_distance.ToArray();
                }

                if (portalBDistance.TryGetValue(tryNum, out List<float> B_distance))
                {
                    sumDistance = 0.0f;
                    for (int i = 0; i < B_distance.Count; i++)
                        sumDistance += B_distance[i];
                    taskResult.portalBDistance = sumDistance / (float)B_distance.Count;
                    taskResult.portalBDistanceList = B_distance.ToArray();
                }
            }

            // TaskTry struct 세팅 완료, txt 변환
            Save(tryNum + 1, taskResult);
        }
    }

    public void Save(uint tryNum, TaskTry saveData)
    {
        
        string name = "Test01_Subject" + subjectNum + "_" + currentType + "_Try_" + tryNum;
        
        //ToJson 부분
        string jsonData = JsonUtility.ToJson(saveData, true);

        string path = Application.dataPath + "/DataSave/Subject" + subjectNum;
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
