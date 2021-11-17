using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicBall : MonoBehaviour
{
    [SerializeField]
    private TextMesh handText;
    
    private List<GameObject> handObjects;
    private List<GameObject> toDestroy;
    private string lastFeedback;
    private string toRestore;
    
    void Awake()
    {
        if (handObjects == null)
        {
            handObjects = new List<GameObject>();
            toDestroy = new List<GameObject>();
            toRestore = "";
        }
    }
    
    void Update()
    {
        UpdateObjects();
        UpdateText();
    }
    
    private void UpdateObjects()
    {
        // scale-out
        bool ok = true;
        foreach (GameObject go in toDestroy)
        {
            go.transform.localScale = Vector3.Lerp(go.transform.localScale, Vector3.zero, 0.1f);
            if (go.transform.localScale.magnitude > 0.01f)
            {
                ok = false;
            }
        }

        // scale-in
        if (ok)
        {
            foreach (GameObject go in handObjects)
            {
                go.transform.localScale = Vector3.Lerp(go.transform.localScale, Vector3.one, 0.1f);
            }
        }
        
        // memory cleanup
        if (ok)
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
        if (transform.localScale.y > 0)
        {
            handText.transform.localPosition = -Vector3.up * 0.1f / transform.localScale.y;
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
