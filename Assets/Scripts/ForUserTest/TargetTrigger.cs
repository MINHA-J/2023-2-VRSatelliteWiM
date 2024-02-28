using System;
using System.Collections;
using System.Collections.Generic;
using Leap.Unity.Interaction;
using Unity.VisualScripting;
using UnityEngine;

public class TargetTrigger : MonoBehaviour
{
    [SerializeField] private string currentTarget;

    private InteractionBehaviour _interaction;
    private Rigidbody _rigidbody;

    private Vector3 _beforePos;

    private void Start()
    {
        _interaction = this.GetComponent<InteractionBehaviour>();
        _rigidbody = this.GetComponent<Rigidbody>();
    }

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

    private void Update()
    {
        UpdateGrasped();
    }
}
