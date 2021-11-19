using Facebook.WitAi;
using Facebook.WitAi.Lib;
using Oculus.Voice;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Logic of conjuring objects using VoiceSDK and modified Sketchfab integration
 */
public class ModelConjuring : MonoBehaviour
{
    [SerializeField]
    private float objectScale = 0.2f;

    [SerializeField]
    private MagicBall leftHandMagicBall;
    [SerializeField]
    private MagicBall rightHandMagicBall;

    [SerializeField]
    private SketchfabIntegration sketchfab;
    [SerializeField]
    private OVRCameraRig tracking;
    [SerializeField]
    private AppVoiceExperience voice;

    private MagicBall targetAnchor;

    void OnDisable()
    {
        sketchfab.onFailed -= OnModelFailed;
        sketchfab.onFinished -= OnModelCreated;
        voice.events.OnRequestCreated.RemoveListener(OnVoiceRequest);
    }

    void OnEnable()
    {
        sketchfab.onFailed += OnModelFailed;
        sketchfab.onFinished += OnModelCreated;
        voice.events.OnRequestCreated.AddListener(OnVoiceRequest);

        if (Application.isEditor)
        {
            SetVoiceInput(true);
        }
    }

    void Update()
    {
        UpdateHand(leftHandMagicBall);
        UpdateHand(rightHandMagicBall);
    }

    /**
     * Set state of the magic ball and enable/disable voice input
     */
    void UpdateHand(MagicBall magicBall)
    {
        float distanceToHand = (magicBall.transform.position - tracking.centerEyeAnchor.position).magnitude;
        bool shouldActive = distanceToHand < 0.35f;

        if (shouldActive && (leftHandMagicBall.GetStatus() == MagicBall.Status.IDLE) && (rightHandMagicBall.GetStatus() == MagicBall.Status.IDLE))
        {
            targetAnchor = magicBall;
            targetAnchor.SetStatus(MagicBall.Status.ACTIVE);
            SetVoiceInput(true);
        } else if (!shouldActive && (magicBall.GetStatus() == MagicBall.Status.ACTIVE))
        {
            magicBall.SetStatus(MagicBall.Status.IDLE);
            SetVoiceInput(false);
        } else if (!voice.Active && (magicBall.GetStatus() == MagicBall.Status.ACTIVE))
        {
            magicBall.SetStatus(MagicBall.Status.DONE);
            SetVoiceInput(false);
        } else if (!shouldActive && (magicBall.GetStatus() == MagicBall.Status.DONE))
        {
            magicBall.SetStatus(MagicBall.Status.IDLE);
            SetVoiceInput(false);
        }
    }

    /**
     * Callback on successful model creation
     */
    void OnModelCreated(GameObject model, string modelName)
    {
        SetObjectTransform(model);
        targetAnchor.DestroyHandObjects();
        targetAnchor.AssignToHand(model);
        targetAnchor.SetText(modelName);
        targetAnchor.SetStatus(MagicBall.Status.BUILD);
    }

    /**
     * Callback on model creation fail
     */
    void OnModelFailed(string reason)
    {
        targetAnchor.AddText(reason);
        targetAnchor.SetStatus(MagicBall.Status.IDLE);
        SetVoiceInput(false);
    }

    /**
     * Callback providing voice request
     */
    void OnVoiceRequest(WitRequest request)
    {
        request.onFullTranscription += OnVoiceResponse;
    }

    /**
     * Callback providing said text as string, which is used as input for Sketchfab search query
     */
    void OnVoiceResponse(string text)
    {
        Debug.Log("Recognized test: " + text);
        text = text.ToLower();
        text = text.Replace(".", " ");
        text = text.Replace(",", " ");
        if (text.StartsWith("a ")) text = text.Substring(2);
        if (text.StartsWith("an ")) text = text.Substring(3);
        if (text.StartsWith("the ")) text = text.Substring(4);

        targetAnchor.SetStatus(MagicBall.Status.DOWNLOAD);
        targetAnchor.SetText(text);
        sketchfab.RequestObject(text);
    }

    /**
     * Set object's scale and center position
     */
    private void SetObjectTransform(GameObject model)
    {
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.Euler(-90, 0, 0);
        model.transform.localScale = Vector3.one;

        // get all bounding boxes
        List<Vector3> toTest = new List<Vector3>();
        foreach (MeshRenderer renderer in model.GetComponentsInChildren<MeshRenderer>())
        {
            renderer.GetComponent<MeshFilter>().mesh.RecalculateBounds();
            toTest.Add(renderer.bounds.max);
            toTest.Add(renderer.bounds.min);
        }
        foreach (SkinnedMeshRenderer renderer in model.GetComponentsInChildren<SkinnedMeshRenderer>())
        {
            renderer.sharedMesh.RecalculateBounds();
            toTest.Add(renderer.bounds.max);
            toTest.Add(renderer.bounds.min);
        }

        // calculate bounding box of whole object
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

        // set scale
        Vector3 diff = max - min;
        float size = Mathf.Max(Mathf.Max(diff.x, diff.y), diff.z);
        model.transform.localScale = Vector3.one * 1.0f / size * objectScale;

        // set position
        min = min / size * objectScale;
        max = max / size * objectScale;
        Vector3 position = (max + min) / 2.0f;
        position.y = min.y;
        model.transform.localPosition = -position;
    }

    /**
     * Enables or disables the voice input
     */
    private void SetVoiceInput(bool on)
    {
        if (voice.Active && !on)
        {
            voice.Deactivate();
        }
        else if (!voice.Active && on)
        {
            voice.Activate();
        }
    }
}
