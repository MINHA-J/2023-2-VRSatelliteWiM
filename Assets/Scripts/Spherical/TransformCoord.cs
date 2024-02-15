using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class TransformCoord : MonoBehaviour
{
    [Header("Basic")]
    private GameObject sphericalWorld;
    private GameObject cam;
    private SphericaiWorld _sphereWorld;
    //public GameObject prefab;
    
    [Header("Setting")]
    //public float scale_ = 20.0f;
    public GameObject standard;
    public Transform spawnPos;
    [SerializeField] private float _radius = 0.25f;
    [SerializeField] private float _ratio = 40.0f;
    [SerializeField] private Vector3 _camPosition;
    [SerializeField] private uint index = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        sphericalWorld = SphericaiWorld.Instance.sphericalMap;
        cam = SphericaiWorld.Instance.cam;
        //_sphereWorld = sphericalWorld.GetComponent<SphericaiWorld>();
            
        Collider collider = GetComponent<Collider>();
        
        _camPosition = cam.transform.position;
        _radius = GetComponent<SphereCollider>().radius;

        Vector3 localUp = new Vector3(Mathf.Cos(transform.rotation.z), Mathf.Cos(transform.rotation.y), Mathf.Cos(transform.rotation.x));
        //Debug.DrawLine(this.transform.position, this.transform.position + this.transform.up * 50, Color.green, 400.0f);
        //Debug.DrawLine(this.transform.position, this.transform.position + this.transform.forward * 50, Color.blue, 400.0f);
        //Debug.DrawLine(this.transform.position, this.transform.position + this.transform.right * 50, Color.red, 400.0f);
    }

    public bool SetROI(Vector3 localVec)
    {
        // point는 local positive
        Vector3 localToWorld = getSphericalAngle(localVec);

        //Vector3 markPos = new Vector3(localVec.x, 0.001f, localVec.z);
        //float scale = (localVec - standard.transform.localPosition).sqrMagnitude * 100.0f;
            
        //Vector3 markedPos = Test_makeMarkObject(markPos, scale); // TODO: 임시로 표시함
        //_sphereWorld.CreateProxies(markedPos, 100.0f, spawnPos.position);
        
        // 움직인 Camera의 위치 반영
        Vector3 camPos = new Vector3(_camPosition.x, 0.0f, _camPosition.z);
        localToWorld = camPos + localToWorld;
        // Debug.DrawLine(sphericalWorld.transform.position, 
        //     sphericalWorld.transform.position + localVec, 
        //     Color.gray, 200.0f, false);
        
        // 해당 위치와 근접하게 이미 Proxy가 존재한다면, 생성하지 않음
        bool canDo = SphericaiWorld.Instance.CanDeployProxies(localToWorld, 10.0f);
        if (!canDo)
            return false;
        
        SphericaiWorld.Instance.CreateProxies(index, localToWorld, 100.0f, spawnPos.position);
        //SphericaiWorld.Instance.CreateProxies(index, localToWorld, 100.0f, localVec);
        SphericaiWorld.Instance.CreateSatellite(index, localVec);
        
       
        //SphericaiWorld.Instance.CreateProxies(index, localToWorld, 100.0f, spawnPos.position);
        index++;
        
        return true;
    }

    private Vector3 getSphericalAngle(Vector3 point)
    {
        // point는 local position
        // 구의 중심 좌표
        Vector3 Center = new Vector3(0, 0, 0);
        //Vector3 sphereCenter = new Vector3(0, 0, 0);
        // 중심과 점 사이의 직선 벡터 계산
        Vector3 between = new Vector3(point.x - Center.x, point.y - Center.y, point.z - Center.z);
        
        // 직선 벡터를 구의 표면과의 교점으로 정규화
        double length = Math.Sqrt(between.x * between.x + between.y * between.y + between.z * between.z);
        Vector3 normalize = new Vector3(between.x / (float)length, between.y / (float)length, between.z / (float)length);
        
        // 각도 계산
        double angle = Math.Acos(-normalize.z) * 180 / Math.PI;
        
        // local position에서 World로 변환
        Vector3 result = -standard.transform.localPosition + point;
        //Debug.DrawLine(Center, result * point.sqrMagnitude, Color.gray, 200.0f, false);
        result =  Quaternion.AngleAxis(90.0f, Vector3.right) * between;
        result.y = 0.02f; // MarkProxy가 Object에 닿아야 하기 때문에 설정한 위치값
        
        
        result *= point.magnitude * point.magnitude;
        float ratio = CalculateWeight(result);
        result *= ratio;
        //Debug.DrawLine(Center, result, Color.yellow, 200.0f, false);
        
        return result;
    }
    
    private float CalculateWeight(Vector3 result)
    {
        // 거리에 따른 가중치 계산
        float normalizedDistance = Mathf.Clamp01(result.magnitude / 30.0f);
        float weight = Mathf.Lerp(_ratio * 2.4f, _ratio * 0.8f, normalizedDistance);
        return weight; 
        
    }

    private Vector3 getSphericalAngle_try(Vector3 point)
    {
        Vector3 Center = new Vector3(0, 0, 0);
        
        // 구의 법선 벡터 계산
        Vector3 sphereNormal = CalculateSphereNormal(point);

        // 구의 표면 위의 시작점과 끝점을 평면으로 옮기기
        //Vector3 startOnPlane = MovePointToPlane(lineStart.position, sphereCenter.position, sphereNormal);
        Vector3 endOnPlane = MovePointToPlane(point, sphericalWorld.transform.position, sphereNormal);

        // 평면 위에서 직선 그리기
        Debug.DrawLine(Center, endOnPlane, Color.red, 10f);

        return endOnPlane;
    }
    
    private Vector3 CalculateSphereNormal(Vector3 pointOnSphere)
    {
        // 구의 중심에서 선택한 점까지의 벡터를 구하여 법선 벡터 계산
        Vector3 sphereNormal = (pointOnSphere - sphericalWorld.transform.position).normalized;
        return sphereNormal;
    }
    
    private Vector3 MovePointToPlane(Vector3 point, Vector3 planePoint, Vector3 planeNormal)
    {
        // 평면으로 점을 옮기는 공식: pointOnPlane = point - (dot(point - planePoint, planeNormal) * planeNormal)
        Vector3 pointOnPlane = point - (Vector3.Dot(point - planePoint, planeNormal) * planeNormal);
        return pointOnPlane;
    }

    // private Vector3 Test_makeMarkObject(Vector3 cartesian, float scale)
    // {
    //     GameObject instance = Instantiate(prefab);
    //     instance.transform.position = new Vector3(
    //         _camPosition.x + cartesian.x,
    //         0.03f,
    //         (_camPosition.z + cartesian.z)
    //     );
    //     
    //     instance.transform.position *= scale;
    //
    //     return instance.transform.position;
    //     //Debug.Log("기준으로부터 "+ scale);
    // }

    public void Update()
    {
        _camPosition = cam.transform.position;
        //Debug.Log(_camPosition);
    }

    private Vector3 getSphericalCoordinates(Vector3 cartesian, float radius)
    {
        float r = radius;
            // Mathf.Sqrt(
            // Mathf.Pow(cartesian.x, 2) + 
            // Mathf.Pow(cartesian.y, 2) + 
            // Mathf.Pow(cartesian.z, 2));

        // use atan2 for built-in checks
        float phi = Mathf.Atan2(cartesian.z, cartesian.x);
        float theta = Mathf.Asin(cartesian.y / r);

        return new Vector3(r, phi, theta);
    }

    private Vector3 getCartesianCoordinates(Vector3 spherical)
    {
        Vector3 ret = new Vector3 ();

        float t = _radius * Mathf.Cos(spherical.z);
        
        ret.x = t * Mathf.Cos (spherical.y);
        ret.y = _radius*Mathf.Sin(spherical.z);
        ret.z = t * Mathf.Sin (spherical.y);
 
        return ret;
    }
    
}
