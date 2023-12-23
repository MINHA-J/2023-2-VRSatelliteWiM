using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Leap;
using UnityEngine;
using Leap.Unity;
using Leap.Unity.Attributes;
using Leap.Unity.Interaction;
using Unity.XR.CoreUtils;

public class MiniatureManipulation : MonoBehaviour
{
    [Header("Basic Setting")] 
    public HandModelBase HandModel;
    public GameObject HeadModel;

    [Header("Bool")] 
    public bool IsHolding = true;
    private bool ShowGizmos = true;

    [Header("For Test")] 
    public GameObject TempEyes;
    public bool canSetROI = false;
    
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

    // grasp and move
    private InteractionBehaviour interactionBehaviour;
    private Camera mainCamera;
    private float timer  = 0.0f;
    private float duration  = 0.08f;

    private void Awake()
    {
        miniatureWorld = GetComponent<MiniatureWorld>();
        radius = GetComponent<SphereCollider>().radius;
        interactionBehaviour = GetComponent<InteractionBehaviour>();
        mainCamera = HeadModel.transform.parent.GetComponent<Camera>();
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



    public bool checkHandCoordinate()
    {
        directionX = (middle.Bone(Bone.BoneType.TYPE_DISTAL).NextJoint - hand.PalmPosition).normalized;
        directionY = (thumb.Bone(Bone.BoneType.TYPE_DISTAL).NextJoint - hand.PalmPosition).normalized;
        directionZ = (index.Bone(Bone.BoneType.TYPE_DISTAL).NextJoint - hand.PalmPosition).normalized;

        // 계산된 방향 벡터의 내적을 계산
        float dotProductXY = Vector3.Dot(directionX, directionY);
        float dotProductXWorld = 1 - Vector3.Dot(directionX, Vector3.forward);
        float dotProductZWorld = 1 - Vector3.Dot(directionZ, Vector3.forward);
        //float dotProductYZ = Vector3.Dot(directionY, directionZ);
        //float dotProductZX = Vector3.Dot(directionZ, directionX);

        float dotProductPalm = 1 - Vector3.Dot(hand.PalmNormal, Vector3.up);
        
        // 내적이 0에 가까운 작은 오차 범위 내에 있는 경우, 두 직선은 직교합니다.
        float epsilon = 0.8f;
        //231113 잠시 변경하였습니다. 3축으로 모두 하니까 Leap motion의 한계가 ...
        if (interactionBehaviour.isGrasped == true)
        {
            return true;
        }
        else
        {
            if (Mathf.Abs(dotProductPalm) < epsilon &&
                Mathf.Abs(dotProductXWorld) < epsilon && Mathf.Abs(dotProductZWorld) < epsilon)
                return true;
            else
                return false;
        }
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

        //[DEBUG] 231106 머리와 Plane map 사이의 거리 비율을 출력함.
        //Debug.Log(ratio);
    }

    public void SetOnHand()
    {
        this.transform.position = hand.PalmPosition;
        this.transform.position += this.transform.up * 0.1f;

        /* 231101 잠시 test 중 >> 음 주석 처리한게 낫군...
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
        inWorldTransform.rotation = this.transform.rotation;
        
    }

    private Vector3 CameraRay()
    {
        RaycastHit raycastHit1, raycastHit2;
        Ray ray1 = mainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));

        Vector3 rayPoint = Vector3.zero;
        
        // Camera로부터 나오는 Ray와 miniature world 간의 교차점을 구함
        if (Physics.Raycast(ray1, out raycastHit1, 3.0f))
        {
            Debug.DrawLine(ray1.origin, raycastHit1.point, Color.green);
            if (raycastHit1.transform.CompareTag("Miniature"))
            { 
                // // miniature의 바닥으로 내린 점
                // Vector3 pointInWorldMiniature = this.transform.InverseTransformPoint(raycastHit1.point);
                // pointInWorldMiniature.y = 0;
                // pointInWorldMiniature = transform.TransformPoint(pointInWorldMiniature);
                //
                // // miniature에서 front
                // Vector3 direction = transform.TransformVector(new Vector3(0, 0, 1));
                // Ray ray2 = new Ray(pointInWorldMiniature, direction);
                //
                // if (Physics.Raycast(ray2, out raycastHit2))
                //     Debug.DrawLine(ray2.origin, raycastHit2.point, Color.blue);
                canSetROI = true;
                
                Vector3 origin = raycastHit1.point + ray1.direction * 0.01f;
                Ray ray2 = new Ray(origin, ray1.direction);
                if (Physics.Raycast(ray2, out raycastHit2))
                {
                    Debug.DrawLine(ray2.origin, raycastHit2.point, Color.blue);
                    rayPoint = raycastHit2.point;
                }
            }
            else
                canSetROI = false;
        }
        else
            canSetROI = false;
        
        
        return rayPoint;
    }

    private void CoordinateTransformation(Vector3 pointInWorld)
    {
        if (canSetROI)
        {
            // pointInWorld를 miniature에서의 로컬 좌표로 변환합니다.
            Vector3 pointInWorldMiniature = this.transform.InverseTransformPoint(pointInWorld);
            // miniature에서의 로컬 좌표를 World ROI에서의 로컬 좌표로 변환합니다.

            // World ROI에서의 로컬 좌표를 world 좌표로 변환합니다.
            Vector3 pointInWorldRoi = worldROI.transform.TransformPoint(pointInWorldMiniature);
            //TempEyes.transform.position = pointInWorldRoi;
            TempEyes.transform.DOMove(pointInWorldRoi, duration, false);
            miniatureWorld.UpdateCandidatePosition(pointInWorld, pointInWorldRoi);
        }
        else
        {
            TempEyes.transform.position = new Vector3(0, -5.0f, 0);
        }
    }

    public void UpdateEyePosition()
    {
        Vector3 cameraPoint = CameraRay();
        if (cameraPoint != Vector3.zero)
            CoordinateTransformation(cameraPoint);
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

                //SetOnHand();
            }

            SetRoiRatio();
            
        }
        else
        {
            SetfirstPos = false;
            this.transform.position = inWorldTransform.position;
            this.transform.rotation = inWorldTransform.rotation;
        }
        timer += Time.deltaTime;
        if (timer >= duration)
        {
            UpdateEyePosition();
            timer = 0f;
        }
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