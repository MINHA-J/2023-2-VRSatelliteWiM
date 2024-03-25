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

    [SerializeField] public float[] sectionTimesList;

    public float ACreationTime; // techniqueTime
    public float BCreationTime;

    public int ACreationNum;
    public int BCreationNum;
    [SerializeField] public float[] ACreateTimeList;
    [SerializeField] public float[] BCreateTimeList;

    public int ACorrectionNum;
    public int BCorrectionNum;
    [SerializeField] public float[] ACorrectTimeList;
    [SerializeField] public float[] BCorrectTimeList;

    public int errorNum;
    [SerializeField] public float[] errorTimeList;

    public float ADistance;
    public float BDistance;

    [SerializeField] public float[] ADistanceList;
    [SerializeField] public float[] BDistanceList;

    public float moveTime;
    public float moveDistance;
}

[System.Serializable]
public struct HandData
{
    [SerializeField] public float[] handRecordTime;
    
    [SerializeField] public Vector3[] handMovement;
    [SerializeField] public float[] handMovementValue;
    
    [SerializeField] public Quaternion[] handRotation;
    [SerializeField] public float[]  handRotationValue;
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
