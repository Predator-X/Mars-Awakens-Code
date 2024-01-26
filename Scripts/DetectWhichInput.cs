using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectWhichInput : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetJoystickNames().Length > 0)
        {
            Debug.Log("Gamepad is being used");
        }
        else
        {
            Debug.Log("Keyboard is being used");
        }
    }
}
