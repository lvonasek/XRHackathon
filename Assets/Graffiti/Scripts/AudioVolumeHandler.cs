using UnityEngine;

public class AudioVolumeHandler : MonoBehaviour
{
    [SerializeField]
    private float fadingSpeed = 0.03f;
    [SerializeField]
    private AudioSource sfx;

    private void Update()
    {
        float targetVolume = OVRManager.hasInputFocus ? 1 : 0;
        sfx.volume = Mathf.Lerp(sfx.volume, targetVolume, fadingSpeed);
    }
}
