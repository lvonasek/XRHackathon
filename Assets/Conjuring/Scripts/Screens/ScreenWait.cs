using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Monitoring authentication process and decision of next screen to be shown
 */
public class ScreenWait : MonoBehaviour
{
    [SerializeField]
    private GameObject screenLogin;
    [SerializeField]
    private GameObject screenInstruction;
    [SerializeField]
    private GameObject gameLogic;
    [SerializeField]
    private SketchfabIntegration sketchfab;

    void Update()
    {
        if (!sketchfab.GetAuth().isWorking())
        {
            if (sketchfab.GetAuth().isUserLogged())
            {
                screenInstruction.SetActive(true);
                gameLogic.SetActive(true);
            }
            else
            {
                screenLogin.SetActive(true);
            }
            gameObject.SetActive(false);
        }
    }
}
