using Facebook.WitAi;
using Facebook.WitAi.Lib;
using Oculus.Voice;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelConjuring : MonoBehaviour
{
    [SerializeField]
    private float magicBallScale = 0.1f;
    [SerializeField]
    private float objectScale = 0.2f;

    [SerializeField]
    private GameObject leftHandMagicBall;
    [SerializeField]
    private GameObject rightHandMagicBall;
    [SerializeField]
    private SketchfabIntegration sketchfab;
    [SerializeField]
    private OVRCameraRig tracking;
    [SerializeField]
    private AppVoiceExperience voice;
    
    private bool anchorLeft;
    private List<GameObject> leftHandObjects;
    private List<GameObject> rightHandObjects;
    private int voiceCount;
    
    void Awake()
    {
        if (leftHandObjects == null)
        {
            leftHandObjects = new List<GameObject>();
        }
        if (rightHandObjects == null)
        {
            rightHandObjects = new List<GameObject>();
        }
    }
    
    void OnDisable()
    {
        sketchfab.onFinished -= OnModelCreated;
        voice.events.OnRequestCreated.RemoveListener(OnVoiceRequest);
    }
    
    void OnEnable()
    {
        sketchfab.onFinished += OnModelCreated;
        voice.events.OnRequestCreated.AddListener(OnVoiceRequest);
        
        if (Application.isEditor)
        {
            SetVoiceInput(true);
        }
    }
    
    void Update()
    {
        UpdateHand(true);
        UpdateHand(false);
    }
    
    void UpdateHand(bool leftHand)
    {
        Transform hand = leftHand ? tracking.leftHandAnchor : tracking.rightHandAnchor;
        GameObject magicBall = leftHand ? leftHandMagicBall : rightHandMagicBall;
        bool wasVoiceActive = magicBall.transform.localScale.magnitude > 0.5f * magicBallScale;
        float distanceToHand = (hand.position - tracking.centerEyeAnchor.position).magnitude;
        float targetBallScale = distanceToHand < 0.35f ? magicBallScale : 0;
        magicBall.transform.position = hand.position + Vector3.up * 0.1f;
        magicBall.transform.localScale = Vector3.Lerp(magicBall.transform.localScale, targetBallScale * Vector3.one, 0.1f);
        bool isVoiceActive = magicBall.transform.localScale.magnitude > 0.5f * magicBallScale;
        if (wasVoiceActive != isVoiceActive)
        {
            if (isVoiceActive)
            {
                anchorLeft = leftHand;
            }
            SetVoiceInput(isVoiceActive);
        }
    }
    
    void OnModelCreated(GameObject model)
    {
        AssignToHand(model);
        ScaleObject(model);
    }
    
    void OnVoiceResponse(string text)
    {
        Debug.Log("Recognized test: " + text);
        text = text.ToLower();
        if (text.StartsWith("a ")) text = text.Substring(2);
        if (text.StartsWith("an ")) text = text.Substring(3);
        if (text.StartsWith("the ")) text = text.Substring(4);
        sketchfab.RequestObject(text);
    }
    
    void OnVoiceRequest(WitRequest request)
    {
        request.onFullTranscription += OnVoiceResponse;   
    }
    
    private void AssignToHand(GameObject model)
    {
        Vector3 position = model.transform.localPosition;
        if (anchorLeft)
        {
            foreach (GameObject go in leftHandObjects)
            {
                Destroy(go);
            }
            leftHandObjects.Clear();
            leftHandObjects.Add(model);
            model.transform.parent = tracking.leftHandAnchor;
        }
        else
        {
            foreach (GameObject go in rightHandObjects)
            {
                Destroy(go);
            }
            rightHandObjects.Clear();
            rightHandObjects.Add(model);
            model.transform.parent = tracking.rightHandAnchor;
        }
        model.transform.localPosition = position;
        model.transform.localRotation = Quaternion.Euler(-90, 0, 0);
    }
    
    private void ScaleObject(GameObject model)
    {
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.Euler(-90, 0, 0);
        model.transform.localScale = Vector3.one;
        
        List<Vector3> toTest = new List<Vector3>();
        foreach (MeshRenderer renderer in model.GetComponentsInChildren<MeshRenderer>())
        {
            toTest.Add(renderer.bounds.max);
            toTest.Add(renderer.bounds.min);
        }
        foreach (SkinnedMeshRenderer renderer in model.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            toTest.Add(renderer.bounds.max);
            toTest.Add(renderer.bounds.min);
        }
        
        Vector3 min = Vector3.one * int.MaxValue;
        Vector3 max = Vector3.one * int.MinValue;
        foreach (Vector3 v in toTest)
        {
            if (min.x > v.x) min.x = v.x;
            if (min.y > v.y) min.y = v.y;
            if (min.z > v.z) min.z = v.z;
                
            if (max.x < v.x) max.x = v.x;
            if (max.y < v.y) max.y = v.y;
            if (max.z < v.z) max.z = v.z;
        }
        
        Vector3 diff = max - min;
        float size = Mathf.Max(Mathf.Max(diff.x, diff.y), diff.z);
        model.transform.localScale = Vector3.one * 1.0f / size * objectScale;
        model.transform.localPosition = Vector3.up * -min.y / size * objectScale;
    }
    
    private void SetVoiceInput(bool on)
    {
        voiceCount += on ? 1 : -1;
        if (voice.Active && (voiceCount == 0))
        {
            voice.Deactivate();
        }
        else if (!voice.Active && (voiceCount > 0))
        {
            voice.Activate();
        }
    }
}
