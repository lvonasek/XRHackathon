using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelConjuring : MonoBehaviour
{
    [SerializeField]
    private float objectScale = 0.05f;

    [SerializeField]
    private SketchfabIntegration sketchfab;
    [SerializeField]
    private OVRCameraRig tracking;
    
    private Transform toAnchor;
    
    void OnDisable()
    {
        sketchfab.onFinished -= OnModelCreated;
    }
    
    void OnEnable()
    {
        sketchfab.onFinished += OnModelCreated;
        
        toAnchor = tracking.rightHandAnchor;
        sketchfab.RequestObject("bart simpson");
    }
    
    void OnModelCreated(GameObject model)
    {
        AssignToHand(model);
        ScaleObject(model);
    }
    
    private void AssignToHand(GameObject model)
    {
        model.transform.parent = toAnchor;
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.Euler(-90, 0, 0);
    }
    
    private void ScaleObject(GameObject model)
    {
        Vector3 min = Vector3.one * int.MaxValue;
        Vector3 max = Vector3.one * int.MinValue;
        
        foreach (MeshFilter meshFilter in model.GetComponentsInChildren<MeshFilter>())
        {
            foreach (Vector3 v in meshFilter.mesh.vertices)
            {
                if (min.x > v.x) min.x = v.x;
                if (min.y > v.y) min.y = v.y;
                if (min.z > v.z) min.z = v.z;
                
                if (max.x < v.x) max.x = v.x;
                if (max.y < v.y) max.y = v.y;
                if (max.z < v.z) max.z = v.z;
            }
        }
        
        float size = (max - min).magnitude;
        model.transform.localScale = Vector3.one * 1.0f / size * objectScale;
    }
}
