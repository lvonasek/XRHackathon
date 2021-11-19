using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
 * Integrates login screen persistency and actions
 */
public class ScreenLogin : MonoBehaviour
{
    [SerializeField]
    private InputField username;
    [SerializeField]
    private InputField password;
    [SerializeField]
    private GameObject screenWait;
    [SerializeField]
    private SketchfabIntegration sketchfab;

    /**
     * Restore last used username
     */
    private void OnEnable()
    {
        username.text = sketchfab.GetUsername();
    }

    /**
     * Start logging process and move to waiting screen
     */
    public void Login()
    {
        try
        {
            sketchfab.Login(username.text, password.text);
            gameObject.SetActive(false);
            screenWait.SetActive(true);
        }
        catch (Exception e)
        {
            Debug.LogException(e, this);
        }
    }

    /**
     * Open Sketchfab registration page in a web browser
     */
    public void Register()
    {
        Application.OpenURL("https://sketchfab.com/");
    }
}
