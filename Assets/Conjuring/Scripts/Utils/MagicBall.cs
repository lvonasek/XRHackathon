using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private float scalingSpeed = 0.1f;

    [SerializeField]
    private TextMesh handText;

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
        UpdateMagicBall();
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

    private void UpdateMagicBall()
    {
        switch (status)
        {
            case Status.DONE:
            case Status.IDLE:
                handText.transform.parent = transform;
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, scalingSpeed);
                break;
            case Status.ACTIVE:
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * 0.1f, scalingSpeed);
                break;
            case Status.BUILD:
            case Status.DOWNLOAD:
            case Status.INSTANCE:
                handText.transform.parent = transform.parent;
                handText.transform.localScale = Vector3.one * 0.02f;
                transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * 0.25f, scalingSpeed);
                break;

        }
    }

    private void UpdateObjects()
    {
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

    private void UpdateText()
    {
        if (transform.localScale.y > 0.001)
        {
            handText.transform.position = transform.position - Vector3.up * 0.1f;
            handText.transform.rotation = Camera.main.transform.rotation;
        }
        if (lastFeedback != null)
        {
            handText.text = lastFeedback;
            toRestore = lastFeedback;
            lastFeedback = null;
        }
    }

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

    public void DestroyHandObjects()
    {
        toDestroy = handObjects;
        handObjects = new List<GameObject>();
    }

    public void AddText(string text)
    {
        lastFeedback = toRestore + "\n" + text;
    }

    public void SetText(string text)
    {
        lastFeedback = text;
    }
}
