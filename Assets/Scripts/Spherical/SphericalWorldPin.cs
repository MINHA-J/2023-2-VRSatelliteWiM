using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphericalWorldPin : MonoBehaviour
{
    private Rigidbody _rigidbody;
    private Collider _collider;
    [SerializeField] private bool isTriggering;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _collider = GetComponent<MeshCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("SphereMap"))
        {
            //Debug.Log("trigger Enter "+other.name);
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.useGravity = false;
            _rigidbody.isKinematic = true;
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotationX;
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotationY;
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotationZ;

            isTriggering = true;
            transform.SetParent(other.transform); // Trigger된 오브젝트의 자식으로 놔둠.

            Vector3 localPos = this.transform.localPosition;
            bool canDo = other.GetComponent<TransformCoord>().SetROI(localPos);
            //if (!canDo)
            //Debug.Log("Proxy를 배치하지 못했음");

            Destroy(this.gameObject);
        }
    }
    
    // private void OnTriggerExit(Collider other)
    // {
    //     if (other.gameObject.CompareTag("SphereMap"))
    //     {
    //         Debug.Log("trigger Exit " + other.name);
    //         isTriggering = false;
    //         _rigidbody.useGravity = true;
    //         _rigidbody.constraints = RigidbodyConstraints.None;
    //         transform.SetParent(null); // Trigger Exit되면 종속관계 해제
    //         
    //         Destroy(this.gameObject);
    //         //SphericaiWorld.Instance.SetSatellite();
    //     }
    // }

}
