using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TargetTrigger : MonoBehaviour
{
    [SerializeField] private string currentTarget;
    
    private void OnTriggerEnter(Collider other)
    {
        currentTarget = other.name;

        if (other.name == "Indecator B")
        {
            switch (TestManager.Instance.experimentNum)
            {
                case 1:
                    Test01_Manager manager_1 = TestManager.Instance.GetTestManager().GetComponent<Test01_Manager>();
                    manager_1.GoNextTestState();
                    break;
                case 2:
                    Test02_Manager manager_2 = TestManager.Instance.GetTestManager().GetComponent<Test02_Manager>();
                    manager_2.GoNextTestState();
                    break;
            }
            Debug.Log("Object가 Target에 TriggerEnter!");
        }
    }
}
