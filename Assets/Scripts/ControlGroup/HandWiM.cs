using System.Collections;
using System.Collections.Generic;
using Leap.Unity.HandsModule;
using Leap.Unity.Interaction;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;


public class HandWiM : MonoBehaviour
{
    [Header("Basic")]
    public GameObject world;
    public GameObject leftHandModel;
    public GameObject rightHandModel;
    public InteractionHand L_Interaction;
    public InteractionHand R_Interaction;
    
    [Space(20f)] 
    [SerializeField] private GameObject duplicate;
    
    private Vector3 miniature_scale = new Vector3(0.01f, 0.01f, 0.01f);
    private LayerMask duplicate_layer;
    private LayerMask world_layer;
    
    private Matrix4x4 selected_object_initial_matrix;
    private Matrix4x4 selected_object_new_matrix;
    private Quaternion initial_rotation;
    
    public GameObject selected_object;
    public GameObject selected_object_original_parent;
    public GameObject corresponding_object;
    
    private XRController xrController;
    
    private bool map_visible = false;
    private bool dragging_object = false;
    private bool selection_in_miniature;


    void Start()
    {
        world = GameObject.Find("World");
        duplicate_layer = LayerMask.NameToLayer("DuplicateObjects");
        world_layer = LayerMask.NameToLayer("SceneObjects");
        xrController = leftHandModel.GetComponent<XRController>();
        
        create_world_in_miniature(); 
    }

    void Update()
    {
        word_in_miniature_activation();
        dragging();
        UpdateDuplicate();
    }

    private void UpdateDuplicate()
    {
        duplicate.transform.position = leftHandModel.transform.position;
        duplicate.transform.localRotation = leftHandModel.transform.localRotation * Quaternion.Inverse(initial_rotation);
    }
    
    public void create_world_in_miniature()
    {
        // copy world and all children and move them to another layer so thtat light sources only light world or miniature
        duplicate = Instantiate(world);
        duplicate.transform.position = leftHandModel.transform.position;
        duplicate.transform.localScale = miniature_scale;

        set_layer_recursively(world, world_layer);
        set_layer_recursively(duplicate, duplicate_layer);
        
        duplicate.AddComponent<InteractionBehaviour>().moveObjectWhenGrasped = true;
        duplicate.AddComponent<InteractionWorldObject>();
        //avatar.SetActive(false);
    }

    private void dragging()
    {
        // mapping: grip button (middle finger)

        if (R_Interaction.isGraspingObject && selected_object == null)
        {
            SelectObject(R_Interaction.graspedObject.gameObject);
        }
        else if (selected_object != null)
        {
            DeselectObject();
        }

        // apply the dragging to the corresponding object in the miniature/world
        if (selected_object != null && corresponding_object != null && dragging_object)
        {
            if (selection_in_miniature)
            {
                Matrix4x4 h = duplicate.transform.localToWorldMatrix;
                Matrix4x4 m_h = selected_object.transform.localToWorldMatrix;
                Matrix4x4 w = world.transform.localToWorldMatrix;
                Matrix4x4 m = w * Matrix4x4.Inverse(h) * m_h;

                Vector3 position = m.GetColumn(3);

                // Extract new local rotation
                Quaternion rotation = Quaternion.LookRotation(
                    m.GetColumn(2),
                    m.GetColumn(1)
                );

                // Extract new local scale
                Vector3 scale = new Vector3(
                    m.GetColumn(0).magnitude,
                    m.GetColumn(1).magnitude,
                    m.GetColumn(2).magnitude
                );

                corresponding_object.transform.position = position;
                corresponding_object.transform.rotation = rotation;
            }
            else
            {
                Matrix4x4 w = world.transform.localToWorldMatrix;
                Matrix4x4 w_t = selected_object.transform.localToWorldMatrix;
                Matrix4x4 h = duplicate.transform.localToWorldMatrix;
                Matrix4x4 m = h * Matrix4x4.Inverse(w) * w_t;

                Vector3 position = m.GetColumn(3);

                // Extract new local rotation
                Quaternion rotation = Quaternion.LookRotation(
                    m.GetColumn(2),
                    m.GetColumn(1)
                );

                // Extract new local scale
                Vector3 scale = new Vector3(
                    m.GetColumn(0).magnitude,
                    m.GetColumn(1).magnitude,
                    m.GetColumn(2).magnitude
                );

                corresponding_object.transform.position = position;
                corresponding_object.transform.rotation = rotation;
            }
        }

        //gripButtonLF = gripButton;

    }

    private void SelectObject(GameObject go)
    {
        if (go.name != "LowpolyTerrain")
        {
            selected_object = go;
            selected_object_initial_matrix = go.transform.localToWorldMatrix;
            selected_object_original_parent = go.transform.parent.gameObject;
            dragging_object = true;

            if (selection_in_miniature)
            {
                corresponding_object =
                    recursive_find_child(world.transform, duplicate.transform, selected_object.transform).gameObject;
            }
            else
            {
                corresponding_object =
                    recursive_find_child(duplicate.transform, world.transform, selected_object.transform).gameObject;
            }

            selected_object.transform.SetParent(rightHandModel.transform, true);
        }
    }
    
    private void DeselectObject()
    {
        selected_object.transform.SetParent(selected_object_original_parent.transform, true);

        dragging_object = false;
        selected_object = null;
        corresponding_object = null;
    }

    void word_in_miniature_activation()
    {
        if (L_Interaction.isGraspingObject)
        {
            leftHandModel.SetActive(false);
            duplicate.SetActive(true);
            initial_rotation = leftHandModel.transform.localRotation;
            map_visible = true;
        }
        else
        {
            map_visible = false;
            leftHandModel.SetActive(true);
            duplicate.SetActive(true);
        }
    }

    private void set_layer_recursively(GameObject obj, int newLayer)
    {
        if (null == obj)
        {
            return;
        }

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (null == child)
            {
                continue;
            }

            set_leap_interaction(child.gameObject);
            set_layer_recursively(child.gameObject, newLayer);
        }
    }

    private void set_leap_interaction(GameObject target)
    {
        
        if (!target.TryGetComponent(out Rigidbody _rigidbody))
        {
            target.AddComponent<Rigidbody>();
        }
        
        if (!target.TryGetComponent(out BoxCollider _collider))
        {
            target.AddComponent<BoxCollider>();
        }

        if (!target.TryGetComponent(out InteractionBehaviour _behaviour))
        {
            target.AddComponent<InteractionBehaviour>();
        }

    }
    
    public Transform recursive_find_child(Transform parent, Transform mirror_parent, Transform target)
    {
        Transform child = null;
        Transform mirror_child = null;

        for (int i = 0; i < parent.childCount; i++)
        {
            bool found = false;
            child = parent.GetChild(i);

            for (int j = 0; j < mirror_parent.childCount; j++)
            {
                if (child.name == mirror_parent.GetChild(j).name)
                {
                    mirror_child = mirror_parent.GetChild(j);
                    break;
                }
            }

            if (GameObject.ReferenceEquals(mirror_child.gameObject, target.gameObject))
            {
                found = true;
                if (mirror_child.transform.localPosition != target.transform.localPosition)
                {
                    int x = 4;
                }

                break;
            }
            else
            {
                child = recursive_find_child(child, mirror_child, target);
                if (child != null)
                {
                    break;
                }
            }
        }

        return child;
    }
}
