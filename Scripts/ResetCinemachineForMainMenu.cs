using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetCinemachineForMainMenu : MonoBehaviour
{

    public GameObject cinemachine1, cinemachine2;
    // Start is called before the first frame update
    private void Awake()
    {
        cinemachine1.SetActive(true); cinemachine2.SetActive(false);
    }
}
