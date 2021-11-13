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
    private SketchfabIntegration sketchfab;
    
    public void Login()
    {
        sketchfab.Login(username.text, password.text);
    }

    public void Register()
    {
        Application.OpenURL("https://sketchfab.com/");
    }
}
