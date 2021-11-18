using Oculus.Voice;
using UnityEngine;

public class AudioVolumeHandler : MonoBehaviour
{
    [SerializeField]
    private float fadingSpeed = 0.03f;
    [SerializeField]
    private AudioSource sfx;
    [SerializeField]
    private AppVoiceExperience voice;

    private void Update()
    {
        float targetVolume = OVRManager.hasInputFocus && !voice.Active ? 1 : 0;
        sfx.volume = Mathf.Lerp(sfx.volume, targetVolume, fadingSpeed);
    }
}
