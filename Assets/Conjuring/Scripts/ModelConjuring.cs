using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelConjuring : MonoBehaviour
{
    [SerializeField]
    private SketchfabIntegration sketchfab;
    
    void Start()
    {
        //sketchfab.RequestObject("bart simpson");
        sketchfab.onFinished += OnFinished;
    }
    
    void OnFinished(GameObject model)
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
        model.transform.localScale = Vector3.one * 1.0f / size;
    }
}
