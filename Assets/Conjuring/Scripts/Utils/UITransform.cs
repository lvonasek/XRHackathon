using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Updates UI transformation to be visible to the user
 */
public class UITransform : MonoBehaviour
{
    [SerializeField]
    private float animationSpeed = 0.025f;
    [SerializeField]
    private float altitudeTolerance = 0.3f;
    [SerializeField]
    private float angleTolerance = 30;
    [SerializeField]
    private float distanceMin = 1.5f;
    [SerializeField]
    private float distanceMax = 3.0f;

    private bool correctAltitude;
    private bool correctDirection;

    void Start()
    {
        if (Application.isEditor)
        {
            transform.position = new Vector3(0, 0, 2.0f / 3.0f);
        }
    }

    void Update()
    {
        if (!Application.isEditor && OVRManager.hasInputFocus)
        {
            UpdateAltitude();
            UpdateDistance();
            UpdateDirection();
        }
    }

    /**
     * Update UI altitude if it is too low or too high
     */
    private void UpdateAltitude()
    {
        float uiY = transform.position.y;
        float camY = Camera.main.transform.position.y;
        float diff = Mathf.Abs(camY - uiY);
        if (diff > altitudeTolerance)
        {
            correctAltitude = true;
        }
        else if (diff < 0.01f)
        {
            correctAltitude = false;
        }

        if (correctAltitude)
        {
            Vector3 position = transform.position;
            position.y = Mathf.Lerp(uiY, camY, animationSpeed);
            transform.position = position;
        }
    }

    /**
     * Update UI distance if it is too near or too far
     */
    private void UpdateDistance()
    {
        float distanceCenter = (distanceMax - distanceMin) / 2.0f;
        Vector3 direction = (transform.position - Camera.main.transform.position);
        direction.y = 0.0f;

        if (direction.magnitude < distanceMin)
        {
            Vector3 position = Camera.main.transform.position + direction.normalized * distanceMin;
            position.y = transform.position.y;
            transform.position = Vector3.Lerp(transform.position, position, animationSpeed);
        }
        else if (direction.magnitude > distanceMax)
        {
            Vector3 position = Camera.main.transform.position + direction.normalized * distanceMax;
            position.y = transform.position.y;
            transform.position = Vector3.Lerp(transform.position, position, animationSpeed);
        }
    }

    /**
     * Update UI rotation to point to the user
     */
    private void UpdateDirection()
    {
        float camYaw = Camera.main.transform.rotation.eulerAngles.y;
        float uiYaw = transform.rotation.eulerAngles.y;
        while (uiYaw - camYaw > 360)
        {
            uiYaw -= 360;
        }
        while (uiYaw - camYaw < -360)
        {
            uiYaw += 360;
        }

        float diff = Mathf.Abs(uiYaw - camYaw);
        if (diff > angleTolerance)
        {
            correctDirection = true;
        }
        else if (diff < 0.1f)
        {
            correctDirection = false;
        }

        if (correctDirection)
        {
            Quaternion rotation = Quaternion.Euler(0, camYaw, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, rotation, animationSpeed);
            
            Vector3 direction = (transform.position - Camera.main.transform.position);
            direction.y = 0.0f;
            Vector3 forward = transform.forward * direction.magnitude;
            forward.y = 0.0f;
            Vector3 position = Camera.main.transform.position + forward;
            position.y = transform.position.y;
            transform.position = position;
        }
    }
}
