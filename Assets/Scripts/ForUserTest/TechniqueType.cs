using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class TechniqueType : MonoBehaviour
{
    public int experimentNum = 1;
    [FormerlySerializedAs("type")] public TaskGroupType groupType;
    
}
