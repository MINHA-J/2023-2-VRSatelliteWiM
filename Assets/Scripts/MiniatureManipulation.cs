using System;
using System.Collections;
using System.Collections.Generic;
using Leap;
using UnityEngine;
using Leap.Unity;
using Leap.Unity.Attributes;
using Leap.Unity.Interaction;
using Unity.XR.CoreUtils;

public class MiniatureManipulation : MonoBehaviour
{
    public bool IsHolding = true;
    private bool ShowGizmos = true;

    public HandModelBase HandModel;
    public GameObject HeadModel;
    
    private Transform inWorldTransform;
    private MiniatureWorld miniatureWorld;
    
    // hand coordinate
    private Leap.Hand hand;
    private Finger thumb, index, middle;
    private Vector3 directionX, directionY, directionZ;
    private float radius;

    // roi and miniature
    private GameObject worldROI;
    private Vector3 worldOriginScale;
    private Vector3 firstGetPos;
    private bool SetfirstPos = false;
    
    // grasp
    private InteractionBehaviour interactionBehaviour;

    private void Awake()
    {
        miniatureWorld = GetComponent<MiniatureWorld>();
        radius = GetComponent<SphereCollider>().radius;
        interactionBehaviour = GetComponent<InteractionBehaviour>();
    }

    // Start is called before the first frame update
    void Start()
    {
        //ShowGizmos = false;
        inWorldTransform = this.transform;
        hand = HandModel.GetLeapHand();
        thumb = hand.Fingers[0];
        index = hand.Fingers[1];
        middle = hand.Fingers[2];
        
        //기존 설정된 ROI의 Size를 set
        worldROI = miniatureWorld.ROI.gameObject;
        worldOriginScale = worldROI.transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        IsHolding = checkHandCoordinate();
        if (IsHolding)
        {
            if (!SetfirstPos)
            {
                firstGetPos = hand.PalmPosition;
                SetfirstPos = true;
                
                SetOnHand();
            }
            
            SetRoiRatio();
        }
        else
        {
            SetfirstPos = false;
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
        if (Mathf.Abs(dotProductXY) < epsilon && Mathf.Abs(dotProductYZ) < epsilon && Mathf.Abs(dotProductZX) < epsilon)
        //if ( Mathf.Abs(dotProductYZ) < epsilon)
            return true;
        else if (interactionBehaviour.isGrasped)
            return true;
        else 
            return false;
    }

    private void SetRoiRatio()
    {
        Vector3 curGetPos = this.transform.position;
        float ratio = (curGetPos - HeadModel.transform.position).magnitude /
                      (firstGetPos - HeadModel.transform.position).magnitude;

        if (ratio > 0.93f)
            ratio = 1.0f;
        
        Vector3 ROIscale = Vector3.Lerp(new Vector3(2, 2, 2), worldOriginScale, ratio);
        worldROI.transform.localScale = ROIscale;
        
        Debug.Log(ratio);
    }
    
    public void SetOnHand()
    {
        this.transform.position = hand.PalmPosition;
        this.transform.position += this.transform.up * 0.1f;
        
        /* 231101 잠시 test 중 >> 음 이게 낫군...
        Vector3 miniRight = new Vector3(0,0,0);
        Vector3 miniUp = new Vector3(0,0,0);
        Vector3 miniForward = new Vector3(0,0,0);
        
        // Correct coordinate
        
        Plane perpendicularPlane = new Plane(directionY, this.transform.position);

        directionZ *= Mathf.Acos(Vector3.Dot(directionY, directionZ) / directionY.magnitude / directionZ.magnitude) * Mathf.Rad2Deg;

        miniUp = directionY; // first, set up vector
        miniForward = (directionZ - directionY).normalized; // set forward vector
        miniRight = Vector3.Cross(miniUp, miniForward).normalized; // set right vector

        // Set coordinate
        Debug.DrawLine(transform.position, transform.position + miniUp * 0.5f, Color.green, 0.01f, false);
        Debug.DrawLine(transform.position, transform.position + miniForward * 0.5f, Color.blue, 0.01f, false);
        Debug.DrawLine(transform.position, transform.position + miniRight * 0.5f, Color.red, 0.01f, false);
        
        this.transform.right = miniRight;
        this.transform.up = miniUp;
        this.transform.forward = miniForward;

        Debug.DrawLine(transform.position, transform.position + transform.up * 0.1f, Color.green, 0.01f, false);
        Debug.DrawLine(transform.position, transform.position + transform.forward * 0.1f, Color.blue, 0.01f, false);
        Debug.DrawLine(transform.position, transform.position + transform.right * 0.1f, Color.red, 0.01f, false);
        */
        
        //this.transform.right = hand.PalmNormal;
        //this.transform.up = hand.GetThumb().Direction;
        //this.transform.forward = miniForward;
        inWorldTransform = this.transform;
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