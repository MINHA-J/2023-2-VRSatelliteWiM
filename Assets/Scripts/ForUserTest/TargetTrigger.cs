using System;
using System.Collections;
using System.Collections.Generic;
using Leap.Unity.Interaction;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class TargetTrigger : MonoBehaviour
{
    [FormerlySerializedAs("currentTarget")] [SerializeField] private string triggerObject;

    private InteractionBehaviour _interaction;
    private Rigidbody _rigidbody;

    private Vector3 _beforePos;

    [SerializeField] private bool _wasGrasped = false; // User에 의해 잡힌 적이 있음 -> B에 가야함
    private bool _IsChecked = false;
    
    private void Start()
    {
        _interaction = this.GetComponent<InteractionBehaviour>();
        _rigidbody = this.GetComponent<Rigidbody>();
    }

    private void OnTriggerEnter(Collider other)
    {
        triggerObject = other.name;

        if (triggerObject == "Indecator B")
        {
            switch (TestManager.Instance.experimentNum)
            {
                case 1:
                    Test01_Manager manager_1 = TestManager.Instance.GetTestManager().GetComponent<Test01_Manager>();
                    manager_1.MovementDistance(this.transform.position);
                    manager_1.GoNextTestState();
                    break;
                case 2:
                    Test02_Manager manager_2 = TestManager.Instance.GetTestManager().GetComponent<Test02_Manager>();
                    manager_2.GoNextTestState();
                    break;
            }
            
            Debug.Log("[TARGET] Object가 Indicator B에 TriggerEnter!");
        }
        else if (_wasGrasped 
                 && (triggerObject == "Indecator A" || triggerObject == "ground" || triggerObject == "ground (1)"))
        {
            Debug.Log("[TARGET] !! Reset !!");
            ResetThisPosition();
        }
    }
    
    public void ResetThisPosition()
    {
        Test01_Manager manager_1 = TestManager.Instance.GetTestManager().GetComponent<Test01_Manager>();
        manager_1.AddError();
        
        _IsChecked = false;
        _wasGrasped = false;
        MiniatureWorld.Instance.ProxiesTable.TryGetValue(0, out ProxyNode node);
        this.transform.position = node.Marks[0].transform.position;
    }

    /*
    private void UpdateGrasped()
    {
        Transform _transform = this.transform;
        if (_interaction.isGrasped)
        {
            _rigidbody.useGravity = false;
            _interaction.graspedPoseHandler.GetGraspedPosition(out Vector3 pos, out Quaternion quaternion);
            _transform.position = pos;
            _transform.rotation = quaternion;

            _beforePos = pos;
        }
        else
        {
            float angle = Vector3.Angle(_transform.up, Vector3.up);
            if (angle > 45.0f)
                StandUp();
            _rigidbody.useGravity = true;
        }
    }
    
    void StandUp()
    {
        // 넘어진 물체를 수직으로 회전시킴
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        // 물체의 속도를 초기화하여 넘어지지 않도록 함
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
    }
    */

    private void Update()
    {
        if (!_IsChecked && _interaction.isGrasped)
        {
            _wasGrasped = true;
            _IsChecked = true;
        }
    }
}
