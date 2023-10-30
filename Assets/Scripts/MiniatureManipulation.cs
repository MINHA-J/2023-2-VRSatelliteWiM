using System.Collections;
using System.Collections.Generic;
using Leap;
using UnityEngine;
using Leap.Unity;
using Leap.Unity.Attributes;

public class MiniatureManipulation : MonoBehaviour
{
    public bool IsHolding = true;
    private bool ShowGizmos = true;

    public HandModelBase HandModel;
    private Transform inWorldTransform;

    private Leap.Hand hand;
    private Finger thumb, index, middle;
    private Vector3 directionX, directionY, directionZ;
    private float radius;
    
    // Start is called before the first frame update
    void Start()
    {
        //ShowGizmos = false;
        inWorldTransform = this.transform;
        hand = HandModel.GetLeapHand();
        thumb = hand.Fingers[0];
        index = hand.Fingers[1];
        middle = hand.Fingers[2];

        radius = GetComponent<SphereCollider>().radius;
    }

    // Update is called once per frame
    void Update()
    {
        IsHolding = checkHandCoordinate();
        if (IsHolding)
        {
            SetOnHand();
        }
        else
        {
            this.transform.position = inWorldTransform.position;
            this.transform.rotation = inWorldTransform.rotation;
        }
    }

    public bool checkHandCoordinate()
    {        
        directionX = (middle.Bone(Bone.BoneType.TYPE_DISTAL).NextJoint - hand.PalmPosition).normalized;
        directionY = (thumb.Bone(Bone.BoneType.TYPE_DISTAL).NextJoint - hand.PalmPosition).normalized;
        directionZ = (index.Bone(Bone.BoneType.TYPE_DISTAL).NextJoint - hand.PalmPosition).normalized;

        // 계산된 방향 벡터의 내적을 계산
        float dotProductXY = Vector3.Dot(directionX, directionY);
        float dotProductYZ = Vector3.Dot(directionY, directionZ);
        float dotProductZX = Vector3.Dot(directionZ, directionX);
        
        // 내적이 0에 가까운 작은 오차 범위 내에 있는 경우, 두 직선은 직교합니다.
        float epsilon = 0.5f;
        //if (Mathf.Abs(dotProductXY) < epsilon && Mathf.Abs(dotProductYZ) < epsilon && Mathf.Abs(dotProductZX) < epsilon)
        if ( Mathf.Abs(dotProductYZ) < epsilon)
        {
            Debug.Log("Hand Coordinate ON");
            return true;
        }
        else
        {
            //Debug.Log("Can't set hand coordinate.");
            return false;
        }
    }

    public void SetOnHand()
    {
        this.transform.position = hand.PalmPosition;
        
        directionX = (middle.Bone(Bone.BoneType.TYPE_DISTAL).NextJoint - hand.PalmPosition).normalized;
        directionY = (thumb.Bone(Bone.BoneType.TYPE_DISTAL).NextJoint - hand.PalmPosition).normalized;
        directionZ = (index.Bone(Bone.BoneType.TYPE_DISTAL).NextJoint - hand.PalmPosition).normalized;
        
        this.transform.up = directionY;
        this.transform.forward = directionZ;
        this.transform.right = directionX;

        this.transform.position += this.transform.up * 0.1f;
    }
    
#if UNITY_EDITOR
     private void OnDrawGizmos()
    {
        if (ShowGizmos && HandModel != null && HandModel.IsTracked)
        {
            hand = HandModel.GetLeapHand();
            thumb = hand.Fingers[0];
            index = hand.Fingers[1];
            middle = hand.Fingers[2];

            Gizmos.color = Color.green;
            Gizmos.DrawRay(hand.PalmPosition, thumb.Bone(Bone.BoneType.TYPE_DISTAL).NextJoint - hand.PalmPosition);

            Gizmos.color = Color.blue;
            Gizmos.DrawRay(hand.PalmPosition, index.Bone(Bone.BoneType.TYPE_DISTAL).NextJoint - hand.PalmPosition);

            Gizmos.color = Color.red;
            Gizmos.DrawRay(hand.PalmPosition, middle.Bone(Bone.BoneType.TYPE_DISTAL).NextJoint - hand.PalmPosition);
            
            Color centerColor = Color.clear;
            Vector3 centerPosition = Vector3.zero;
            Quaternion circleRotation = Quaternion.identity;

            Vector3 axis;
            float angle;
            circleRotation.ToAngleAxis(out angle, out axis);
            //Gizmos.DrawLine();
            //Utils.DrawCircle(centerPosition, axis, ActivateDistance / 2, centerColor);
            //Utils.DrawCircle(centerPosition, axis, DeactivateDistance / 2, Color.blue);
        }
    }
#endif

}