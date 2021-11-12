using UnityEngine;

public class HandsController : MonoBehaviour
{
    [SerializeField]
    private GameObject leftBrush;
    [SerializeField]
    private GameObject rightBrush;
    [SerializeField]
    private Camera canvasCamera;
    [SerializeField]
    private Camera vrCamera;

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

        // draw into texture
        UpdateBrush(leftHandLight, leftBrush);
        UpdateBrush(rightHandLight, rightBrush);
    }

    private void UpdateHandLight(HandLight handLight, OVRHand hand)
    {
        if (Application.isEditor)
        {
            return;
        }

        float angleScale = 30.0f;
        float distanceScale = 20.0f;
        float minimalHandPower = 0.75f;
        float power = GetHandPower(hand);

        float smoothing = 0.25f;
        float targetAngle = Mathf.Max(0, (power - minimalHandPower) * angleScale);
        float targetDistance = Mathf.Max(0, (power - minimalHandPower) * distanceScale);
        if (!OVRManager.hasInputFocus)
        {
            targetAngle = 0;
            targetDistance = 0;
        }
        handLight.angle = Mathf.Lerp(handLight.angle, targetAngle, smoothing);
        handLight.distance = Mathf.Lerp(handLight.distance, targetDistance, smoothing);
    }

    private void UpdateBrush(HandLight handLight, GameObject brush)
    {
        Ray ray;
        if (Application.isEditor)
        {
            brush.SetActive(Input.GetMouseButton(0));
            ray = vrCamera.ScreenPointToRay(Input.mousePosition);
        } else
        {
            brush.SetActive(handLight.angle > 1);
            ray = new Ray(handLight.transform.position, handLight.transform.forward);
        }

        if (Physics.Raycast(ray, out RaycastHit hit, 10))
        {
            if (hit.collider.gameObject != null)
            {
                brush.transform.localPosition = hit.textureCoord - Vector2.one * canvasCamera.orthographicSize;
            }
        }
    }

    // calculates hand power from pinch strength, low power for low confidence
    private float GetHandPower(OVRHand hand)
    {
#if USE_HANDS
        float output = 0;
        OVRHand.HandFinger finger = OVRHand.HandFinger.Index;
        if (hand.GetFingerConfidence(finger).CompareTo(OVRPlugin.TrackingConfidence.High) == 0)
        {
            output += hand.GetFingerPinchStrength(finger);
        }
        return output;
#else
        OVRInput.Controller controller = OVRInput.Controller.Touch;
        switch (hand.HandType)
		{
            case OVRHand.Hand.HandLeft:
                controller = OVRInput.Controller.LTouch;
                break;
            case OVRHand.Hand.HandRight:
                controller = OVRInput.Controller.RTouch;
                break;
		}
        return OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, controller) ? 1 : 0;
#endif
    }
}
