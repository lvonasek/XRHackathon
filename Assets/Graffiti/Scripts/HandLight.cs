using UnityEngine;

public class HandLight : MonoBehaviour
{
    public float angle = 5;
    public float distance = 4;

    [SerializeField]
    private Vector3 defaultRotation;
    [SerializeField]
    float preferCameraHandRotationThreshold = 0.5f;
    [SerializeField]
    private float rotationSmoothing = 0.1f;
    [SerializeField]
    private float rotationStabilizing = 0.5f;
    [SerializeField]
    private OVRCameraRig tracking;

    [SerializeField]
    private GameObject lightEmission;
    [SerializeField]
    private ParticleSystem lightParticles;
    [SerializeField]
    private GameObject lightVolume;

    private Quaternion lastRotation;

    void LateUpdate()
    {
        // ensure all the light volume quads are camera-facing
        for (int i = 0; i < lightVolume.transform.childCount; i++)
        {
            lightVolume.transform.GetChild(i).rotation = Quaternion.LookRotation((lightVolume.transform.GetChild(i).position - Camera.main.transform.position).normalized);
        }

        // calculate camera-hand rotation
        Vector3 cameraPosition = tracking.centerEyeAnchor.position;
        Vector3 handPosition = transform.position;
        Quaternion cameraHandRotation = Quaternion.LookRotation(handPosition - cameraPosition, Vector3.up);

        // stabilize rotation of the hand light if the rotation is close to camera-hand rotation
        transform.localRotation = Quaternion.Euler(defaultRotation);
        float directionCorrectness = Vector3.Dot(transform.forward, tracking.centerEyeAnchor.forward);
        float smoothingDecrease = Mathf.Max(0, preferCameraHandRotationThreshold - directionCorrectness);
        float lerp = rotationStabilizing - smoothingDecrease;
        if (lerp > 0)
        {
            transform.rotation = Quaternion.Lerp(transform.rotation, cameraHandRotation, lerp);
        }

        // smooth rotation of the hand light
        transform.rotation = Quaternion.Lerp(lastRotation, transform.rotation, rotationSmoothing);
        lastRotation = transform.rotation;

        // update light angle
        float volumeScaling = distance * 0.04f;
        float volumeSize = angle * volumeScaling;
        ParticleSystem.ShapeModule shape = lightParticles.shape;
        shape.angle = angle;

        // update light distance
        float emissionScale = 0.0025f;
        float particleLifeScale = 0.1f;
        lightEmission.transform.localScale = Vector3.one * distance * emissionScale;
        lightVolume.transform.localScale = new Vector3(volumeSize, distance, volumeSize);
        lightParticles.startLifetime = distance * particleLifeScale;
    }
}
