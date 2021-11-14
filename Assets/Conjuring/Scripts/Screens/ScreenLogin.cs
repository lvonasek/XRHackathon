using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    
    private void OnEnable()
    {
        username.text = sketchfab.GetUsername();
    }
    
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

    public void Register()
    {
        Application.OpenURL("https://sketchfab.com/");
    }
}
