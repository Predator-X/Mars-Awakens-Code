using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Cover : MonoBehaviour
{
    InputAction cover;
    private InputActionAsset inputActionAsset;
    private InputActionMap playerActionMap;
    [SerializeField] float maxDistanceFromCover =3f , horizontalCoverDetecotrLength = 1f;
   // [SerializeField]
    public LayerMask coverLayerMask;
    [SerializeField] Transform highCoverDetectionTransform, rightCoverDetectionTransform, leftCoverDetectionTransform ,
        leftHighCoverDetector , rightHightCoverDetector;
    Vector3 coverHitPoint;
    Vector3 coverSurfaceDirection;
    public bool inHighCover , inCover , canAim , inRightHightSightCover, inLeftHightSightCover , hasExitCover = false;
    PlayerController playerController;

    [SerializeField] float coverDistance = 1.5f; // Maximum distance to detect cover.
    [SerializeField] float stickToCoverDistance = 0.7f; // Distance to stick the player to cover.
    private Vector3 currentCoverHitPoint;
    public GameObject currentCoverObject;

    void Awake()
    {
        inputActionAsset = this.GetComponent<PlayerInput>().actions;
        playerActionMap = inputActionAsset.FindActionMap("Player");

        playerController = GetComponent<PlayerController>();
    }
    private void Update()
    {
        if (inCover)
        {
            SetCoverType();
            if (!playerController.autoMoverActive)
            {
                InCoverMovementRestrictor();
            }
            

            RaycastHit hitinfo;
            if (Physics.Raycast(highCoverDetectionTransform.position, highCoverDetectionTransform.forward, out hitinfo, maxDistanceFromCover, coverLayerMask))
            {
                GetCoverSurfaceDirection(hitinfo.transform.position);
            }
        }

        if (hasExitCover)
        {
            // playerController.StartCoroutine()
           // StartCoroutine(playerController.moveCamToRight());
           
        }
    }

    private void FixedUpdate()
    {
        Debug.DrawRay(transform.position, transform.forward.normalized * maxDistanceFromCover, Color.blue);

        Debug.DrawRay(leftCoverDetectionTransform.position,
               leftCoverDetectionTransform.forward* horizontalCoverDetecotrLength , Color.red);

         Debug.DrawRay(rightCoverDetectionTransform.position,
            rightCoverDetectionTransform.forward* horizontalCoverDetecotrLength , Color.red);

        Debug.DrawRay(highCoverDetectionTransform.position, highCoverDetectionTransform.forward * horizontalCoverDetecotrLength, Color.red);

        Debug.DrawRay(leftHighCoverDetector.position, leftHighCoverDetector.forward * horizontalCoverDetecotrLength, Color.red);

        Debug.DrawRay(rightHightCoverDetector.position, rightHightCoverDetector.forward * horizontalCoverDetecotrLength, Color.red);

    }
    private void OnEnable()
    {
        
        cover = playerActionMap.FindAction("TakeCover");
        playerActionMap.FindAction("TakeCover").started += TakeCover;
        playerActionMap.FindAction("ExitCover").started += ExitCover;
    }
    private void OnDisable()
    {

        cover = playerActionMap.FindAction("TakeCover");
        playerActionMap.FindAction("TakeCover").started -= TakeCover;
        playerActionMap.FindAction("ExitCover").started -= ExitCover;
    }

    private void MoveCharacterToCover()
    {
        inCover = true;
       // playerController.BeginMoveToCover(coverHitPoint);
        this.GetComponent<PlayerController>().BeginMoveToCover(coverHitPoint);
        Debug.LogWarning("MoveCharacterToCover");
        
    }

    void TakeCover(InputAction.CallbackContext ctx)
    {
        // shootHasBeenPressed = true;
        Debug.Log(ctx.action.name + "Cover");
        if (IsNearCover())
        {

            SetCoverType();
            MoveCharacterToCover();
            Debug.Log("Taking Cover and?!");
        }
        if (ctx.action.name != "TakeCover") 
        {
            return;
 
        }

        if (ctx.performed)
        {
            Debug.Log("TakeCover performed");
            //if (IsNearCover())
            //{
                
            //    CoverTypeDetecotr();
            //    MoveCharacterToCover();
            //    Debug.Log("Taking Cover and?!");
            //}
            
        }
       
    }

    void ExitCover(InputAction.CallbackContext ctx)
    {
        if (inCover)
        {
            playerController.BeginExitCover();
            inCover = false;
            hasExitCover = true;
        }
    }



    private bool IsNearCover()
    {
        Debug.Log("IsNear Started !");
        RaycastHit hitinfo;
        if(Physics.Raycast(transform.position,transform.forward,out hitinfo, maxDistanceFromCover, coverLayerMask))
        {
            coverHitPoint = hitinfo.point;

            
            currentCoverObject = hitinfo.collider.gameObject;

            return true;
        }
        else
        {
            return false;
        }
        GetCoverSurfaceDirection(hitinfo.transform.position);
    }

    public void SetCoverType()
    {
       
      
        if (Physics.Raycast(highCoverDetectionTransform.position, highCoverDetectionTransform.forward, maxDistanceFromCover, coverLayerMask))
        {
            inHighCover = true;
           
            // Debug.Log("going into High Cover");
        }
        else 
        { 
            inHighCover = false;
            canAim = true;
           // Debug.Log("going into Low Cover");
        }
    }

    private void InCoverMovementRestrictor()
    {
        bool didLeftCoverDetectorHit = Physics.Raycast(leftCoverDetectionTransform.position,
            leftCoverDetectionTransform.forward, horizontalCoverDetecotrLength, coverLayerMask);

        bool didRightCoverDetectorHit = Physics.Raycast(rightCoverDetectionTransform.position,
            rightCoverDetectionTransform.forward, horizontalCoverDetecotrLength, coverLayerMask);

        inLeftHightSightCover = Physics.Raycast(leftHighCoverDetector.position,
            leftHighCoverDetector.forward, horizontalCoverDetecotrLength, coverLayerMask);

        inRightHightSightCover = Physics.Raycast(rightHightCoverDetector.position,
            rightHightCoverDetector.forward, horizontalCoverDetecotrLength, coverLayerMask);

        StickToCover();

        if (!didLeftCoverDetectorHit || !didRightCoverDetectorHit)
        {
            //Means we are at the covers corner
            if (inHighCover)
            {
                canAim = true;
            }
            //Seting Move Directions
            if (!didLeftCoverDetectorHit)
            {
                SetCharacterMoverCoverDirections(coverSurfaceDirection, -coverSurfaceDirection);
            }
            else
            {
                SetCharacterMoverCoverDirections(coverSurfaceDirection, coverSurfaceDirection);
            }
        }
        else
        {
            if (inHighCover)
            {
                canAim = false;
            }
            SetCharacterMoverCoverDirections(coverSurfaceDirection, Vector3.zero);
        }

        
    }

    void StickToCover()
    {
       
        RaycastHit hitinfo;
        if (Physics.Raycast(transform.position, transform.forward, out hitinfo, maxDistanceFromCover, coverLayerMask))
        {
            if (stickToCoverDistance < Vector3.Distance(hitinfo.point, transform.position) && currentCoverObject == hitinfo.collider.gameObject)
            {
             //   ReAllignToCover(hitinfo);
              
                currentCoverHitPoint = hitinfo.point;

                // Calculate the direction from the player to the cover.
                Vector3 toCover = currentCoverHitPoint - this.gameObject.transform.position;
                // Calculate the desired position for the player to stick to cover.
                Vector3 desiredPosition = currentCoverHitPoint - toCover.normalized * stickToCoverDistance;
                  this.gameObject.transform.position = desiredPosition;
               // this.gameObject.GetComponent<Rigidbody>().velocity += desiredPosition;
            }

        }
    }

  public  void RotatePlayerToCover(GameObject takeLookAtThis)
    {
       

        Vector3 direction = (takeLookAtThis.transform.position - transform.position).normalized;

        direction.y = 0f;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
    }

    void ReAllignToCover(RaycastHit hit)
    {
        // Calculate the rotation to align the player with the cover's normal.
        Quaternion desiredRotation = Quaternion.LookRotation(hit.normal, Vector3.up);
    }


    private void SetCharacterMoverCoverDirections(Vector3 moveDirection , Vector3 directionToProhibit)
    {
        playerController.inCoverMoveDirection = moveDirection;
        playerController.inCoverProhebitedDirection = directionToProhibit;
    }

    //Finding a tangent from a given normal to get perpendicular between two vectors
    public Vector3 GetCoverSurfaceDirection(Vector3 hitNormal)
    {
        //Debug.Log("-------------------------------------cover Surface setted to" + coverSurfaceDirection);
        coverSurfaceDirection = Vector3.Cross(hitNormal, Vector3.up).normalized;
       
        return coverSurfaceDirection;
    }

}
