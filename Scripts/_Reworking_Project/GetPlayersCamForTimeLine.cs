using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class GetPlayersCamForTimeLine : MonoBehaviour
{
    public CutSceneEnter cutSceneE;
    [SerializeField] GameObject playersVCam;
    [SerializeField] bool enablePlayer=false;//to enable player before cut scene ends
    void Start()
    {
        playersVCam = cutSceneE.playersVcam;
        // Get the CinemachineVirtualCamera component from the playersVCam GameObject
        cutSceneE.EnablePlayerForTimeLine();
        CinemachineVirtualCamera playersVirtualCamera = playersVCam.GetComponent<CinemachineVirtualCamera>();

        

        // Get the CinemachineVirtualCamera component from the current GameObject
        CinemachineVirtualCamera currentVirtualCamera = GetComponent<CinemachineVirtualCamera>();

        // Copy the values from playersVirtualCamera to currentVirtualCamera
        currentVirtualCamera.m_Lens = playersVirtualCamera.m_Lens;
        currentVirtualCamera.m_Transitions = playersVirtualCamera.m_Transitions;
        currentVirtualCamera.Follow = playersVirtualCamera.Follow;
        currentVirtualCamera.LookAt = playersVirtualCamera.LookAt;
        currentVirtualCamera.transform.localPosition = playersVirtualCamera.transform.localPosition;

        
       // currentVirtualCamera.gameObject.transform.position = playersVCam.gameObject.transform.position;

        // enabling playablePlayer

       // cutSceneE.gameObject.SetActive(false);
        // You might also need to copy other properties like Priority, Follow, LookAt, etc., if applicable.

        // Alternatively, you can just set the priority to match the player's camera:
        // currentVirtualCamera.Priority = playersVirtualCamera.Priority;
    }


   
}
