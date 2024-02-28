using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Serialization;

public class Question_NasaTLX : MonoBehaviour
{
    private int questionNum = 0;
    [Space(10f)]
    public QuestionText textUI;
    public Slider answer;

    [Space(10f)] 
    public TestInformation information;

    [Space(10f)]  
    public List<int> answerValue = new List<int>();
    private TryQuestion _tryQuestion = new TryQuestion();
    
    private string[] questions = new string [6]
    {
        "얼마나 많은 정신적, 지각적 활동이 필요했나요?",
        "얼마나 많은 신체적 활동이 필요했나요?",
        "작업을 수행하는 동안 느낀 시간적 압력은 어떠했나요?",
        "작업을 얼마나 성공적으로 수행했나요?",
        "작업을 수행하기 위해 얼마나 열심히 활동해야 했나요?",
        "작업을 수행하기 위해 얼마나 스트레스 받거나 짜증났나요?",
    };

    private string[,] values = new string [6, 2]
    {
        { "쉬움", "어려움" },
        { "여유로움", "힘들었음" },
        { "여유로움", "촉박함" },
        { "만족함", "불만족함" },
        { "조금", "많이" },
        { "조금", "많이" },
    };

    
    [System.Serializable]
    public struct QuestionText
    {
        public TextMeshProUGUI qestionNumber;
        public TextMeshProUGUI question;
        public TextMeshProUGUI low;
        public TextMeshProUGUI high;
    }
    
    [System.Serializable]
    public struct TestInformation
    {
        public int subjectNum;
        public int experimentNum;
        [FormerlySerializedAs("currentType")] public TaskGroupType currentGroupType;
        public uint currentTryNum;
    }

    private void Start()
    {
        GetTestManager();
        
        questionNum = 0;
        answer.value = 0.0f;
        
        firstQuestion();
        
        answerValue.Clear();
    }

    private void GetTestManager()
    {
        information.subjectNum = TestManager.Instance.subjectNum;
        information.experimentNum = TestManager.Instance.experimentNum;
        information.currentGroupType = TestManager.Instance.currentGroupType;
        //information.currentTryNum = TestManager.Instance.totalTryNum[(int)information.currentGroupType];
        information.currentTryNum = TestManager.Instance.currentTryNum;
    }

    private void firstQuestion()
    {
        questionNum++;
        
        textUI.qestionNumber.text = "Q" + questionNum;
        textUI.question.text = questions[questionNum - 1];
        textUI.low.text = values[questionNum - 1, 0];
        textUI.high.text = values[questionNum - 1, 1];
    }

    [ContextMenu("Set Next Question")]
    public void NextQuestion()
    {

        answerValue.Add((int)answer.value);
        answer.value = 0.0f;
        if (answerValue.Count >= 6)
        {
            Debug.Log("NASA TLX 설문 끝. 다음 작업 수행합니다. ");
            GetTestManager();
            Save();
            return;
        }

        questionNum++;
        textUI.qestionNumber.text = "Q" + questionNum;
        textUI.question.text = questions[questionNum - 1];
        textUI.low.text = values[questionNum - 1, 0];
        textUI.high.text = values[questionNum - 1, 1];
    }

    public void Save()
    {
        string name = "Test01_Subject" + information.subjectNum + "_"
                      + "_Try_" + information.currentTryNum
                      + "_" + information.currentGroupType + "_NASATLX";
        
        _tryQuestion.answerValue = answerValue.ToArray();
        //ToJson 부분
        string jsonData = JsonUtility.ToJson(_tryQuestion, true);

        string path = Application.dataPath + "/DataSave/Subject" + information.subjectNum + "/0" +
                      information.experimentNum;
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        File.WriteAllText(path + "/" + name + ".txt", jsonData);
        
        TestManager.Instance.BackToTask();
    }

    private void GetKeyboardCommand()
    {
        if (Input.GetKeyDown(KeyCode.F2))
                    NextQuestion();
    }
    
    private void Update()
    {
        GetKeyboardCommand();
    }
}
