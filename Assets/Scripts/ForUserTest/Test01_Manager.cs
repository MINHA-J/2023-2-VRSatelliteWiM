using System.Collections.Generic;
using System.IO;
using UnityEngine;


public class Test01_Manager : TestManager
{
    [Header("[Test01] GameObject")] 
    public Transform portalPlace;

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
        SetGameObjects();
        SetByTestState();
        InitalizeThisTry();

        taskNum = 1;
    }

    private void Update()
    {
        TickTime();
        
        // 0, 1, 2 try까지만 확인
        if (!IsTestRecordEnd && taskTryNum >= MaxTaskTryNum)
        {
            CheckResult();
            ChangeTaskType();
            IsTestRecordEnd = true;
        }
    }


    public override void ChangeTaskType()
    {
        base.ChangeTaskType();
        initalizeDictionary();
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
        
        IsTestRecordEnd = false;
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
                Debug.Log("[TEST01] " + currentType + "의 " + taskTryNum + "/ " + MaxTaskTryNum);
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

                IsTestRecordEnd = false;
                break;
        }
    }

    private void CheckResult()
    {
        Debug.Log("[TEST01][RESULT] Save Data Start");

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
