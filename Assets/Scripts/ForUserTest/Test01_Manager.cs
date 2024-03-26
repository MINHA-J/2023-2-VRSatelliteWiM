using System;
using System.Collections.Generic;
using System.IO;
using Leap.Unity;
using Unity.Mathematics;
using Unity.VisualScripting;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;


public class Test01_Manager : TestManager
{
    [Space(10.0f)] 
    [Header("Hand Gameobject")] 
    public GameObject RightHand;
    public GameObject LeftHand;

    public GameObject R_palm;
    public GameObject L_palm;
    
    [FormerlySerializedAs("portalPlace")] [Header("[Test01] GameObject")]
    public Transform portalPlaces;
    [SerializeField] private GameObject _target;
    
    [Space(10.0f)]
    
    [Header("[Test01] Techique")] 
    [SerializeField] private MakeRoi testInteraction;
    [SerializeField] private HandRayPortal controlInteraction_1;
    [SerializeField] private GameObject controlInteraction_2;

    [Space(10.0f)] 
    [Header("[Test01] values")] 
    [SerializeField] private int _repeatNum = 6;
    
    //+---------------- 실험 결과를 저장할 데이터 ----------------+//
    // 실험에 총 걸린 시간을 Check하기 위함 ( TryNum, Time )
    private Dictionary<uint, float> totalTime = new Dictionary<uint, float>();
    
    // 실험 Section 1-3 각각의 시간을 저장하기 위함 
    private Dictionary<uint, List<float>> sectionsTime = new Dictionary<uint, List<float>>();
    
    // Portal 생성 및 제어에 걸린 시간을 Check 하기 위함 (A, B)
    private Dictionary<uint, List<float>> techniqueTime = new Dictionary<uint, List<float>>();
    
    // Portal 생성 당시의 시간을 저장하기 위함
    private uint ACreationNum = 0;
    private uint BCreationNum = 0;
    private Dictionary<uint, List<float>> ACreationTime = new Dictionary<uint, List<float>>();
    private Dictionary<uint, List<float>> BCreationTime = new Dictionary<uint, List<float>>();

    // TODO: Portal 제어 당시의 시간을 저장하기 위함
    private uint ACorrectionNum = 0;
    private uint BCorrectionNum = 0;
    private Dictionary<uint, List<float>> ACorrectionTime = new Dictionary<uint, List<float>>();
    private Dictionary<uint, List<float>> BCorrectionTime = new Dictionary<uint, List<float>>();
    
    // TODO: Portal을 통해 옮기는 과정에서의 Error를 저장하기 위함
    private uint errorNum = 0;
    private Dictionary<uint, List<float>> errorTimes = new Dictionary<uint, List<float>>();
    
    // Target과 생성된 Portal Distance
    private Dictionary<uint, List<float>> ACreationDistance = new Dictionary<uint, List<float>>();
    private Dictionary<uint, List<float>> BCreationDistance = new Dictionary<uint, List<float>>();
    
    // Task 수행 중 Target 상호작용 시간
    private Dictionary<uint, float> movementTime = new Dictionary<uint, float>();
    
    // 옮겨진 Target과 B구역의 Distance
    private Dictionary<uint, float> BMoveDistance = new Dictionary<uint, float>();
            
    // Hand의 이동과 회전한 정도
    private List<float> handRTime = new List<float>();
    private List<float> handLTime = new List<float>();
    private List<Vector3> handRMovement = new List<Vector3>();
    private List<Vector3> handLMovement = new List<Vector3>();
    private List<float> handRMovementValue = new List<float>();
    private  List<float>handLMovementValue = new List<float>();
    private List<Quaternion> handRRotation = new List<Quaternion>();
    private List<Quaternion> handLRotation = new List<Quaternion>();
    private List< float> handRRotationValue = new List<float>();
    private List<float> handLRotationValue = new List<float>();
    
    
    //+-----------------------------------------------------+//
    
    private bool IsUpdated = false;
    private Vector3 _beforeRHandPos;
    private Vector3 _beforeLHandPos;
    private quaternion _beforeRHandRot;
    private quaternion _beforeLHandRot;
    
    
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
        {
            TickTime();
        }

        if (IsTickThisTime)
        {
            SaveHandMovement();
        }
        
        if (!IsUpdated && currentTryNum >= repeatTryNum / 2)
        {
            Debug.Log("Section 01 FINISH");
            IsUpdated = true;
        }
        
        // 0, 1, 2 try까지만 확인 (반복 횟수)
        // 다음 technique로 넘어갑니다
        if (currentTryNum >= repeatTryNum)
        {
            ChangeTaskType();
            
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit(); // 어플리케이션 종료
#endif
            
        }

        UpdateHandMovement();
    }

    public override void TickTime()
    {
        base.TickTime();

        if (IsTickSessionTime)
            _sessionTime += Time.deltaTime;
    }

    private void UpdateHandMovement()
    {
        if (RightHand.activeSelf)
        {
            _beforeRHandPos = R_palm.transform.position;
            _beforeRHandRot = R_palm.transform.rotation;

        }
        if (LeftHand.activeSelf)
        {
            _beforeLHandPos = L_palm.transform.position;
            _beforeLHandRot = L_palm.transform.rotation;
        }

    }

    private void SaveHandMovement()
    {
        if (RightHand.activeSelf)
        {
            Vector3 palmPosition = R_palm.transform.position;
            float _distance = (_beforeRHandPos - palmPosition).sqrMagnitude;
            handRTime.Add(_totalTime);
            handRMovement.Add(palmPosition);
            handRMovementValue.Add(_distance);

            // 두 벡터를 기준으로 회전을 계산하여 Quaternion으로 변환
            Quaternion rot = R_palm.transform.rotation;
            float _angle = Quaternion.Angle(rot, _beforeRHandRot);
            handRRotation.Add( rot);
            handRRotationValue.Add(_angle);
        }

        if (LeftHand.activeSelf)
        {
            Vector3 palmPosition = L_palm.transform.position;
            float _distance = (_beforeLHandPos - palmPosition).sqrMagnitude;
            handLTime.Add(_totalTime);
            handLMovement.Add(palmPosition);
            handLMovementValue.Add(_distance);

            // 두 벡터를 기준으로 회전을 계산하여 Quaternion으로 변환
            Quaternion rot = L_palm.transform.rotation;
            float _angle = Quaternion.Angle(rot, _beforeLHandRot);
            handLRotation.Add(rot);
            handLRotationValue.Add(_angle);
        }

    }

    public override void SetGameObjects()
    {
        base.SetGameObjects();
        
        testInteraction = techniques.transform.GetChild(0).GetComponent<MakeRoi>();
        controlInteraction_1 = techniques.transform.GetChild(0).GetComponent<HandRayPortal>();
        controlInteraction_2 = techniques.transform.GetChild(0).gameObject;
    }
    
    public override void ChangeTaskType()
    {
        base.ChangeTaskType();
        initalizeDictionary();
    }


    private void initalizeDictionary()
    {
        Debug.Log("[TEST] initalize Dictionary");
        currentTryNum = 0;
        
        totalTime.Clear();
        sectionsTime.Clear();
        techniqueTime.Clear();
        
        ACreationNum = 0;
        BCreationNum = 0;
        ACreationTime.Clear();
        BCreationTime.Clear();
        
        ACorrectionNum = 0;
        BCorrectionNum = 0;
        ACorrectionTime.Clear();
        BCorrectionTime.Clear();
        
        errorNum = 0;
        errorTimes.Clear();
        
        ACreationDistance.Clear();
        BCreationDistance.Clear();
        
        movementTime.Clear();
        
        BMoveDistance.Clear();

        handRTime.Clear();
        handLTime.Clear();
        handRMovement.Clear();
        handLMovement.Clear();
        handRMovementValue.Clear();
        handLMovementValue.Clear();
        handRRotation.Clear();
        handLRotation.Clear();
        handRRotationValue.Clear();
        handLRotationValue.Clear();
        
        IsTestRecordEnd = false;
    }

    private void InitalizeTryData()
    {
                
        ACreationNum = 0;
        BCreationNum = 0;
        ACorrectionNum = 0;
        BCorrectionNum = 0;
        errorNum = 0;
        
        handRTime.Clear();
        handLTime.Clear();
        handRMovement.Clear();
        handLMovement.Clear();
        handRMovementValue.Clear();
        handLMovementValue.Clear();
        handRRotation.Clear();
        handLRotation.Clear();
        handRRotationValue.Clear();
        handLRotationValue.Clear();

    }

    public override void InitalizeThisTry()
    {
        base.InitalizeThisTry();
        // 카메라 위치 재설정
        //player.GetComponent<XROrigin>().MoveCameraToWorldLocation(new Vector3(0, 0.46f, 1.36f));
        
        InitalizeTryData();
        MiniatureWorld.Instance.RemoveProxies();
        MiniatureWorld.Instance.RemoveSatellites();
        MiniatureWorld.Instance.RemoveSatellites();
        
        switch (currentGroupType)
        {
            case TaskGroupType.TestGroup: // 01,02: Satellite
                //Debug.Log("[SET] 실험군 InitalizeThisTry()-Try Setting 완료");
                MiniatureWorld.Instance.gameObject.transform.position = new Vector3(0.0f, 1.46f, 1.9f);
                break;

            case TaskGroupType.ControlGroup1: //01: Parabolic Ray
                //Debug.Log("[SET] 대조군1 InitalizeThisTry()-Try Setting 완료");
                MiniatureWorld.Instance.gameObject.transform.position = new Vector3(0.0f, -10.0f, 0.0f);
                break;

            case TaskGroupType.ControlGroup2: //01: Poros
                //Debug.Log("[SET] 대조군2 InitalizeThisTry()-Try Setting 완료");
                MiniatureWorld.Instance.gameObject.transform.position = new Vector3(0.0f, -10.0f, 0.0f);
                break;
            
            // case TaskGroupType.ControlGroup3: //01: Controller Ray
            //     Debug.Log("[SET] 대조군3 InitalizeThisTry()-Try Setting 완료");
            //     MiniatureWorld.Instance.gameObject.transform.position = new Vector3(0.0f, -10.0f, 0.0f);
            //     break;
        }
    }

    public void SaveCreationValue()
    {
        if (state == TestState.SettingTarget_A)
        {
            ACreationNum++;
            if (ACreationTime.TryGetValue(currentTryNum, out List<float> temp))
            {
                temp.Add(_thisTime);
            }
            else
            {
                List<float> timeList = new List<float>();
                timeList.Add(_thisTime);
                ACreationTime.Add(currentTryNum, timeList);
            }
        }
        else if (state == TestState.SettingTarget_B)
        {
            BCreationNum++;
            if (BCreationTime.TryGetValue(currentTryNum, out List<float> temp))
            {
                temp.Add(_thisTime);
            }
            else
            {
                List<float> timeList = new List<float>();
                timeList.Add(_thisTime);
                BCreationTime.Add(currentTryNum, timeList);
            }
        }
        else return;
    }
    
    public void SaveCorrectionValue(float t)
    {
        if (state == TestState.SettingTarget_A)
        {
            ACorrectionNum++;
            if (ACorrectionTime.TryGetValue(currentTryNum, out List<float> temp))
            {
                temp.Add(t);
            }
            else
            {
                List<float> timeList = new List<float>();
                timeList.Add(t);
                ACorrectionTime.Add(currentTryNum, timeList);
            }
        }
        else if (state == TestState.SettingTarget_B)
        {
            BCorrectionNum++;
            if (BCorrectionTime.TryGetValue(currentTryNum, out List<float> temp))
            {
                temp.Add(t);
            }
            else
            {
                List<float> timeList = new List<float>();
                timeList.Add(t);
                BCorrectionTime.Add(currentTryNum, timeList);
            }
        }
        else return;
    }
    
    public void SaveTargetSetDistance(Vector3 portalPos)
    {
        if (state == TestState.SettingTarget_A)
        {
            float distance = Vector3.Distance(portalPos, indicator_A.transform.position);
            if (ACreationDistance.TryGetValue(currentTryNum, out List<float> temp))
            {
                temp.Add(distance);
            }
            else
            {
                List<float> value = new List<float>();
                value.Add(distance);
                ACreationDistance.Add(currentTryNum, value);
            }
        }
        else if (state == TestState.SettingTarget_B)
        {
            float distance = Vector3.Distance(portalPos, indicator_B.transform.position);
            if (BCreationDistance.TryGetValue(currentTryNum, out List<float> temp))
            {
                temp.Add(distance);
            }
            else
            {
                List<float> value = new List<float>();
                value.Add(distance);
                BCreationDistance.Add(currentTryNum, value);
            }
        }
        else return;
    }

    public override void SetTargetObjects()
    {
        float target_xValue = 0.0f;
        float target_yValue = 0.0f;
        float target_zValue = 0.0f;        
        
        float timeSeed = Time.time * 100f;
        Random.InitState((int)timeSeed);
        
        List<float> xCandidate = new List<float>() { -4.8f, 0.0f, 4.8f };
        List<int> pairCandidate = new List<int>() { 0, 1, 1, 0, 1, 2, 2, 1, 2, 0, 0, 2 };
        // 6개 구역으로 수정 240322
        
        float minValue = 12.0f, maxValue =15.25f;

        // A와 Target Set
        //int tempA = Random.Range(0, 3);
        int alpha = Random.Range(0, 2);
        if (alpha == 0)
        {
            int index = pairCandidate[(int)currentTestData.targetPosition[(int)currentTryNum] * 2 + 0];
            target_xValue = xCandidate[index];
            target_yValue = 0.022f;
            target_zValue = Random.Range(minValue, maxValue);

            indicator_A.transform.position = new Vector3(target_xValue, target_yValue, target_zValue);
            _target = Instantiate(targetPrefab, new Vector3(target_xValue, 0.15f, target_zValue), quaternion.identity);
            
            index = pairCandidate[(int)currentTestData.targetPosition[(int)currentTryNum] * 2 + 1];
            target_xValue = xCandidate[index];
            target_yValue = 0.022f;
            target_zValue = Random.Range(minValue, maxValue);
            
            indicator_B.transform.position = new Vector3(target_xValue, target_yValue, target_zValue);
        }
        else if (alpha == 1)
        {
            int index = pairCandidate[(int)currentTestData.targetPosition[(int)currentTryNum] * 2 + 1];
            target_xValue = xCandidate[index];
            target_yValue = 0.022f;
            target_zValue = Random.Range(minValue, maxValue);

            indicator_A.transform.position = new Vector3(target_xValue, target_yValue, target_zValue);
            Instantiate(targetPrefab, new Vector3(target_xValue, 0.15f, target_zValue), quaternion.identity);
            
            index = pairCandidate[(int)currentTestData.targetPosition[(int)currentTryNum] * 2 + 0];
            target_xValue = xCandidate[index];
            target_yValue = 0.022f;
            target_zValue = Random.Range(minValue, maxValue);
            
            indicator_B.transform.position = new Vector3(target_xValue, target_yValue, target_zValue);
        }
        
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
                StartSessionTime(); // 해당 세션의 시간 측정 시작
                portalIndex = 0;
                break;

            case TestState.FinishSet_A:
                techniques.SetActive(false);
                FinishTimeSetting(); // Portal을 세팅하는데 결리는 Time 측정 종료, 기록
                FinishSessionTime();
                GoNextTestState(); // 자동으로 다음으로
                
                break;

            case TestState.SettingTarget_B:
                techniques.SetActive(true);
                StartTimeSetting(); // Portal을 세팅하는데 결리는 Time 측정 시작
                StartSessionTime(); // 해당 세션의 시간 측정 시작
                portalIndex = 1;
                break;

            case TestState.FinishSet_B:
                techniques.SetActive(false);
                FinishTimeSetting(); // Portal을 세팅하는데 결리는 Time 측정 종료, 기록
                FinishSessionTime();
                GoNextTestState(); // 자동으로 다음으로
                break;

            case TestState.MoveObject:
                StartTimeSetting();
                StartSessionTime(); // 해당 세션의 시간 측정 시작
                break;

            case TestState.EndThisTry:
                // Trigger시에 자동으로 현재 Try의 Total Time 측정 종료, 기록
                FinishTimeSetting();
                FinishSessionTime();

                //MovementDistance(); TargetTrigger.cs 로 moved
                
                SetTimeThisTry(false);
                HideInteraction();
                
                CheckResult(); // 실험 결과 저장

                if (IsTechRepeated()) // 3번씩 반복했다면
                {
                    // 이전 실험 데이터 초기화 - 시야 가려져서..
                    MiniatureWorld.Instance.RemoveProxies();
                    MiniatureWorld.Instance.RemoveSatellites();
                    
                    Question_NasaTLX(); // Nasa TLX와 Choice Question
                }

                totalTryNum[(int)currentGroupType]++; // 현재 수행한 기술의 Try수 +1
                break;
        }
    }

    private bool IsTechRepeated()
    {
        int tryNum = (int)totalTryNum[(int)currentGroupType];
        if (isPractice)
        {
            if (currentTryNum == 2)
                return true;
        }
        else
        {
            if (tryNum != 0 && tryNum % (_repeatNum-1) == 0)
                return true;
        }

        return false;
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

    private void StartSessionTime()
    {
        _sessionTime = 0.0f;
        IsTickSessionTime = true;
    }
    
    private void FinishSessionTime()
    {
        IsTickSessionTime = false;
        if (sectionsTime.TryGetValue(currentTryNum, out List<float> list))
        {
            list.Add(_sessionTime);
        }
        else
        {
            List<float> sessionTime = new List<float>();
            sessionTime.Add(_sessionTime);
            sectionsTime.Add(currentTryNum, sessionTime);
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
                techniqueTime.Add(currentTryNum, portalTime);
                break;

            case TestState.FinishSet_B:
                // Portal을 세팅하는데 결리는 Time 측정 종료, 기록
                if (techniqueTime.TryGetValue(currentTryNum, out List<float> list))
                    list.Add(_thisTime); //portalTime[1] = B portal time
                break;
            
            case TestState.EndThisTry:
                movementTime.Add(currentTryNum, _thisTime);
                break;
        }

    }
    
    public void AddError()
    {
        // Object 떨어뜨리거나, 포탈 중심에 설정하지 못한 경우 재설정 해줍니다
        errorNum++;
        if (errorTimes.TryGetValue(currentTryNum, out List<float> time))
        {
            time.Add(_totalTime);
        }
        else
        {
            List<float> errorTime = new List<float>();
            errorTime.Add(_totalTime);
            errorTimes.Add(currentTryNum, errorTime);
        }
    }

    public void MovementDistance(Vector3 pos)
    {
        Transform indicatorB = indicator_B.transform;

        float distance = (pos - indicatorB.position).magnitude;
        BMoveDistance.Add(currentTryNum, distance);
    }

    private void CheckResult()
    {
        Debug.Log("[RESULT] Save Data Start");

        uint tryNum = currentTryNum;
        //for (uint tryNum = 0; tryNum < repeatTryNum; tryNum++)
        {
            TaskTry taskResult = new TaskTry();
            HandData R = new HandData();
            HandData L = new HandData();
            currentTry = taskResult;

            if (totalTime.TryGetValue(tryNum, out float time))
            {
                // 1) 실험에 총 걸린 시간
                taskResult.tryNum = (tryNum + 1);
                taskResult.totalTime = time;

                // section 각각에 걸린 시간
                if (sectionsTime.TryGetValue(tryNum, out List<float> sectionTime))
                {
                    taskResult.sectionTimesList = sectionTime.ToArray();
                }
                
                // 2) 포탈 생성에 걸린 시간
                if (techniqueTime.TryGetValue(tryNum, out List<float> creationTime))
                {
                    taskResult.ACreationTime = creationTime[0];
                    taskResult.BCreationTime = creationTime[1];
                }

                // 3) 각 포탈 생성 횟수와 시간
                if (ACreationTime.TryGetValue(tryNum, out List<float> A_createList))
                {
                    taskResult.ACreationNum = (int)ACreationNum;
                    taskResult.ACreateTimeList = A_createList.ToArray();
                }

                if (BCreationTime.TryGetValue(tryNum, out List<float> B_createList))
                {
                    taskResult.BCreationNum = (int)BCreationNum;
                    taskResult.BCreateTimeList = B_createList.ToArray();
                }

                // 포탈 제어 시간
                if (ACorrectionTime.TryGetValue(tryNum, out List<float> A_correction))
                {
                    taskResult.ACorrectionNum = (int)ACorrectionNum;
                    taskResult.ACorrectTimeList = A_correction.ToArray();
                }
                if (BCorrectionTime.TryGetValue(tryNum, out List<float> B_correction))
                {
                    taskResult.BCorrectionNum = (int)BCorrectionNum;
                    taskResult.BCorrectTimeList = B_correction.ToArray();
                }
                
                // 포탈 옮기는 과정에서의 Error
                if (errorTimes.TryGetValue(tryNum, out List<float> errorTime))
                {
                    taskResult.errorNum = (int)errorNum;
                    taskResult.errorTimeList = errorTime.ToArray();
                }

                // 4) 각 포탈 생성 거리
                float sumDistance = 0.0f;
                if (ACreationDistance.TryGetValue(tryNum, out List<float> A_distance))
                {
                    for (int i = 0; i < A_distance.Count; i++)
                        sumDistance += A_distance[i];
                    taskResult.ADistance = sumDistance / (float)A_distance.Count;
                    taskResult.ADistanceList = A_distance.ToArray();
                }
                if (BCreationDistance.TryGetValue(tryNum, out List<float> B_distance))
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
                
                // Player Hand 움직임
                R.handRecordTime = handRTime.ToArray();
                R.handMovement = handRMovement.ToArray();
                R.handMovementValue = handRMovementValue.ToArray();
                R.handRotation = handRRotation.ToArray();
                R.handRotationValue = handRRotationValue.ToArray(); 
                
                L.handRecordTime = handLTime.ToArray();
                L.handMovement = handLMovement.ToArray();
                L.handMovementValue = handLMovementValue.ToArray();
                L.handRotation = handLRotation.ToArray();
                L.handRotationValue = handLRotationValue.ToArray(); 
                
            }
            // TaskTry struct 세팅 완료, txt 변환
            //Save(totalTryNum[(int)currentGroupType], taskResult); 실행 위치 변경
            SaveTry(currentTryNum, taskResult);
            SaveHandTry(currentTryNum, R, true);
            SaveHandTry(currentTryNum, L, false);
        }
    }

    public void SaveTry(uint tryNum, TaskTry saveData)
    {
        string name = " ";
        if (isPractice)
            name = "Practice_Subject" + subjectNum + "_Try_" + tryNum + "_" + currentGroupType;
        else
            name = "Test01_Subject" + subjectNum + "_Try_" + tryNum + "_" + currentGroupType;

        //ToJson 부분
        string jsonData = JsonUtility.ToJson(saveData, true);

        string path = Application.dataPath + "/DataSave/Subject" + subjectNum + "/01";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        File.WriteAllText(path + "/" + name + ".txt", jsonData);
    }
    
    public void SaveHandTry(uint tryNum, HandData rData, bool IsRight)
    {
        string name = " ";
        if (isPractice)
            name = "Practice_Subject" + subjectNum + "_Try_" + tryNum + "_" + currentGroupType;
        else
            name = "Test01_Subject" + subjectNum + "_Try_" + tryNum + "_" + currentGroupType;

        if (IsRight)
            name += "_RightHand";
        else
            name += "_LeftHand";
        
        //ToJson 부분
        string jsonData = JsonUtility.ToJson(rData, true);

        string path = Application.dataPath + "/DataSave/Subject" + subjectNum + "/01";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        File.WriteAllText(path + "/" + name + ".txt", jsonData);
    }

}