using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Magic ball state handling, assigning and destroying objects
 */
public class MagicBall : MonoBehaviour
{
    public enum Status
    {
        IDLE,       // ball is not visible
        ACTIVE,     // ball is visible and microphone is on
        DOWNLOAD,   // ball is bigger 3D model is downloading
        BUILD,      // ball is bigger 3D model is building
        INSTANCE,   // ball is bigger 3D model is preparing
        DONE        // ball is not visible
    };

    [SerializeField]
    private float ballOffset = 0.1f;
    [SerializeField]
    private float ballSizeBig = 0.25f;
    [SerializeField]
    private float ballSizeNormal = 0.1f;
    [SerializeField]
    private float scalingSpeed = 0.1f;

    [SerializeField]
    private TextMesh handText;
    [SerializeField]
    private OVRSkeleton skeleton;

    private List<GameObject> handObjects;
    private List<GameObject> toDestroy;
    private string lastFeedback;
    private string toRestore;
    private Status status;
    private float timestamp;

    void Awake()
    {
        if (handObjects == null)
        {
            handObjects = new List<GameObject>();
            toDestroy = new List<GameObject>();
            toRestore = "";
            status = Status.IDLE;
        }
    }

    void Update()
    {
        UpdateMagicBallPose();
        UpdateMagicBallScale();
        UpdateObjects();
        UpdateText();
    }

    public Status GetStatus()
    {
        return status;
    }

    public void SetStatus(Status status)
    {
        this.status = status;
    }

    /**
     * Detects if hand mode is activated
     */
    private bool IsHandModeOn()
    {
        if (Application.isEditor)
        {
            return false;
        }
        return skeleton.IsDataValid || (transform.parent.position.magnitude < 0.01f);
    }

    /**
     * Updates magic ball position, rotation and assign it to objects
     */
    private void UpdateMagicBallPose()
    {
        if (IsHandModeOn())
        {
            Vector3 index = Vector3.zero;
            Vector3 thumb = Vector3.zero;
            Vector3 position = Vector3.zero;
            Quaternion rotation = Quaternion.identity;
            foreach (OVRBone bone in skeleton.Bones)
            {
                if (bone.Id == OVRSkeleton.BoneId.Hand_IndexTip)
                {
                    index = bone.Transform.position;
                }
                if (bone.Id == OVRSkeleton.BoneId.Hand_ThumbTip)
                {
                    thumb = bone.Transform.position;
                }
                position += bone.Transform.position;
            }
            position /= (float)skeleton.Bones.Count;
            rotation = Quaternion.LookRotation(index - thumb, Vector3.up);
            transform.rotation = rotation;
            transform.position = position + transform.up * ballOffset;

            foreach (GameObject go in handObjects)
            {
                go.transform.rotation = rotation;
                go.transform.position = position;
            }
        }
        else
        {
            transform.localPosition = Vector3.up * ballOffset;
            transform.localRotation = Quaternion.identity;

            foreach (GameObject go in handObjects)
            {
                go.transform.localPosition = Vector3.zero;
                go.transform.localRotation = Quaternion.identity;
            }
        }
    }

    /**
     * Updates magic ball scale based on current status
     */
    private void UpdateMagicBallScale()
    {
        if (!OVRManager.hasInputFocus)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, scalingSpeed);
            return;
        }

        switch (status)
        {
            case Status.DONE:
            case Status.IDLE:
                handText.transform.parent = transform;
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, scalingSpeed);
                break;
            case Status.ACTIVE:
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * ballSizeNormal, scalingSpeed);
                break;
            case Status.BUILD:
            case Status.DOWNLOAD:
            case Status.INSTANCE:
                handText.transform.parent = transform.parent;
                handText.transform.localScale = Vector3.one * 0.02f;
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * ballSizeBig, scalingSpeed);
                break;
        }
    }

    /**
     * Updates objects transition and handle status BUILD->DONE transition
     */
    private void UpdateObjects()
    {
        // set visibility
        foreach (GameObject go in handObjects)
        {
            go.SetActive(OVRManager.hasInputFocus);
        }

        // scale-out
        bool done = true;
        foreach (GameObject go in toDestroy)
        {
            go.transform.localScale = Vector3.Lerp(go.transform.localScale, Vector3.zero, scalingSpeed);
            if (go.transform.localScale.x > 0.01f)
            {
                done = false;
            }
        }

        // scale-in
        if (done)
        {
            foreach (GameObject go in handObjects)
            {
                go.transform.localScale = Vector3.Lerp(go.transform.localScale, Vector3.one, scalingSpeed);
                if (go.transform.localScale.x < 0.99f)
                {
                    timestamp = Time.time;
                    done = false;
                }
            }

            float shownUp = Time.time - timestamp;
            if (!done && (status == Status.BUILD))
            {
                status = Status.INSTANCE;
            } else if (done && (status == Status.INSTANCE) && (shownUp > 1))
            {
                status = Status.DONE;
            }
        }

        // memory cleanup
        if (done)
        {
            foreach (GameObject go in toDestroy)
            {
                Destroy(go);
            }
            toDestroy.Clear();
        }
    }

    /**
     * Updates text string, position and face it to the camera
     */
    private void UpdateText()
    {
        if (transform.localScale.y > 0.001)
        {
            handText.transform.position = transform.position - Vector3.up * ballOffset;
            handText.transform.rotation = Camera.main.transform.rotation;
        }
        if (lastFeedback != null)
        {
            handText.text = lastFeedback;
            toRestore = lastFeedback;
            lastFeedback = null;
        }
    }

    /**
     * Add object to the hand (making it follow the hand)
     */
    public void AssignToHand(GameObject instance)
    {
        GameObject model = new GameObject();
        instance.transform.parent = model.transform;
        handObjects.Add(model);

        Vector3 position = model.transform.localPosition;
        Quaternion rotation = model.transform.localRotation;
        model.transform.parent = transform.parent;
        model.transform.localPosition = position;
        model.transform.localRotation = rotation;
        model.transform.localScale = Vector3.zero;
    }

    /**
     * Remove all hand objects, will be destroyed by UpdateObjects() method
     */
    public void DestroyHandObjects()
    {
        toDestroy.AddRange(handObjects);
        handObjects.Clear();
    }

    /**
     * Extend current text feedback with additional string
     */
    public void AddText(string text)
    {
        lastFeedback = toRestore + "\n" + text;
    }

    /**
     * Set current feedback text
     */
    public void SetText(string text)
    {
        lastFeedback = text;
    }
}
