using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Updates UI transformation to be visible to the user
 */
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
