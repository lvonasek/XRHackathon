using UnityEngine;

public class HandLight : MonoBehaviour
{
    public float angle = 5;
    public float distance = 4;

    [SerializeField]
    private Vector3 defaultRotation;
    [SerializeField]
    private float rotationSmoothing = 0.1f;

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

        // smooth rotation of the hand
        transform.localRotation = Quaternion.Euler(defaultRotation);
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
