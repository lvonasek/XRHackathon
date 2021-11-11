using UnityEngine;

public class HandsController : MonoBehaviour
{
    [SerializeField]
    private OVRHand leftHand;
    [SerializeField]
    private OVRHand rightHand;

    [SerializeField]
    private HandLight leftHandLight;
    [SerializeField]
    private HandLight rightHandLight;

    void Update()
    {
        // enable or disable hands light with animation
        UpdateHandLight(leftHandLight, leftHand);
        UpdateHandLight(rightHandLight, rightHand);
    }

    private void UpdateHandLight(HandLight handLight, OVRHand hand)
    {
        float angleScale = 30.0f;
        float distanceScale = 20.0f;
        float minimalHandPower = 0.9f;
        float power = GetHandPower(hand);

        float smoothing = 0.1f;
        float targetAngle = Mathf.Max(0, (power - minimalHandPower) * angleScale);
        float targetDistance = Mathf.Max(0, (power - minimalHandPower) * distanceScale);
        if (!OVRManager.hasInputFocus)
        {
            targetAngle = 0;
            targetDistance = 0;
        }
        handLight.angle = Mathf.Lerp(handLight.angle, targetAngle, smoothing);
        handLight.distance = Mathf.Lerp(handLight.distance, targetDistance, smoothing);
        handLight.gameObject.SetActive(handLight.angle > 0);
    }

    // calculates hand power from pinch strength, low power for low confidence
    private float GetHandPower(OVRHand hand)
    {
        float output = 0;
        OVRHand.HandFinger finger = OVRHand.HandFinger.Index;
        if (hand.GetFingerConfidence(finger).CompareTo(OVRPlugin.TrackingConfidence.High) == 0)
        {
            output += hand.GetFingerPinchStrength(finger);
        }
        return output;
    }
}
