using UnityEngine;

public class HandLight : MonoBehaviour
{
    [SerializeField]
    private Vector3 defaultRotation;
    [SerializeField]
    private GameObject lightVolume;
    [SerializeField]
    private float rotationSmoothing = 0.1f;

    private Quaternion lastRotation;

    void LateUpdate()
    {
        // ensure all the light volume quads are camera-facing
        for (int i = 0; i < lightVolume.transform.childCount; i++)
        {
            lightVolume.transform.GetChild(i).rotation = Quaternion.LookRotation((lightVolume.transform.GetChild(i).position - Camera.main.transform.position).normalized);
        }

        // smooth rotation of the hand
        transform.localRotation = Quaternion.Euler(defaultRotation);
        transform.rotation = Quaternion.Lerp(lastRotation, transform.rotation, rotationSmoothing);
        lastRotation = transform.rotation;
    }
}
