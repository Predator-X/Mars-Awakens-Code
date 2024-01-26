using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Animations;

public class OnTriggerEnterEnable : MonoBehaviour
{
    [SerializeField]
    GameObject objToEnable;
    [SerializeField]                                      //Removes this gameObjects sphere collider 
    bool grabPlayersCamera = false , grabonDellay=false , removeColliderOnTrigger;
    [SerializeField] float grabDellayTimer=3f , realisePlCameraAfter = 3f;
    [SerializeField]
    Transform lookAtObj;
    Transform lookAtTransformHolderPrefious;

    GameObject playersVcamFallow;
    bool readyForGettingCam = false;

    //For Zooming in and out 
    public CinemachineVirtualCamera virtualCamera;
    public float zoomInFOV = 30f; // Adjust this value for the zoom-in FOV
    public float zoomOutFOV = 50f; // Adjust this value for the original FOV (zoom-out)
    public float zoomInDuration = 2f; // Duration of the zoom In effect
    public float zoomOutDuration = 2f; // Duration of the zoom Out effect

    private void Update()
    {
        if (readyForGettingCam)
        {
            
        }
        
    }
    private void OnTriggerEnter(Collider other)
    {
        // Check if the playyer has entered
        if (other.CompareTag("Player"))
        {
            if (removeColliderOnTrigger)
            {
                DestroyThisObjectsCollider();
            }
            objToEnable.SetActive(true);
            if ( !grabonDellay && grabPlayersCamera && lookAtObj !=null)
            {  
               
                //playersVcamFallow = other.gameObject.GetComponent<PlayerController>().fallowCamera;
                playersVcamFallow = other.transform.Find("MoveCamera (1)").gameObject;
               // readyForGettingCam = true;
                SetUpCameraToLookAt(playersVcamFallow);
                
            }
            else if(grabonDellay && grabPlayersCamera && lookAtObj != null)
            {
                playersVcamFallow = other.transform.Find("MoveCamera (1)").gameObject;
                StartCoroutine(SetUpCameraToLookAtAtDellay(playersVcamFallow, grabDellayTimer));
            }
        }
    }
    //void SetUpCameraToLookAt()
    //{
    //    playersVcamFallow.GetComponent<Cinemachine3rdPersonFollow>().enabled = false;
    //    playersVcamFallow.GetComponent<CinemachineVirtualCamera>().LookAt = lookAtObj;

    //}
    void SetUpCameraToLookAt(GameObject camObj)
    {
        camObj.GetComponent<Cinemachine3rdPersonAim>().enabled = false;
        lookAtTransformHolderPrefious = camObj.GetComponent<CinemachineVirtualCamera>().LookAt;
        camObj.GetComponent<CinemachineVirtualCamera>().LookAt = lookAtObj;
        
       // CinemachineComposer composer = camObj.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineComposer>();
     CinemachineComposer composer=   camObj.GetComponent<CinemachineVirtualCamera>().AddCinemachineComponent<CinemachineComposer>();
        camObj.GetComponent<CinemachineVirtualCamera>().m_LookAt = lookAtObj;
        
        readyForGettingCam = false;
        // Start the zoom-in and zoom-out coroutine
        StartCoroutine(ZoomInAndOutCoroutine());
        StartCoroutine(ReverseCamSettingsAfterTime(realisePlCameraAfter, camObj));
        Debug.Log("Reverse Started");

    }
    IEnumerator SetUpCameraToLookAtAtDellay(GameObject camObj, float timer)
    {
        yield return new WaitForSeconds(timer);
        camObj.GetComponent<Cinemachine3rdPersonAim>().enabled = false;
        lookAtTransformHolderPrefious = camObj.GetComponent<CinemachineVirtualCamera>().LookAt;
        camObj.GetComponent<CinemachineVirtualCamera>().LookAt = lookAtObj;

        // CinemachineComposer composer = camObj.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<CinemachineComposer>();
        CinemachineComposer composer = camObj.GetComponent<CinemachineVirtualCamera>().AddCinemachineComponent<CinemachineComposer>();
        camObj.GetComponent<CinemachineVirtualCamera>().m_LookAt = lookAtObj;

        readyForGettingCam = false;

        // Start the zoom-in and zoom-out coroutine
        StartCoroutine(ZoomInAndOutCoroutine());

        StartCoroutine(ReverseCamSettingsAfterTime(realisePlCameraAfter, camObj));
        Debug.Log("Reverse Started");
    }

    IEnumerator ReverseCamSettingsAfterTime(float timer , GameObject camObj)
    {
        yield return new WaitForSeconds(timer);
        camObj.GetComponent<CinemachineVirtualCamera>().LookAt = lookAtTransformHolderPrefious;
        camObj.GetComponent<Cinemachine3rdPersonAim>().enabled = true;
        camObj.GetComponent<CinemachineVirtualCamera>().AddCinemachineComponent<CinemachineComposer>().enabled=false;
        Debug.Log("Reverse Finisht!!!!");
    }

    void DestroyThisObjectsCollider()
    {
       Component collider = this.gameObject.GetComponent<SphereCollider>();
        if(collider != null)
        {
            Destroy(collider);
        }
    }

    //private System.Collections.IEnumerator ZoomInAndOutCoroutine()
    //{
    //    // Zoom in
    //    ChangeFOV(zoomInFOV);
    //    yield return new WaitForSeconds(2f); // Adjust the duration of zoom-in

    //    // Zoom out
    //    ChangeFOV(zoomOutFOV);
    //}

    private System.Collections.IEnumerator ZoomInAndOutCoroutine()
    {
        // Zoom in
        yield return ZoomInFOVSmoothly(zoomInFOV, zoomInDuration);

        // Zoom out
        yield return ZoomOutFOVSmoothly(zoomOutFOV, zoomOutDuration);
    }

    private System.Collections.IEnumerator ZoomInFOVSmoothly(float targetFOV, float duration)
    {
        virtualCamera = playersVcamFallow.GetComponent<CinemachineVirtualCamera>();
        float initialFOV = virtualCamera.m_Lens.FieldOfView;//virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_Lens.FieldOfView;
        float startTime = Time.time;

        while (Time.time - startTime < duration)
        {
            float t = (Time.time - startTime) / duration;
            float currentFOV = Mathf.Lerp(initialFOV, targetFOV, t);

            ChangeFOV(currentFOV);

            yield return null;
        }

        ChangeFOV(targetFOV); // Ensure the final value is set accurately
    }

    private System.Collections.IEnumerator ZoomOutFOVSmoothly(float targetFOV, float duration)
    {
        virtualCamera = playersVcamFallow.GetComponent<CinemachineVirtualCamera>();
        float initialFOV = virtualCamera.m_Lens.FieldOfView;//virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>().m_Lens.FieldOfView;
        float startTime = Time.time;

        while (Time.time - startTime < duration)
        {
            float t = (Time.time - startTime) / duration;
            float currentFOV = Mathf.Lerp(initialFOV, targetFOV, t);

            ChangeFOV(currentFOV);

            yield return null;
        }

        ChangeFOV(targetFOV); // Ensure the final value is set accurately
    }

    private void ChangeFOV(float targetFOV)
    {
        virtualCamera = playersVcamFallow.GetComponent<CinemachineVirtualCamera>();
        LensSettings lensSettings = virtualCamera.m_Lens;
        lensSettings.FieldOfView = targetFOV;
        virtualCamera.m_Lens = lensSettings;
    }
}
