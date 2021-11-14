using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITransform : MonoBehaviour
{
    void Start()
    {
        if (Application.isEditor)
        {
            transform.position = new Vector3(0, 0, 0.67f);
        }
    }
}
