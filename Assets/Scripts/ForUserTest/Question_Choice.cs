using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using TMPro;
using System.IO;


public class Question_Choice : MonoBehaviour
{
    public int questionNum = 0;
    [Space(10f)]
    public QuestionText textUI;
    public Slider answer;
    
    [Space(10f)] 
    public TestInformation information;
    
    
    [Space(10f)]  
    public List<int> answerValue = new List<int>();
    private TryQuestion _tryQuestion = new TryQuestion();
    private int _QuestionNum = 4;
    private string[] questions = new string[4]
    {
        "공간인식이 편리함.",
        "자연스러움",
        "사용 난이도",
        "선호도"
    };

    private string[] comments = new string[4]
    {
        "목표물을 확인하고, 포탈을 생성하는 과정에서 \n공간을 인식하는 과정이 편리했다.",
        "포탈을 생성하고 제어하는 과정이 자연스러웠다",
        "객체를 향해 포탈을 생성하고, 제어하는 방법이 어렵지 않았다.",
        "해당 방법을 얼마나 선호하시나요?"
    };
        
    [System.Serializable]
    public struct QuestionText
    {
        public TextMeshProUGUI qestionNumber;
        public TextMeshProUGUI question;
        public TextMeshProUGUI comment;
    }
    
    [System.Serializable]
    public struct TestInformation
    {
        public int subjectNum;
        public int experimentNum;
        [FormerlySerializedAs("currentType")] public TaskGroupType currentGroupType;
        public uint currentTryNum;
    }
    // Start is called before the first frame update
    void Start()
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
        textUI.comment.text = comments[questionNum - 1];
    }
    
    [ContextMenu("Set Next Question")]
    public void NextQuestion()
    {

        answerValue.Add((int)answer.value);
        answer.value = 0.0f;
        if (answerValue.Count >= _QuestionNum)
        {
            //Debug.Log("NASA TLX 설문 끝. 다음 작업 수행합니다. ");
            GetTestManager();
            Save();        
            TestManager.Instance.BackToTask();
            return;
        }

        questionNum++;
        textUI.qestionNumber.text = "Q" + questionNum;
        textUI.question.text = questions[questionNum - 1];
        textUI.comment.text = comments[questionNum - 1]; 
    }
    
    public void Save()
    {
        if (TestManager.Instance.isPractice) return;

        string name = "Test01_Subject" + information.subjectNum + "_"
                      + "_Try_" + information.currentTryNum
                      + "_" + information.currentGroupType + "_Choice";
        
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
