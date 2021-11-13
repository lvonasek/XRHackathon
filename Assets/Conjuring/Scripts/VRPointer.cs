using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class VRPointer : MonoBehaviour
{
    public LineRenderer laser;
    public GameObject pointer;
    public static bool showLaser = true;

    private GraphicRaycaster raycaster;
    private OVRCameraRig vrCamera;
    private InputField activeInput;
    private TouchScreenKeyboard keyboard;

    void Update()
    {
        // Get raycaster
        if (raycaster == null)
        {
            raycaster = FindObjectOfType<GraphicRaycaster>();
        }
        if (raycaster == null)
        {
            return;
        }

        // Get VR camera
        if (vrCamera == null)
        {
            vrCamera = FindObjectOfType<OVRCameraRig>();
        }
        if (vrCamera == null)
        {
            return;
        }

        // Update laser
        laser.enabled = OVRManager.hasInputFocus && showLaser;
        laser.SetPosition(0, transform.position + transform.forward * 0.25f);
        laser.SetPosition(1, transform.position + transform.forward * 2.0f);
        pointer.SetActive(false);

        // Get pointer position
        EventSystem.current.SetSelectedGameObject(null);
        PointerEventData data = new PointerEventData(EventSystem.current);
        Vector3 position3d = transform.position + transform.forward * 2.0f;
        Vector3 position2d = Camera.main.WorldToScreenPoint(position3d);
        if (Application.isEditor)
        {
            position2d = Input.mousePosition;
        }

        // Do raycasting
        data.position = position2d;
        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(data, results);

        if (TouchScreenKeyboard.visible)
        {
            activeInput.text = keyboard.text;
        }
        else
        {
            // Select item under cursor
            foreach (RaycastResult result in results)
            {
                laser.SetPosition(1, result.worldPosition);
                pointer.SetActive(true);
                pointer.transform.position = result.worldPosition;
                if ((result.gameObject.GetComponent<Button>() != null) || (result.gameObject.GetComponent<InputField>() != null))
                {
                    EventSystem.current.SetSelectedGameObject(result.gameObject);
                }
            }

            // Click
            if (OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
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
    }
}
