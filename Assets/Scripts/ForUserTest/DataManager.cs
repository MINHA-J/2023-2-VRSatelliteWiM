using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.Serialization;

public class TestData
{
    private uint subjectNum; //실험자 번호

    private uint testNum; //실험번호

    private float totalTime; //실험에 총 걸린 시간
    private uint portalCreationNum; //포탈 생성 갯수
    private float portalCreationTime; //포탈 생성에 걸린 시간

    private float movement; //물리적인 움직임 정도
}

[System.Serializable]
public struct TaskTry
{
    public uint tryNum;
    public float totalTime;

    public float ACreationTime;
    public float BCreationTime;

    public int ACreationNum;
    public int BCreationNum;
    
    public float ADistance;
    public float BDistance;
    
    [SerializeField]
    public float[] ATimeList;
    [SerializeField]
    public float[] ADistanceList;
    [SerializeField]
    public float[] BTimeList;
    [SerializeField]
    public float[] BDistanceList;

    public float moveTime;
}

[Serializable]
public class TaskTryList
{
    public List<TaskTry> TaskTries;
}

[Serializable]
public struct TryQuestion
{
    [SerializeField]
    public int[] answerValue;
}
