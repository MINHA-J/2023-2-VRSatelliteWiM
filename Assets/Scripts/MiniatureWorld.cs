using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class MiniatureWorld : MonoBehaviour
{
    [Header("Basic")]
    public Dictionary<uint, GameObject> SatelliteTable = new Dictionary<uint, GameObject>();
    public Dictionary<uint, ProxyNode> ProxiesTable = new Dictionary<uint, ProxyNode>();

    [Header("Setting")]
    public MiniatureWorldROI ROI;
    public float Radius = 10.0f;
    public bool UseEditorColor = false;
    public Color Color;
    public float RadiusRatio;
    public float ProxyScaleFactor { get; private set; }
    public Vector3 CandidatePos;
    public Vector3 CandidateBeforePos;

    [Header("Satellites")] 
    public MiniatureManipulation Manipulation;
    public GameObject satellites;
    public GameObject prefabSatellite;
    
    public const float MinMarkSize = 0.1f;
    public const float MaxMarkSize = 10.0f;
    
    // Create Proxy
    OneEuroFilter<Vector3> markPosFilter = new OneEuroFilter<Vector3>(30.0f, 0.3f);
    OneEuroFilter<Vector3> proxyPosFilter = new OneEuroFilter<Vector3>(30.0f, 0.3f);
    OneEuroFilter markScaleFilter = new OneEuroFilter(30.0f);
    OneEuroFilter proxyScaleFilter = new OneEuroFilter(30.0f);
    private const float MINIMIZE_THRESHOLD = 0.05f;
    
    private float originalSize = 0.6f;
    private static List<MiniatureWorld> instances = new List<MiniatureWorld>();
    private ProxySphere sphere;
    
    private List<GameObject> _gameObjects = new List<GameObject>();
    
    
    private static MiniatureWorld instance;

    public static MiniatureWorld Instance
    {
        get
        {
            if (instance == null)
            {
                return null;
            }

            return instance;
        }
    }
    // public static IEnumerable<MiniatureWorld> Instances
    // {
    //     get => instances;
    // }

    void Awake()
    {        
        ProxyScaleFactor = 1f;
        if (!UseEditorColor)
        {
            Color = WarpColorPalette.GetColor();
        }

        sphere = GetComponentInChildren<ProxySphere>();
        sphere.Color = Color;

        //transform.DOScale(0f, 0.2f).From();

        //manipulation = GetComponent<Manipulation>();
        //manipulation.OnModeChange += OnManipulationModeChange;

        //PinchTracker.RegisterPinchable(this);
        //instances.Add(this);
        if (null == instance)
        {
            //이 클래스 인스턴스가 탄생했을 때 전역변수 instance에 게임매니저 인스턴스가 담겨있지 않다면, 자신을 넣어준다.
            instance = this;

            //씬 전환이 되더라도 파괴되지 않게 한다.
            //gameObject만으로도 이 스크립트가 컴포넌트로서 붙어있는 Hierarchy상의 게임오브젝트라는 뜻이지만, 
            //나는 헷갈림 방지를 위해 this를 붙여주기도 한다.
            //DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            //만약 씬 이동이 되었는데 그 씬에도 Hierarchy에 GameMgr이 존재할 수도 있다.
            //그럴 경우엔 이전 씬에서 사용하던 인스턴스를 계속 사용해주는 경우가 많은 것 같다.
            //그래서 이미 전역변수인 instance에 인스턴스가 존재한다면 자신(새로운 씬의 GameMgr)을 삭제해준다.
            //Destroy(this.gameObject);
        }
    }

    public void UpdateCandidatePosition(Vector3 before, Vector3 vector)
    {
        CandidateBeforePos = before;
        CandidatePos = vector;
    }
    
    /// <summary>
    /// Spherical world에서 Pin으로 선택된 ROI를 Set, Satellite를 생성
    /// </summary>
    /// <param name="pin">Detect Collider</param>
    public void CreateSatellite(uint index, Vector3 pos)
    { 
        if (SatelliteTable.TryGetValue(index, out GameObject node))
        {
            // Satellite를 Table에서 제거함
            Destroy(node);
            SatelliteTable.Remove(index);
        }
        
        GameObject instance = Instantiate(prefabSatellite);
        instance.transform.SetParent(satellites.transform);
        // Minimap위에서 Ray를 찍은 점
        pos = transform.InverseTransformPoint(pos);
        instance.transform.localPosition = pos + new Vector3(0, 0.3f, 0);
        instance.transform.forward = -this.transform.up;

        // [TASK01] 
        // if (ProxiesTable.Count > TestManager.Instance.maxPortalNum)
        // {
        //     RemoveSatellite(index - 2);
        // }
        
        instance.GetComponent<Satellite>().initPos = instance.transform.localPosition;
        instance.GetComponent<Satellite>().SetSatelliteIndex(index);
        instance.GetComponent<Satellite>().SetProxies(ProxiesTable[index].Marks[0], ProxiesTable[index]);
        SatelliteTable.Add(index, instance);
    }

    public void CreateProxies(uint index, Vector3 M_pos, float size, Vector3 P_pos)
    {
        if (ProxiesTable.TryGetValue(index, out ProxyNode node))
        {
            // 해당 인덱스에 이미 존재하는 경우
            RemoveSatellite(index);
        }
        
        //GameObject EntryWarp = Resources.Load("Prefabs/ProxyNode_fix", typeof(GameObject)) as GameObject;
        GameObject EntryWarp = Resources.Load("Prefabs/ProxyNode", typeof(GameObject)) as GameObject;
        //GameObject ExitWarp = Resources.Load("Prefabs/MarkNode-test231106", typeof(GameObject)) as GameObject;
        GameObject ExitWarp = Resources.Load("Prefabs/MarkNode", typeof(GameObject)) as GameObject;

        GameObject markedSpace = Instantiate(ExitWarp);
        GameObject proxySpace = Instantiate(EntryWarp);

        ProxyNode proxyNode = proxySpace.GetComponent<ProxyNode>();
        MarkNode markNode = markedSpace.GetComponent<MarkNode>();

        proxyNode.Marks.Add(markNode);
        proxyNode.SetCreationMode(true);

        float proxyFilteredSize = 0, markFilteredScale = 0;


        markPosFilter.Filter(M_pos);
        proxyPosFilter.Filter(P_pos);

        proxyFilteredSize = proxyScaleFilter.Filter(0.5f);
        markFilteredScale = markScaleFilter.Filter(3.0f);


        Vector3 markedPosition = new Vector3(M_pos.x, 0.0f, M_pos.z);
        markedSpace.transform.position = markedPosition;
        markedSpace.transform.localScale = new Vector3(markFilteredScale, markFilteredScale, markFilteredScale);
        markNode.Radius = markedSpace.GetComponent<SphereCollider>().radius;

        proxySpace.transform.position = P_pos;
        proxySpace.transform.localScale = new Vector3(proxyFilteredSize, proxyFilteredSize, proxyFilteredSize);

        proxyNode.SetCreationMode(false);

        ProxiesTable.Add(index, proxyNode);

        //proxyNode.Minimize();
        //proxySpace.transform.localScale = new Vector3(MINIMIZE_THRESHOLD - 0.01f, MINIMIZE_THRESHOLD - 0.01f, MINIMIZE_THRESHOLD - 0.01f);
    }

    public bool CanDeployProxies(Vector3 tempPos, float range)
    {
        // 주변에 이미 배치된 proxy가 존재하는 경우, Pass함
        foreach (var elem in ProxiesTable)
        {
            ProxyNode proxy = elem.Value;
            if ((tempPos - proxy.Marks[0].transform.position).sqrMagnitude < range)
            {
                return false;
            }
        }
        return true;
    }

    public void RemoveSatellite(uint index)
    {
        // Satellite를 Table에서 제거함
        GameObject satellite = SatelliteTable[index];
        SatelliteTable.Remove(index);
        Destroy(satellite);

        // Proxies를 Table에서 제거함
        ProxyNode proxyNode = ProxiesTable[index];
        MarkNode markNode = proxyNode.Marks[0];
        ProxiesTable.Remove(index);
        Destroy(proxyNode.gameObject);
        Destroy(markNode.spotlight);
        Destroy(markNode.gameObject);
    }

    public void RemoveProxy(uint index)
    {
        if (ProxiesTable.TryGetValue(index, out ProxyNode node))
        {
            // Proxies를 Table에서 제거함
            ProxyNode proxyNode = ProxiesTable[index];
            MarkNode markNode = proxyNode.Marks[0];
            ProxiesTable.Remove(index);
            Destroy(proxyNode.gameObject);
            Destroy(markNode.spotlight);
            Destroy(markNode.gameObject);
        }
    }
    
    public void RemoveProxies()
    {
        foreach (var pair in ProxiesTable)
        {
            ProxyNode proxyNode = pair.Value;
            MarkNode markNode = proxyNode.Marks[0];
            
            _gameObjects.Add(markNode.gameObject);
            _gameObjects.Add(markNode.spotlight);
            _gameObjects.Add(proxyNode.gameObject);
        }

        for (int i = _gameObjects.Count - 1; i >= 0; i--)
            Destroy(_gameObjects[i]);
        
        _gameObjects.Clear();
        ProxiesTable.Clear();
        ProxiesTable = new Dictionary<uint, ProxyNode>();
    }

    public void RemoveSatellites()
    {
        foreach (var pair in SatelliteTable)
            _gameObjects.Add(pair.Value);

        for (int i = _gameObjects.Count - 1; i >= 0; i--)
            Destroy(_gameObjects[i]);
        
        _gameObjects.Clear();
        SatelliteTable.Clear();
        SatelliteTable = new Dictionary<uint, GameObject>();
    }

    public MarkNode GetFirstMarkNode()
    {
        if (ProxiesTable.Count < 1)
            return null;

        Test01_Manager manager = TestManager.Instance.GetTestManager().GetComponent<Test01_Manager>();
        if (manager.portalIndex == 0)
        {
            return ProxiesTable.First().Value.Marks[0];
        }
        else
        {
            return ProxiesTable[1].Marks[0];

        }
    }
    
    private void OnDestroy()
    {
        //PinchTracker.DeregisterPinchable(this);
        instances.Remove(this);
    }

    new void Update()
    {
        if (ROI != null)
        {
            //vfxGraph.SetVector3("EndPos", mark.transform.position);
            //vfxGraph.SetFloat("EndRadius", mark.Radius);
            ROI.Color = Color;
        }


        // A bit of a stupid hack because the cone effect requires world-space size of the
        // sphere, yet the whole ProxySphere otherwise operates oblivious to its actual size
        sphere.Radius = Radius;
        RadiusRatio = ROI.transform.localScale.x / this.transform.lossyScale.x;
    }
}
