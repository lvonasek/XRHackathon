using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/**
 * Using controller or hand, this class integrates usage of UI objects
 */
public class VRPointer : MonoBehaviour
{
    [SerializeField]
    private LineRenderer laser;
    [SerializeField]
    private GameObject pointer;
    [SerializeField]
    private OVRSkeleton skeleton;

    private GraphicRaycaster raycaster;
    private OVRCameraRig vrCamera;
    private InputField activeInput;
    private TouchScreenKeyboard keyboard;

    private List<RaycastResult> results;
    private Vector3 lastPosition;
    private float lastTouch;
    private bool touch;

    void Update()
    {
        if (InitObjects())
        {
            UpdateLaser();
            UpdateRaycasting();

            if (TouchScreenKeyboard.visible)
            {
                activeInput.text = keyboard.text;
            }
            else
            {
                UpdateUISelection();
                UpdateUIAction();
            }
        }
    }

    /**
     * Ensure all necessary objects are instantiated
     */
    private bool InitObjects()
    {
        if (Application.isEditor)
        {
            return false;
        }

        // Get raycaster
        if (raycaster == null)
        {
            raycaster = FindObjectOfType<GraphicRaycaster>();
        }
        if (raycaster == null)
        {
            return false;
        }

        // Get VR camera
        if (vrCamera == null)
        {
            vrCamera = FindObjectOfType<OVRCameraRig>();
        }
        if (vrCamera == null)
        {
            return false;
        }

        return true;
    }

    /**
     * Detects if hand mode is activated
     */
    private bool IsHandModeOn()
    {
        return skeleton.IsDataValid || (transform.position.magnitude < 0.01f);
    }

    /**
     * Updates laser pointer line and detects if there was click gesture used
     */
    private void UpdateLaser()
    {
        touch = false;
        if (IsHandModeOn())
        {
            Vector3 index = Vector3.zero;
            Vector3 thumb = Vector3.zero;
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
            }
            laser.enabled = false;
            laser.SetPosition(0, (thumb + index) * 0.5f);
            laser.SetPosition(1, index);

            if ((index - thumb).magnitude < 0.01f)
            {
                if (Time.time - lastTouch > 1)
                {
                    lastTouch = Time.time;
                    touch = true;
                }
            }
        }
        else
        {
            laser.enabled = OVRManager.hasInputFocus;
            laser.SetPosition(0, transform.position + transform.forward * 0.25f);
            laser.SetPosition(1, transform.position + transform.forward * 2.0f);
        }
    }

    /**
     * Raycasts from the hand/controller into UI
     */
    private void UpdateRaycasting()
    {
        // Reset objects
        pointer.SetActive(false);
        if (!IsHandModeOn())
        {
            EventSystem.current.SetSelectedGameObject(null);
        }

        // Get pointer position
        PointerEventData data = new PointerEventData(EventSystem.current);
        Vector3 position3d = laser.GetPosition(1);
        Vector3 position2d = Camera.main.WorldToScreenPoint(position3d);
        if (Application.isEditor)
        {
            position2d = Input.mousePosition;
        }

        // Do raycasting
        data.position = position2d;
        results = new List<RaycastResult>();
        raycaster.Raycast(data, results);
    }

    /**
     * Does a UI action if controller/hand triggered it
     */
    private void UpdateUIAction()
    {
        if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch) || touch)
        {
            GameObject selected = EventSystem.current.currentSelectedGameObject;
            if (selected != null)
            {
                Button button = selected.GetComponent<Button>();
                if (button != null)
                {
                    button.onClick.Invoke();
                }
                InputField input = selected.GetComponent<InputField>();
                if (input != null)
                {
                    activeInput = input;
                    keyboard = TouchScreenKeyboard.Open(input.text);
                }
            }
        }
    }

    /**
     * Selects UI object under the cursor and updates laser pointer
     */
    private void UpdateUISelection()
    {
        foreach (RaycastResult result in results)
        {
            float lerp = IsHandModeOn() ? 0.1f : 1.0f;
            lastPosition = Vector3.Lerp(lastPosition, result.worldPosition, lerp);
            laser.SetPosition(1, lastPosition);
            laser.enabled = (laser.GetPosition(1) - laser.GetPosition(0)).magnitude > 0.5f;
            pointer.SetActive(true);
            pointer.transform.position = lastPosition;
            if ((result.gameObject.GetComponent<Button>() != null) || (result.gameObject.GetComponent<InputField>() != null))
            {
                EventSystem.current.SetSelectedGameObject(result.gameObject);
            }
        }
    }
}
