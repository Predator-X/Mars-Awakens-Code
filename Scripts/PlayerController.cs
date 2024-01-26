//INHERITANCE - PlayerController Inherits From character class and POLYMORPHISM the move method
//This Class is used for manaching MainPlayer Character
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Animations.Rigging;

//For new InputSystem
using UnityEngine.InputSystem;
using System.Runtime.InteropServices.ComTypes;
using JetBrains.Annotations;
using UnityEditor;
using Cinemachine;
using TMPro;
using UnityEngine.AI;
using EnemyAI;
using UnityEngine.EventSystems;
using Unity.VisualScripting;
//using Cinemachine.Editor;
//using UnityEngine.InputSystem.Utilities;

public class PlayerController : Character , ICharacterMoverCover
{
    public CharacterController characterController;  //<-- characterController that is build in unity 
    #region ALL DECLARATIONS
    //Animations
    #region Animations DECLARATIONS
    Animator animator;
    int isWalkingHash;
    int isRunningHash;
    int isStarfeLeftHash;
    int isStarfeRightHash;
    [SerializeField] private Rig aimRig , rigRun;

[Tooltip("Adjust this value for the speed of the transition between animation layers.")]  
    [SerializeField] float layerTransitionSpeed = 1.0f; 

    private float aimRigWeight;
    private bool isAming = false;
    private bool _rotateOnMove = true;//when Aiming use diffrent method to rotate
    #endregion

    //Shooting ------------------Changed to ShootWithProTracer
    #region Shoot and Fire DECRARATIONS
    // ShootWithRaycast attack;
    ShootWithProTracer attack;
    private float nextFire;
    #endregion

    //Camera declarations
    #region Camera DECLARATIONS
    [SerializeField] public Camera cam;
    [SerializeField] private LayerMask aimColliderLayerMask = new LayerMask();
    [SerializeField] private Transform debugAimTransform;
    public GameObject aimCamera, fallowCamera;
    #endregion

    public int score;

    //Timer
    public float currentTime = 0;
    string text;

 

    //get all Enemys in scene for Saving
    GameObject[] onStartGameObjectsInScene;
  

    //Effect on camera
    Cinemachine.CinemachineImpulseSource impulseSource;

    //PlayerGUI
    #region Player GUI DECLARATION
    [SerializeField] GameObject crosshair;
    [SerializeField] GameObject popUpTextTMP;
    bool readyToPress = false;
    #endregion

    //for Activate CutScene on Press
    GameObject objHolder;

    //<<<>>> New Code for overriting move method 
    [SerializeField] float gravity = -9.81f;
    Vector3 velocity;

    //For Multiplayer
    #region For Multiplayer DECLARATION
    private InputActionAsset inputActionAsset;
    private InputActionMap playerActionMap;
    #endregion

    //For new InputSystem
    #region For New Input System DECLARATION
    // public InputAction playerInputAction;
    PlayerInput playerInput;
    private InputAction move, look,pause;
    [SerializeField]
    private GameObject lookAt;//Change it to Follow
    public Vector3 anglesHolder;
    //Changing Rotation camera fallow
    [SerializeField]
    private GameObject rootOfBodyToRotate;
    [SerializeField]
    private float turnSmoothTime = 0.1f, turnSmoothVelocity;
    Vector3 playerVelocity;

     [SerializeField]
    private InputAction fire;
    bool shootHasBeenPressed = false;

    [SerializeField]
    MultiplayerInput multiplayerInput;
    #endregion


    //for Move
    #region MOVE DECLARATIONS
    Vector2 mInput;
    bool movementPressed , runPressed , isWkeyPressed;
    Rigidbody rb;
    [SerializeField] float fallForce = -200f;              //<-- Fall Force as gravity does not do the job
    [SerializeField] bool isGrounded;
    [SerializeField] float keepPlayerAboveGroundHeight = 1.5f , raycastshootLengthDetection = 0.4f;
    [SerializeField] Transform shootRaycastToCheckGroundFrom;
    [SerializeField] LayerMask igonerLayerToCheckGround; // also ignore Players own layer  as to not hit its own collider when checking ground
    #endregion

    //Gravity
    //  float correctionY;

    //for TAKING COVER
    #region TAKING COVER DECLARATIONS
    [SerializeField] float inCoverMoveSpeed = 220f;
    Vector3 autoMoverTargerPos;
   public bool autoMoverActive ;
    bool canMove = false , moveBackIntoPossition = false , hasExitCover=false;
  [SerializeField] float autoMoverStoppingDistance = 0.7f;
    public bool inCover { get; set; }
   // Vector3 inCoverProhebitedDirection { get; set; }
    public Vector3 inCoverMoveDirection { get; set; }
    public Vector3 inCoverProhebitedDirection { get; set; }// Vector3 ICharacterMoverCover.inCoverProhebitedDirection { get; set; }
    Cover coverCS;

    bool wasInLeftCorner = false, wasInRightCorner = false;
    bool moveToCoverAnimationFinisht, needToMoveCameras=false;
    #endregion

    //for Stealth Kill
    #region STEALTH KILL DECLARATIONS
    bool isAbleToPerformStealhKill = false , movePlayerToKillPosition=false,takeDownInProgress=false;
    Animator enemysAnimator;
    Vector3 killPosition;
    GameObject enemyTarget;
    #endregion

    //For Elevator_________
    public string elevatorButonNameToLocate = null;

    //------------------
    bool isCamOnLeft = false, camMoveSideFinisht = false , fallowCamMoved, aimCamMoved;
#endregion

    void Awake()
    {
        #region Awake
        coverCS = GetComponent<Cover>();

        playerInput = this.GetComponent<PlayerInput>();
        inputActionAsset = playerInput.actions;
        playerActionMap = inputActionAsset.FindActionMap("Player");

        playerActionMap.FindAction("Move").performed += ctx
            =>
        {
            mInput = ctx.ReadValue<Vector2>();
            movementPressed = mInput.x != 0 || mInput.y != 0;
        };
        playerActionMap.FindAction("Run").performed += ctx => runPressed = ctx.ReadValueAsButton();
  
       /* multiplayerInput.Player.Move.performed += ctx =>
        {
            mInput = ctx.ReadValue<Vector2>();
            movementPressed = mInput.x != 0 || mInput.y != 0 ;
        }; */
        // multiplayerInput.Player.Run.performed += ctx => runPressed = ctx.ReadValueAsButton();

        PauseMenu.GameIsPaused = false;
        #endregion

    }


    // Start is called before the first frame update
    void Start()
    {
        #region Start
        CurrentSpeed = _speed;
       
        characterController.GetComponent<CharacterController>();
        attack = this.GetComponent<ShootWithProTracer>(); //this.GetComponent<ShootWithRaycast>();

        crosshair.SetActive(false);
        //seting animator reference
        animator = this.GetComponent<Animator>();
        //Seting the ID Reference
        isWalkingHash = Animator.StringToHash("isWalking");
      //  Debug.LogError("" +Animator.StringToHash("isWalking"));
        isRunningHash = Animator.StringToHash("isRunning");


       // cam = GameObject.FindGameObjectWithTag("CameraPlayer").GetComponent<Camera>();//<-------------Becouse Of Multiplayer

        onStartGameObjectsInScene = GameObject.FindGameObjectsWithTag("Enemy");

        rb = GetComponent<Rigidbody>();

        #endregion

    }



    void Update()
    {
        #region Update

        if (!PauseMenu.GameIsPaused && !takeDownInProgress)
        {
            attackAndCameraManagmentOnInput();

            //if (hasExitCover)
            //{
            //    SmoothlyMoveCameraSide(fallowCamera, 1f, 1f);

            //    hasExitCover = false;

            //}

            if (needToMoveCameras)
            {
                ResetCamerasSide();
            }
            if (readyToPress && Keyboard.current.enterKey.wasPressedThisFrame)//|| Gamepad.current.xButton.wasPressedThisFrame)
            {
                Debug.LogWarning("EnterPressed!");
                if (objHolder != null)
                {
                    objHolder.SetActive(true);

                    StartCoroutine(objHolder.GetComponent<CutSceneEnter>().FinishCutAndLoadNextScene());
                }

            }

            if (inCover  && !isAming)
            {


                RaycastHit hitinfo;
                if (Physics.Raycast(transform.position, transform.forward, out hitinfo, 5f, coverCS.coverLayerMask) && hitinfo.collider.gameObject == coverCS.currentCoverObject)
                {
                    transform.rotation = Quaternion.LookRotation(-hitinfo.normal);
                }

            }

            if (isAming)
            {

                HandleAimAnimations();


            }
            else
            {
                HandleMovement();
            }

            ViewFinder();
            // HandleRotation();
        }

        #region
    }

   

    void FixedUpdate()
    {
        #region Fixed Update
        if (!PauseMenu.GameIsPaused)
        {



            //Adding FallForce when not in the ground and keeping player above the ground, detecting ground using Raycast
            RaycastHit hitGround;
            if (Physics.Raycast(shootRaycastToCheckGroundFrom.position, -Vector3.up, out hitGround, raycastshootLengthDetection, ~igonerLayerToCheckGround))
            {
                //Keep player above the ground at certain height to avoid geting stuck at terrain
                float distanceToGroundDetected = hitGround.distance;
                if (distanceToGroundDetected < keepPlayerAboveGroundHeight)
                {
                    float correctionY = transform.position.y + (keepPlayerAboveGroundHeight - distanceToGroundDetected);
                    // correctionY = transform.position.y + (keepPlayerAboveGroundHeight - distanceToGroundDetected);
                    transform.position = new Vector3(transform.position.x, correctionY, transform.position.z);
                    isGrounded = true;
                }

                else if(distanceToGroundDetected>keepPlayerAboveGroundHeight)
                {
                    isGrounded = false;
                    rb.AddForce(Vector3.up * fallForce, ForceMode.Impulse);
                }
            }

            Debug.DrawRay(shootRaycastToCheckGroundFrom.position, -Vector3.up * raycastshootLengthDetection, Color.yellow);


            if (inCover && autoMoverActive)
            {
               

                MoveToCover();
            }



            if (takeDownInProgress)
            {
                GameObject takeLookAtThis = enemyTarget.gameObject;

                Vector3 direction = (takeLookAtThis.transform.position - transform.position).normalized;

                direction.y = 0f;
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 5f * Time.deltaTime);

                //_____Move Player to the strike/kill position_____
                if (movePlayerToKillPosition)
                {
                    Vector3 newPosition = transform.position + direction * 7f * Time.deltaTime;
                    transform.position = newPosition;
                    if (CalculateDistance(this.gameObject, takeLookAtThis) < 0.5f)
                    {
                        movePlayerToKillPosition = false;
                    }
                }


                playerVelocity = new Vector3(0f * CurrentSpeed, 0, 0f * CurrentSpeed);


                rb.velocity = transform.TransformDirection(playerVelocity);
            }


            if (inCover && !autoMoverActive)//<-- this has to be in here as if is higher in update() it rotates the whole player-- 
            {
                InCoverMove();
              
            }
            else if (!inCover || !takeDownInProgress)
            {
                Move(head, gun, body);
            }                               //<-------------------
        }

        #endregion
    }

    // | For new InpurtSystem 
    // V
    #region OnEnable && OnDisable 
    private void OnEnable()
    {
        /*
        playerInput.Enable();
        move = multiplayerInput.Player.Move;
        look = multiplayerInput.Player.Look;

        //  fire = multiplayerInput.Player.Fire;
        //  fire = multiplayerInput.Player.Fire.performed += Fire;
        fire.action.performed += Fire;

        move.Enable();
        look.Enable();
        */
        // fire.Enable();
        //----------------------New | ------------------------------------
        //                          V

        //multiplayerInput.Player.Enable(); //<-- this line added when doing animations 

        playerActionMap.Enable();
        move = playerActionMap.FindAction("Move");
        look = playerActionMap.FindAction("Look");

        playerActionMap.FindAction("Fire").started += Fire;
        playerActionMap.FindAction("Aim").started += Aim;
        playerActionMap.FindAction("Aim").canceled += AimCanceled;
        playerActionMap.FindAction("StealthKill").started += StealthKill;

        playerActionMap.FindAction("PauseMenu").started += PauseTheGame;
        playerActionMap.FindAction("PauseMenu1").canceled += PauseTheGame;

        // fire.action.performed += Fire;

        move.Enable();
        look.Enable();

    }
    private void OnDisable()
    {
        //multiplayerInput.Player.Disable(); //<-- this line added when doing animations 
       // playerInput.Disable();
        move.Disable();
        look.Disable();
       
        //----------------------New | ------------------------------------
        //                          V
        playerActionMap.FindAction("Fire").started -= Fire;
        playerActionMap.FindAction("Aim").started -= Aim;
        playerActionMap.FindAction("Aim").canceled -= AimCanceled;
        playerActionMap.FindAction("StealthKill").started -= StealthKill;

        playerActionMap.FindAction("PauseMenu").canceled -= PauseTheGame;
    }

    public void DisableControls()
    {

        playerInput.SwitchCurrentActionMap("NoControls");//inputActionAsset.FindActionMap("NoControls");
    }

    public void EnableControls()
    {
        //playerActionMap = inputActionAsset.FindActionMap("Player");
        playerInput.SwitchCurrentActionMap("Player");
    }
    #endregion

    private void SetAllAnimatorLayersWeightTo(int layerWeight)
    {
        int layerCount = animator.layerCount;

       for (int i =0; i <= layerCount; i++)
        {
            animator.SetLayerWeight(i, layerWeight);
        }
    }

    private void SetAllAnimatorParametersBoolTo(bool trueOrfalse)
    {
        int layerCount = animator.parameterCount;

        for (int i = 0; i <= layerCount; i++)
        {
            animator.SetBool(i, trueOrfalse);
        }
    }

  

    #region MAIN MOVEMENT && ROTATION
    // POLYMORPHISM
    protected override void Move(GameObject head, GameObject gun, GameObject body)  //<<<|>>> New Code added_________________________________________________________ to Move() method in character class by base.Move();
    {
        #region Move

        // Vector3 moveInput = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        // A
        // |
        // V
        //for New InputSystem -------------------------------------------
        Vector3 moveInput = move.ReadValue<Vector2>();
         
            playerVelocity = new Vector3(moveInput.x * CurrentSpeed, 0, moveInput.y * CurrentSpeed);
      
            Vector3 moveVector = transform.TransformDirection(moveInput);
            rb.velocity = transform.TransformDirection(playerVelocity);
            moveVector = moveVector * CurrentSpeed * Time.deltaTime;                                                                //characterController.Move(moveVector * CurrentSpeed * Time.deltaTime);
            velocity = velocity * Time.deltaTime;
        
       
       

      //  Debug.Log("Input x:" + moveInput.x + "  | input y:" + moveInput.y);
        // Debug.Log(isSpaceKeyHeld+"_________________________");
        bool isSpaceKeyHeld;//= multiplayerInput.Player.Space.ReadValue<float>() > 0.1f;  //---------------------------------
        isSpaceKeyHeld = playerActionMap.FindAction("Space").ReadValue<float>() > 0.1f;
        /*
            if (characterController.isGrounded)    
        {
            velocity.y = - 1f;
           // bool isSpaceKeyHeld = multiplayerInput.Player.Space.ReadValue<float>() >0.1f; // old input system ---->// if (Input.GetKeyDown(KeyCode.Space))
          if(isSpaceKeyHeld)
            {
                velocity.y = jumpForce;
            }
        }
        else if(!isSpaceKeyHeld && !characterController.isGrounded)
        {
            velocity.y -= gravity * -2f * Time.deltaTime;   // <--to calculate gravity : y -= gravity * -2f * Time.deltatime; but calculate only on ground 
           // rb.AddForce(transform.up * gravity * -2f);
        }
        */

     


        //--Jumping
        // Jumping
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }

        HandleRotation();                                                                    // characterController.Move(velocity * Time.deltaTime);
        base.Move(head, gun, body);                                                                                                                    // base.Move(head, gun, body);                   //<-- POLYMORPHISM adding code to Move() method that inherits from character class 
        #endregion
    }


    void HandleMovement()
    {
        #region Player Movement Animation
        bool isRunning = animator.GetBool(isRunningHash);
        bool isWalking = animator.GetBool(isWalkingHash);

        if (movementPressed && !isWalking)
        {
            // animator.SetBool(isWalkingHash, true);
            animator.SetBool("isWalking", true);

        }
        if (!movementPressed || Input.GetKeyUp(KeyCode.W) && !isWalking || playerVelocity.magnitude < 0.2f)//<-- this(keyCode) becouse new input system sucks and animation gets stuck without this when using a keyboard 
        {
            // animator.SetBool(isWalkingHash, false);
            animator.SetBool("isWalking", false);
        }
        if ((movementPressed && runPressed) && !isRunning)
        {
            //  animator.SetBool(isRunningHash, true);
            animator.SetBool("isRunning", true);
        }
        if ((!movementPressed || !runPressed) && isRunning)
        {
            // animator.SetBool(isRunningHash, false);
            animator.SetBool("isRunning", false);
        }

        #endregion
    }

    void HandleAimAnimations()
    {
        #region Player Aim Animations
        Vector2 moveInput = move.ReadValue<Vector2>();

        if (moveInput.x < 0f)
        {
            animator.SetBool("isAimWalkingF", false);
            animator.SetBool("isStarfeRight", false);
            animator.SetBool("isWalkingBackwards", false);
            animator.SetBool("isStarfeLeft", true);
            /*    if(aimCamera.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<Cinemachine3rdPersonFollow>().CameraSide <= 1)
                {
                    aimCamera.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<Cinemachine3rdPersonFollow>().CameraSide = Mathf.Lerp(aimCamera.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<Cinemachine3rdPersonFollow>().CameraSide, 0f, Time.deltaTime * 5f);
                }
               */

        }
        if (moveInput.x > 0f)
        {
            animator.SetBool("isAimWalkingF", false);
            animator.SetBool("isStarfeLeft", false);
            animator.SetBool("isStarfeRight", true);
            animator.SetBool("isWalkingBackwards", false);
            /*   if (aimCamera.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<Cinemachine3rdPersonFollow>().CameraSide >= 0)
               {
                   aimCamera.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<Cinemachine3rdPersonFollow>().CameraSide = Mathf.Lerp(aimCamera.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<Cinemachine3rdPersonFollow>().CameraSide, 1f, Time.deltaTime * 5f);
               }
               */
        }
        if (moveInput.x == 0f && moveInput.y == 0f || playerVelocity.magnitude < 0.05f)
        {
            animator.SetBool("isStarfeLeft", false);
            animator.SetBool("isStarfeRight", false);
            animator.SetBool("isAimWalkingF", false);
            animator.SetBool("isWalkingBackwards", false);
            /*   if (aimCamera.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<Cinemachine3rdPersonFollow>().CameraSide >= 0)
               {
                   aimCamera.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<Cinemachine3rdPersonFollow>().CameraSide = Mathf.Lerp(aimCamera.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<Cinemachine3rdPersonFollow>().CameraSide, 1f, Time.deltaTime * 5f);
               }
            */
        }
        if (moveInput.y > 0f)
        {
            animator.SetBool("isStarfeLeft", false);
            animator.SetBool("isStarfeRight", false);
            animator.SetBool("isWalkingBackwards", false);
            animator.SetBool("isAimWalkingF", true);
        }
        if (moveInput.y < 0f)
        {
            animator.SetBool("isStarfeLeft", false);
            animator.SetBool("isStarfeRight", false);
            animator.SetBool("isAimWalkingF", false);
            animator.SetBool("isWalkingBackwards", true);
        }
        #endregion
    }


    #region ROTATION Methods
    void HandleRotation()
    {
        #region HandleRotation
        Vector3 moveInput = move.ReadValue<Vector2>();
        Vector3 lookInput = look.ReadValue<Vector2>();
        lookAt.transform.rotation *= Quaternion.AngleAxis(lookInput.x * mouseSensityvityX, Vector3.up);
        lookAt.transform.rotation *= Quaternion.AngleAxis(lookInput.y * mouseSensityvityY, Vector3.left);

        var angles = lookAt.transform.localEulerAngles;
        angles.z = 0;
        anglesHolder = angles;
        var angle = lookAt.transform.localEulerAngles.x;

        //Clamp the Up/Down rotation
        if (angle > 180 && angle < 340)
        {
            angles.x = 340;
        }
        else if (angle < 180 && angle > 40)
        {
            angles.x = 40;
        }


        lookAt.transform.localEulerAngles = angles;

        if (moveInput.x != 0 || moveInput.y != 0 )//|| !_rotateOnMove)
        {
            if (lookAt.transform.rotation.y != 0)
            {
                //Set the player rotation based on the look transform
                transform.rotation = Quaternion.Euler(0, lookAt.transform.rotation.eulerAngles.y, 0);
                //reset the y rotation of the look transform
                lookAt.transform.localEulerAngles = new Vector3(anglesHolder.x, 0, 0);
              
            }
            // transform.rotation *= Quaternion.AngleAxis(lookInput.x * mouseSensityvityX, Vector3.up);
            // transform.rotation *= Quaternion.AngleAxis(lookInput.y * mouseSensityvityY, Vector3.left);
            //rotate only body
            //HandleBaseRotation();
        }
       float GetSmoothTheAngle()
        {
            float targetAngle = Mathf.Atan2(playerVelocity.x, playerVelocity.z) * Mathf.Rad2Deg + cam.transform.eulerAngles.y;
            float smotthedAngle = Mathf.SmoothDampAngle(rootOfBodyToRotate.transform.eulerAngles.y,
                targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            return smotthedAngle;
        }
        //--------Code 05/2023
        if (playerVelocity.magnitude >= 0.1f )
        {
           
           

            if (moveInput.x !=0 && moveInput.y > 0 && isAming )
            {
                rootOfBodyToRotate.transform.rotation = Quaternion.Euler(0f, GetSmoothTheAngle(), 0f);
               
            }
            if (moveInput.x != 0 && moveInput.y < 0 && isAming)
            {
                float goBackwardsTargetAngle = Mathf.Atan2(playerVelocity.x, playerVelocity.z) * Mathf.Rad2Deg + (cam.transform.eulerAngles.y+180f);
                float goBackwardsSmotthedAngle = Mathf.SmoothDampAngle(rootOfBodyToRotate.transform.eulerAngles.y,
                    goBackwardsTargetAngle, ref turnSmoothVelocity, turnSmoothTime);
                rootOfBodyToRotate.transform.rotation = Quaternion.Euler(0f, goBackwardsSmotthedAngle, 0f);
                //HandleBaseRotation();
            }
            if (!isAming)
            {
                rootOfBodyToRotate.transform.rotation = Quaternion.Euler(0f, GetSmoothTheAngle(), 0f);
            }
            
            
            //Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            
        }
        if(playerVelocity.magnitude<0.1f && isAming)
        {
            rootOfBodyToRotate.transform.rotation = Quaternion.Euler(0f, lookAt.transform.rotation.eulerAngles.y, 0f);
        }
        #endregion
    }

    public void ViewFinder()
    {
        // Create a vector at the center of our camera's viewport
        Vector3 rayOrigin, gOrgin;
        //  Vector3 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        // Ray ray = cam.ScreenPointToRay(screenCenterPoint);
        rayOrigin = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));
        gOrgin = cam.transform.forward;
        if (Physics.Raycast(rayOrigin, gOrgin, out RaycastHit raycastHit, 999f, aimColliderLayerMask))//(ray,out RaycastHit raycastHit , 999f, aimColliderLayerMask))
        {
            debugAimTransform.position = raycastHit.point;
        }

        aimRig.weight = Mathf.Lerp(aimRig.weight, aimRigWeight, Time.deltaTime * 20f); //To Smooth animation transition Rig

    }
    

    public void SetRotateOnMove(bool newRotateOnMove)
    {
        _rotateOnMove = newRotateOnMove;
    }
    #endregion

    #endregion

    #region Camera Slide From Left To Writes Methods And Corutines

    public void ResetCamerasSide()
    {
        if (fallowCamera.active == true)
        {
            //SmoothlyMoveCameraSide(fallowCamera, 1f, 5f);
            //  StartCoroutine(moveCamToLeft());
            //  fallowCamera.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<Cinemachine3rdPersonFollow>().CameraSide = 1f;
            // StartCoroutine(MoveGivenCamToRight(fallowCamera));
            StartCoroutine(SmoothlyMoveCameraSideCoroutine(fallowCamera, 1f, 5f));
            fallowCamMoved = true;
        }
        if (aimCamera.active == true)
        {
            //  StartCoroutine(moveCamToLeft());
            //  SmoothlyMoveCameraSide(aimCamera, 1f, 5f);
            // aimCamera.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<Cinemachine3rdPersonFollow>().CameraSide = 1f;
            StartCoroutine(SmoothlyMoveCameraSideCoroutine(aimCamera, 1f, 5f));
            aimCamMoved = true;
        }
        if (aimCamMoved && fallowCamMoved)
        {
            aimCamMoved = false;
            fallowCamMoved = false;
            needToMoveCameras = false;
        }
    }

    #region Camera Slide From Left To right corutines menthods
    public IEnumerator moveCamToLeft()
    {
        

        float camSide = aimCamera.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<Cinemachine3rdPersonFollow>().CameraSide;
        for (float setToBe = 1f; setToBe >= 0; setToBe -= 0.1f)
        {
            camSide = setToBe;
            aimCamera.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<Cinemachine3rdPersonFollow>().CameraSide = camSide;
            fallowCamera.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<Cinemachine3rdPersonFollow>().CameraSide = camSide;

            yield return new WaitForSeconds(0.1f);
        }
        //   yield return new WaitForEndOfFrame();
        // aimCamera.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<Cinemachine3rdPersonFollow>().CameraSide = Mathf.Lerp(camSide, 0f, Time.deltaTime * 5f);
        if (camSide == 0)
        {
            camMoveSideFinisht = true;
        }
    }
    public IEnumerator moveCamToRight()
    {

        float camSide = aimCamera.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<Cinemachine3rdPersonFollow>().CameraSide;
        //  aimCamera.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<Cinemachine3rdPersonFollow>().CameraSide = Mathf.Lerp(camSide, 1f, Time.deltaTime * 5f);
        for (float setToBe = 0f; setToBe <= 1; setToBe += 0.1f)
        {
            camSide = setToBe;
            aimCamera.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<Cinemachine3rdPersonFollow>().CameraSide = camSide;
            fallowCamera.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<Cinemachine3rdPersonFollow>().CameraSide = camSide;
            yield return new WaitForSeconds(0.1f);
        }
        //yield return new WaitForEndOfFrame();
        if (camSide == 1)
        {
            camMoveSideFinisht = true;
        }
    }
        public IEnumerator MoveGivenCamToRight(GameObject cam)
        {

            float camSide = cam.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<Cinemachine3rdPersonFollow>().CameraSide;
            //  aimCamera.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<Cinemachine3rdPersonFollow>().CameraSide = Mathf.Lerp(camSide, 1f, Time.deltaTime * 5f);
            for (float setToBe = 0f; setToBe <= 1; setToBe += 0.1f)
            {
                camSide = setToBe;
                cam.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<Cinemachine3rdPersonFollow>().CameraSide = camSide;
                
                yield return new WaitForSeconds(0.1f);
            }
            //yield return new WaitForEndOfFrame();
            if (camSide == 1)
            {
                camMoveSideFinisht = true;
            }
            
        }

    private IEnumerator SmoothlyMoveCameraSideCoroutine(GameObject cam, float targetCameraSide, float smoothSpeed)//<-- this method works the best
    {
        Cinemachine3rdPersonFollow thirdPersonFollow;
        thirdPersonFollow = cam.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<Cinemachine3rdPersonFollow>();

        float elapsedTime = 0f;
        float startCameraSide = thirdPersonFollow.CameraSide;

        while (elapsedTime < 1f)
        {
            // Calculate the new CameraSide value using Lerp.
            float smoothedCameraSide = Mathf.Lerp(startCameraSide, targetCameraSide, elapsedTime);

            // Set the CameraSide value to the smoothed value.
            thirdPersonFollow.CameraSide = smoothedCameraSide;

            // Increment the elapsed time based on smoothSpeed.
            elapsedTime += Time.deltaTime * smoothSpeed;

            yield return null;
        }

        // Ensure the final value is exactly the target value.
        thirdPersonFollow.CameraSide = targetCameraSide;
    }
    

    public void SmoothlyMoveCameraSide(GameObject cam, float targetCameraSide, float smoothSpeed)
    {
        Cinemachine3rdPersonFollow thirdPersonFollow;
        thirdPersonFollow = cam.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<Cinemachine3rdPersonFollow>();
        // Calculate the new CameraSide value using Lerp.
        float smoothedCameraSide = Mathf.Lerp(thirdPersonFollow.CameraSide, targetCameraSide, smoothSpeed * Time.deltaTime);

        // Clamp the value to ensure it stays within the valid range [0, 1].
        smoothedCameraSide = Mathf.Clamp01(smoothedCameraSide);

        // Set the CameraSide value to the smoothed value.
        thirdPersonFollow.CameraSide = smoothedCameraSide;
    }
    #endregion
    public void SlideCamera(InputAction.CallbackContext context)
    {
        if (runPressed && !isCamOnLeft && !camMoveSideFinisht)
        {
            StartCoroutine(moveCamToLeft());
            isCamOnLeft = true;
        }
        else if (runPressed && isCamOnLeft && !camMoveSideFinisht)
        {
            StartCoroutine(moveCamToRight());
            isCamOnLeft = false;
        }
        if (!runPressed)
        {

            camMoveSideFinisht = false;
            isCamOnLeft = aimCamera.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<Cinemachine3rdPersonFollow>().CameraSide <= 0.51f;
        }

    }
    #endregion

    #region Smooth Transition Layers Methods
    private void SmoothlySetLayerWeightAnimator(int layerIndex, float targetWeight)
    {

        float currentWeight = animator.GetLayerWeight(layerIndex);
        float newWeight = Mathf.Lerp(currentWeight, targetWeight, layerTransitionSpeed * Time.deltaTime);

        animator.SetLayerWeight(layerIndex, newWeight);

        // Check if the layer weight is close to the target and set it to the target to ensure it doesn't linger around very small values.
        if (Mathf.Approximately(newWeight, targetWeight))
        {
            animator.SetLayerWeight(layerIndex, targetWeight);
        }

    }

    private void SmoothlyTransitionRigLayerWeight(Rig rig, float targetWeight)
    {
        float currentWeight = rig.weight;
        float newWeight = Mathf.Lerp(currentWeight, targetWeight, layerTransitionSpeed * Time.deltaTime);

        rig.weight = newWeight;

        // Check if the weight is close to the target and set it to the target to ensure it doesn't linger around very small values.
        if (Mathf.Approximately(newWeight, targetWeight))
        {
            rig.weight = targetWeight;
        }
    }
    #endregion








    #region Fire && Aim
    void Fire(InputAction.CallbackContext ctx)
    {
       // shootHasBeenPressed = true;
        Debug.Log(ctx.action.name+ "Fire");

        attack.Shoot();
        impulseSource = GetComponent<Cinemachine.CinemachineImpulseSource>();
        impulseSource.GenerateImpulse(cam.transform.forward);

    }

    void Aim(InputAction.CallbackContext ctx)
    {
        Debug.Log("Aim");
        if (!ctx.canceled &&fallowCamera.activeInHierarchy)
        {
            
            isAming = true;
            fallowCamera.SetActive(false);
            aimCamera.SetActive(true);
            _rotateOnMove = false; 
            crosshair.SetActive(true);

           
            if (!inCover)
            {
               
                // Set Animations
                aimRigWeight = 1f;
                animator.SetLayerWeight(1, 1f);// Mathf.Lerp(animator.GetLayerWeight(1), 1f, Time.deltaTime * 10f)); //Matf to make it smoother transition
                animator.SetLayerWeight(0, 0f);

                // If aimCamera is still on left side after hide reset it to main position when not in cover this is just incase 
                if(aimCamera.GetComponent<CinemachineVirtualCamera>().GetCinemachineComponent<Cinemachine3rdPersonFollow>().CameraSide < 1f)
                {       
                    needToMoveCameras = true;
                   
                }

            }


        }
        if (ctx.canceled && aimCamera.activeInHierarchy)
        {
            isAming = false;
            aimCamera.SetActive(false);
            fallowCamera.SetActive(true);
            _rotateOnMove = true;
            crosshair.SetActive(false);

            //Set Animations
            aimRigWeight = 0f;
            animator.SetLayerWeight(0, 1f);
            animator.SetLayerWeight(1, Mathf.Lerp(animator.GetLayerWeight(1), 0f, Time.deltaTime * 10f)); //Matf to make it smoother transition
            
        }

    }
    void AimCanceled (InputAction.CallbackContext ctx)
    {
        Debug.Log("AimCanceled");
        
        if ( aimCamera.activeInHierarchy)
        {
            isAming = false;
            aimCamera.SetActive(false);
            fallowCamera.SetActive(true);
            _rotateOnMove = true;
            crosshair.SetActive(false);

            //Set Animations
            aimRigWeight = 0f;
            animator.SetLayerWeight(0, 1f);
            animator.SetLayerWeight(1, 0f);//Mathf.Lerp(animator.GetLayerWeight(1), 0f, Time.deltaTime * 10f)); //Matf to make it smoother transition
           
        }

    }

    #endregion

    #region StealthKill
    void StealthKill(InputAction.CallbackContext ctx)
    {

        if (isAbleToPerformStealhKill)
        {
            Debug.LogWarning("StealthKillPerformed");
            movePlayerToKillPosition = true;

           
             StartCoroutine(afterStealhKillPerfromed());
         
        }
        
        
    }
   

    IEnumerator afterStealhKillPerfromed()
    {
          takeDownInProgress = true;
        movePlayerToKillPosition = true;
       
    
        
        this.gameObject.GetComponent<RigBuilder>().enabled = false;
            animator.SetBool("StealthKill", true);
        enemysAnimator.SetBool("gotStealthKilled", true);


        enemysAnimator.SetLayerWeight(enemysAnimator.GetLayerIndex("StealthKill"), 1);
       
        yield return new WaitForSeconds(4.39f);
            
            this.gameObject.GetComponent<RigBuilder>().enabled = true;
            if (enemysAnimator != null)
            {
                enemysAnimator.SetBool("gotStealthKilled", false);
            }

            animator.SetBool("StealthKill", false);
        //__Kill Enemy___SetEnemysHealth___
        EnemyHealth enHealth = enemyTarget.transform.root.gameObject.GetComponent<EnemyHealth>();
        if(enHealth != null)
        {
            enHealth.health -= 1000;
            enHealth.Kill();
        }

        movePlayerToKillPosition = false;
        isAbleToPerformStealhKill = false;
        takeDownInProgress = false;



    }
    #endregion

    #region Pause The Game && Calculate Distance Methods
    void PauseTheGame(InputAction.CallbackContext ctx)
    {
        Debug.Log("1GameIsPaused: " + PauseMenu.GameIsPaused + " pauseTheGamePressed: " + PauseMenu.pauseTheGamePressed);

        // Debug.Log(ctx.ReadValueAsButton()+ "Duration: "+ ctx.duration);
        if (PauseMenu.GameIsPaused && !ctx.ReadValueAsButton()) //&& !PauseMenu.pauseTheGamePressed
        {
            // StartCoroutine( SendMessageToUnPause());
            PauseMenu.pauseTheGamePressed = true ;
            PauseMenu.GameIsPaused = false;
            Debug.Log("check false");
        }

        else if (!PauseMenu.GameIsPaused  && !ctx.ReadValueAsButton())//&& !ctx.ReadValueAsButton())//if(ctx.phase == InputActionPhase.Performed && !PauseMenu.pauseTheGamePressed)/
        {
            // SendMessageToPause();
            //  StartCoroutine(SendMessageToPause());
            PauseMenu.pauseTheGamePressed = true;
            PauseMenu.GameIsPaused = true;
            Debug.Log("check ture");
        }
      
            Debug.Log("2GameIsPaused: " + PauseMenu.GameIsPaused + " pauseTheGamePressed: " + PauseMenu.pauseTheGamePressed);
    }


    float CalculateDistance(GameObject obj1, GameObject obj2)
    {
        if (obj1 == null || obj2 == null)
        {
            Debug.LogWarning("One or both GameObjects are null.");
            return 0f;
        }

        return Vector3.Distance(obj1.transform.position, obj2.transform.position);
    }
    #endregion



    #region Timer and Score Methods
    public string updateTimer()
    {
        currentTime += 1;

        float minutes = Mathf.FloorToInt(currentTime / 60);
        float seconds = Mathf.FloorToInt(currentTime % 60);

       return string.Format("{0:00} : {1:00}", minutes, seconds);
    }

    public void SetScore(int scoreset)
    {
        score = scoreset;
    }

    public void AddScore(int sc)
    {
        score += sc;
    }

 
    public int GetScore()
    {
        return score;
    }

    public void SetTime(float timeset)
    {
        currentTime = timeset;
    }
    public float GetTime()
    {
        return currentTime;
    }
    #endregion
    //Added for Elevator____________________________________________
    #region OnTrigger Enter + Exit
    private void OnTriggerEnter(Collider other)
    {
       
        // Debug.Log("ElevatorTriggerWorks");
        if (other.transform.gameObject.CompareTag("Switch")) // should do them all by tag more efficient but no time now 
        {
            PauseMenu.elevatorButonNameToLocate = other.transform.name; Debug.Log("Switch player near Button: " + other.transform.name);
        }
       else if(other.transform.name == "P_Panel_Button_CallUp")
        {
           PauseMenu.elevatorButonNameToLocate = other.transform.name; Debug.Log("Eletor player near Button: " +other.transform.name);
        }
        else if (other.transform.name == "P_Panel_Button_CallDown")
            {
            PauseMenu.elevatorButonNameToLocate = other.transform.name; Debug.Log("Eletor player near Button: " + other.transform.name);
        }
        else if (other.transform.name == "P_Panel_Button_GoDown")
        {
            PauseMenu.elevatorButonNameToLocate = other.transform.name; Debug.Log("Eletor player near Button: " + other.transform.name);
        }
        else if (other.transform.name == "P_Panel_Button_GoUp")
        {
            PauseMenu.elevatorButonNameToLocate = other.transform.name; Debug.Log("Eletor player near Button: " + other.transform.name);
        }
        else if (other.transform.gameObject.CompareTag("POPupTEXT"))
        {
            readyToPress = true;

            objHolder = other.transform.GetChild(0).gameObject;

            if (Gamepad.current!=null)
            {
                popUpTextTMP.gameObject.active = true;
                popUpTextTMP.GetComponent<TextMeshPro>().text = "Press X to open the door";
                Debug.LogWarning("Gamepad is being used");
            }
            else
            {
                popUpTextTMP.gameObject.active = true;
                popUpTextTMP.GetComponent<TextMeshPro>().text = "Press Enter to open the door";
                Debug.LogWarning("Keyboard is being used");
            }
        } 
        //-------------------------SealthKill----------
        else if (other.CompareTag("StealthKill"))
        {
            Debug.LogWarning("Player In StealthKill Zone");
            // enemyTarget = other.transform.parent.gameObject;
            enemyTarget = other.transform.GetChild(0).gameObject;
            isAbleToPerformStealhKill = true;

            killPosition = other.transform.GetChild(0).position;
          //  Debug.LogError("CHild NAme Kill Zone: " + other.transform.GetChild(0).name);

            enemysAnimator = other.transform.parent.gameObject.GetComponent<Animator>();
            //other.transform.parent.gameObject.GetComponent<Animator>().SetBool("gotStealthKilled", true);
           
           

               
            
           

        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.gameObject.CompareTag("POPupTEXT"))
        {
            popUpTextTMP.gameObject.active = false;
            readyToPress = false;

        }
        else if (other.CompareTag("StealthKill"))
        {
            Debug.LogWarning("Player has EXIT StealthKill Zone");
            isAbleToPerformStealhKill = false;

        }
    }
    #endregion

    #region COVER 
    public void BeginMoveToCover(Vector3 targetPos)
    {
        //Disable player input
        DisableControls();
        animator.Play("Crouched Run",2, 0f);
        inCover = true;
        autoMoverActive = true;
        autoMoverTargerPos = targetPos;
    }

    public void BeginExitCover()
    {
        animator.SetBool("ExitCover", true);
        inCover = false;
        EnableControls();
        coverCS.hasExitCover = true;
        hasExitCover = true;
        moveToCoverAnimationFinisht = false;

        needToMoveCameras = true;
        // StartCoroutine(moveCamToRight());
        //  SmoothlyMoveCameraSide(fallowCamera, 1f, 2f);
        //  SmoothlyMoveCameraSide(aimCamera, 1f, 2f);
        //if (animator.GetCurrentAnimatorStateInfo(3).normalizedTime >= 1.0f)
        //{
        //    inCover = false;
        //    animator.SetLayerWeight(3, 0);
        //    animator.SetLayerWeight(0, 1);
        //    EnableControls();
        //    animator.SetBool("ExitCover", false);
        //    Debug.LogWarning("ExitedCover in Begin State");
        //}

    }

    //THIS METHOD IS USING ANIMATION EVENT!
    public void AnimationFinished()
    {

        //  inCover = false;
        SetAllAnimatorLayersWeightTo(0);
        animator.SetBool("inCoverLow", false);
        animator.SetBool("inCoverIdle", false);
        animator.SetBool("TakeCoverLow", false);
        animator.SetBool("TakeCoverHigh", false);
        //  SetAllAnimatorParametersBoolTo(false);
        animator.SetLayerWeight(3, 0);
        animator.SetLayerWeight(0, 1);
        //EnableControls();
       // SetAllAnimatorParametersBoolTo(false);
        animator.SetBool("ExitCover", false);
        Debug.LogWarning("ExitedCover");

        
    }

    //USING ANIMATOR EVENT
    public void MoveToCoverAnimationFinishtToTrue()
    {
         moveToCoverAnimationFinisht = true;
    }
   
    private void MoveToCover()
    {
        #region MoveToCover


        if (Vector3.Distance(transform.position, autoMoverTargerPos) > autoMoverStoppingDistance)
        {
           // nav.enabled = false;
           if(animator.GetBool("RuningToCover") != true)
            {
                animator.SetBool("RuningToCover", true);//Animation
            }
            //Reset animation layers
            SetAllAnimatorLayersWeightTo(0);
            SetAllAnimatorParametersBoolTo(false);
            animator.SetBool("RuningToCover", true);
            //Set the appropiate Animation Layer
            animator.SetLayerWeight(2, 1f);

            //animator.SetLayerWeight(0, 0f);
            //animator.SetLayerWeight(1, 0f);
            //animator.SetLayerWeight(2, 1f);
            //animator.SetLayerWeight(3, 0f);
            //animator.SetLayerWeight(4, 0f);

            Vector3 moveDirection = (autoMoverTargerPos - transform.position).normalized;
            //  Vector3 newPosition = transform.position + moveDirection * runSpeed * Time.deltaTime;
            // Calculate the new position with interpolation for smoother movement.
            Vector3 newPosition = Vector3.Lerp(transform.position, autoMoverTargerPos, 3f * Time.deltaTime);
            transform.position = newPosition;
        }
        else 
        {

            Cover coverComp = GetComponent<Cover>();
            animator.SetBool("RuningToCover", false);
            animator.SetBool("inCoverIdle", true);
            if (!coverComp.inHighCover && moveToCoverAnimationFinisht == true)
            {

               // SetAllAnimatorLayersWeightTo(0);
               
                animator.SetLayerWeight(3, 1f);
                
                animator.SetLayerWeight(0, 0f);
                animator.SetLayerWeight(1, 0f);
                animator.SetLayerWeight(2, 0f);
                animator.SetLayerWeight(4, 0f);

                this.gameObject.GetComponent<RigBuilder>().layers[0].active=false;
                animator.SetBool("TakeCoverLow", true);
                animator.SetBool("TakeCoverHigh", false);
            }
            else if (coverComp.inHighCover && moveToCoverAnimationFinisht == true)
            {
                // rigRun.gameObject.SetActive(true);

                animator.SetLayerWeight(0, 0f);
                animator.SetLayerWeight(1, 0f);
                animator.SetLayerWeight(2, 0f);
                animator.SetLayerWeight(3, 0f);
                // SetAllAnimatorLayersWeightTo(0);
                animator.SetLayerWeight(4, 1f);

                this.gameObject.GetComponent<RigBuilder>().layers[0].active = true;
                animator.SetBool("TakeCoverLow", false);
                animator.SetBool("TakeCoverHigh", true);

            }
            
            autoMoverActive = false;
            autoMoverTargerPos = Vector3.zero;
     
            //Re-enable Player Input
            EnableControls();

        }
        //UpdateAnimator();
        #endregion
    }



    #region In Cover Animations Handle
    public void CoverAnimationHandle()
    {
       

        if (!coverCS.inHighCover )
        {
            
            animator.SetBool("TakeCoverLow", true);
            animator.SetBool("TakeCoverHigh", false);
            animator.SetBool("inCoverHigh", false);
            // animator.SetBool("inCoverLow", true);

            //if prefiuslu in high cover smoothout the transition
            if (animator.GetLayerWeight(4) > 0)
            {
                SmoothlySetLayerWeightAnimator(4, 0);
                SmoothlySetLayerWeightAnimator(3, 1);
            }
            else
            {
                animator.SetLayerWeight(3, 1f);
                animator.SetLayerWeight(4, 0f);
            }
            animator.SetLayerWeight(0, 0f);

            
            if (!isAming)
            {
                this.gameObject.GetComponent<RigBuilder>().layers[0].active = false;
                animator.SetLayerWeight(5, 0f);
                animator.SetLayerWeight(3, 1f);
                animator.SetBool("isAming", false);
            }
             else if (isAming)
            {
               
                animator.SetBool("isAming", true);
                animator.SetLayerWeight(3, 0f);
                animator.SetLayerWeight(5, 1f);

                //Smooth out rig Transition
                
               // this.gameObject.GetComponent<RigBuilder>().layers[0].active = true;
             //   this.gameObject.GetComponent<RigBuilder>().layers[0].rig.weight = 1;

                // Set Animations
                aimRigWeight = 1f;
              //  animator.SetLayerWeight(1, 1f);// Mathf.Lerp(animator.GetLayerWeight(1), 1f, Time.deltaTime * 10f)); //Matf to make it smoother transition
              //  animator.SetLayerWeight(0, 0f);
            }
            

            if(move.ReadValue<Vector2>() == Vector2.zero)
            {
                animator.SetBool("inCoverIdle", true);
                animator.SetInteger("moveInCover", 0);
                if (isAming)
                {
                    animator.SetBool("isAimingIdle", true);
                }

            }
             else if(move.ReadValue<Vector2>().x > 0 || move.ReadValue<Vector2>().y > 0)
            {
                animator.SetInteger("moveInCover", 1);
                animator.SetBool("inCoverIdle", false);
            }
            else if (move.ReadValue<Vector2>().x < 0 || move.ReadValue<Vector2>().y < 0)
            {
                animator.SetInteger("moveInCover", -1);
                animator.SetBool("inCoverIdle", false);
            }

        }
        else if (coverCS.inHighCover && !isAming)
        {
            
            // rigRun.gameObject.SetActive(true);


            animator.SetBool("TakeCoverLow", false);
            animator.SetBool("TakeCoverHigh", false);
            animator.SetBool("inCoverLow", false);
            animator.SetBool("inCoverHigh",true);

            //check if any height cover side is free to aim from and upadate animator
            animator.SetBool("inLeftSide", coverCS.inLeftHightSightCover);
            animator.SetBool("inRightSide", coverCS.inRightHightSightCover);

          
            //Smooth out transition
            if (animator.GetLayerWeight(3) > 0)
            {
               // animator.SetLayerWeight(6, 0f);
                SmoothlySetLayerWeightAnimator(3, 0);
                SmoothlySetLayerWeightAnimator(4, 1);
                //Smooth out rig Transition
                rigRun = this.gameObject.GetComponent<RigBuilder>().layers[0].rig;
                if (this.gameObject.GetComponent<RigBuilder>().layers[0].active == false)
                {
                    rigRun.weight = 0;
                    this.gameObject.GetComponent<RigBuilder>().layers[0].active = true;

                }
                else { SmoothlyTransitionRigLayerWeight(rigRun, 1); }
            }
            else { animator.SetLayerWeight(4, 1f); animator.SetLayerWeight(6, 0f); SmoothlySetLayerWeightAnimator(3, 0); }

          

            if (move.ReadValue<Vector2>() == Vector2.zero)
            {
                animator.SetBool("inCoverIdle", true);
                animator.SetInteger("moveInCover", 0);

            }
            else if (move.ReadValue<Vector2>().x > 0 || move.ReadValue<Vector2>().y > 0)
            {
                animator.SetInteger("moveInCover", 1);
                animator.SetBool("inCoverIdle", false);
            }
            else if (move.ReadValue<Vector2>().x < 0 || move.ReadValue<Vector2>().y < 0)
            {
                animator.SetInteger("moveInCover", -1);
                animator.SetBool("inCoverIdle", false);
            }

        }

       

        if(coverCS.inRightHightSightCover || move.ReadValue<Vector2>().x < 0 || move.ReadValue<Vector2>().y <0)
        {
            SmoothlyMoveCameraSide(fallowCamera, 0, 5f);
            if (isAming)
            {
                SmoothlyMoveCameraSide(aimCamera, 0, 5f);
            }
            
        }
         else if (coverCS.inLeftHightSightCover || move.ReadValue<Vector2>().x > 0 || move.ReadValue<Vector2>().y > 0)
        {
             SmoothlyMoveCameraSide(fallowCamera, 1, 5f);
            if (isAming)
            {
                SmoothlyMoveCameraSide(aimCamera, 1, 5f);
            }
           
        }
        //else
        //{
        //    SmoothlyMoveCameraSide(fallowCamera, 0.5f, 1f);
        //    SmoothlyMoveCameraSide(aimCamera, 0.5f, 1f);
        //}
        //if(coverCS.inHighCover && coverCS.inRightHightSightCover)//right is left I fucked up !
        //{

        //}
        //if(coverCS.inHighCover && coverCS.inLeftHightSightCover)
        //{
        //    wasInRightCorner = true;
        //}
        //if (!isAming&&coverCS.inRightHightSightCover)//right is left I fucked up !
        //{
        //   // wasInLeftCorner = true;
        //    Debug.LogWarning("inLeftCorner and canMove = " + canMove);
        //}
        //if (!isAming&&coverCS.inLeftHightSightCover)
        //{
        //    wasInRightCorner = true;
        //}
      
        if (isAming && coverCS.inHighCover )
        {
            canMove = false;
          
        }
        else
        {
            canMove = true;
        }

    }
    #endregion
   

    
    protected void InCoverMove()
    {
        #region InCover Move
        Vector2 moveInput = move.ReadValue<Vector2>();
       
            RaycastHit hitinfo;
            if (Physics.Raycast(transform.position, transform.forward, out hitinfo, 5f, coverCS.coverLayerMask) && hitinfo.collider.gameObject == coverCS.currentCoverObject)
            {
            coverCS.GetCoverSurfaceDirection(hitinfo.normal);
            }


            // Calculate the perpendicular direction to align the player's orientation in cover.
         //   Vector3 perpDirection = Vector3.Cross(inCoverMoveDirection, Vector3.up);//inCoverMoveDirection;//Vector3.Cross(inCoverMoveDirection, Vector3.up);
        Vector3 perpDirection = Vector3.Cross(hitinfo.normal , Vector3.up);
        // perpDirection.Normalize(); 
   
        
        // Calculate the movement direction based on input.
       //-- Vector3 desiredMoveDirection = perpDirection * moveInput.x + inCoverMoveDirection.normalized * moveInput.y;
        Vector3 desiredMoveDirection = perpDirection * moveInput.x + inCoverMoveDirection.normalized * moveInput.y;
 

        if (desiredMoveDirection.normalized != inCoverProhebitedDirection.normalized)
        {
            Vector3 velocity = new Vector3();
            
            if (canMove)
            {
               // ToFixBug = true;
                if (wasInRightCorner && moveBackIntoPossition && !isAming)
                {
                    desiredMoveDirection = new Vector3(desiredMoveDirection.x - 65f, desiredMoveDirection.y, desiredMoveDirection.z);
                    
                    velocity = transform.TransformDirection(desiredMoveDirection);
                    Debug.LogWarning("MoveBackIntoPosition RightCorner");
                    moveBackIntoPossition = false;
                    wasInRightCorner = false;
                }
                else if (moveBackIntoPossition && !isAming)
                {
                    desiredMoveDirection = new Vector3(desiredMoveDirection.x +65f, desiredMoveDirection.y, desiredMoveDirection.z);
                    velocity = transform.TransformDirection(desiredMoveDirection);
                    Debug.LogWarning("MoveBackIntoPosition");
                    moveBackIntoPossition = false;
                    wasInLeftCorner = false;
                }
              
                else 
                {
                    velocity = desiredMoveDirection.normalized * inCoverMoveSpeed * Time.deltaTime; 
                  //  transform.rotation = Quaternion.LookRotation(-hitinfo.normal); 
                }

            }
            else if (!canMove)
            {
 
                if (coverCS.inLeftHightSightCover)//right is left I fucked up 
                {
                  
                    desiredMoveDirection = new Vector3(desiredMoveDirection.x + 10f, desiredMoveDirection.y, desiredMoveDirection.z);
                    velocity = transform.TransformDirection(desiredMoveDirection);
                    moveBackIntoPossition = true;
                    wasInRightCorner = true;
                   // Debug.LogWarning("WasInRight = true");
                }
                else if (coverCS.inRightHightSightCover)
                {
                   
                        desiredMoveDirection = new Vector3(desiredMoveDirection.x -10f , desiredMoveDirection.y, desiredMoveDirection.z);
                    velocity = transform.TransformDirection(desiredMoveDirection); ;
                    moveBackIntoPossition = true;
                    wasInLeftCorner = true;
                }

            }
           



            // Apply the velocity to the Rigidbody.
           
            rb.velocity = velocity;
            
            // Optionally can add a vertical component to the velocity for jumping or crouching just use the something like the code underneath.
            // rb.velocity = new Vector3(velocity.x, velocity.y, velocity.z);

        }
            else
            {
                rb.velocity = Vector3.zero;
            

            // Handle any special logic for when movement in this direction is prohibited (e.g., stopping the player).
            }
        

        CoverAnimationHandle();
        HandleRotation();
        #endregion
    }

    #endregion
    //EndOfAddition code_______________________________________________

    #region Not Used Methods Expired As Project Evolved

    // | ABSTRACTION
    //  V
    private void attackAndCameraManagmentOnInput()
    {

        // if (Input.GetButtonDown("Fire1") && Time.time > nextFire)
        /*
         if(fire.action.OnStateEnter())
         {
             attack.Shoot();
             impulseSource = GetComponent<Cinemachine.CinemachineImpulseSource>();
             impulseSource.GenerateImpulse(cam.transform.forward);
         }

         if (Input.GetButtonDown("Fire2") && fallowCamera.activeInHierarchy)
         {
             fallowCamera.SetActive(false);
             aimCamera.SetActive(true);
         }
        
        if (Input.GetButtonUp("Fire2") && aimCamera.activeInHierarchy)
         {
             aimCamera.SetActive(false);
             fallowCamera.SetActive(true);

         }
       
        //shift to speed up(run)
        if (Input.GetButtonDown("SpeedUp"))
        {
            CurrentSpeed = runSpeed;
        }
        if (Input.GetButtonUp("SpeedUp"))
        {
            CurrentSpeed = _speed;
        }
         */
    }
    void HandleBaseRotation()//not in use
    {
        Vector3 moveInput = move.ReadValue<Vector2>();
        Vector3 curreentPosition = transform.position;
        // the change in position our character should point to 
        Vector3 newPositon = new Vector3(moveInput.x, 0, moveInput.y);
        //combaine the positon our character should point to
        Vector3 positonToLookAt = curreentPosition + newPositon;
        // rotate the character to face the positionToLookAt
        // transform.LookAt(positonToLookAt);
        //rotate only body
        rootOfBodyToRotate.transform.LookAt(positonToLookAt);
    }
    void HandleBaseRotationBackwards()//not in use
    {
        Vector3 moveInput = move.ReadValue<Vector2>();
        Vector3 curreentPosition = transform.position;
        // the change in position our character should point to 
        Vector3 newPositon = new Vector3(moveInput.x, 0, moveInput.y);
        //combaine the positon our character should point to
        Vector3 positonToLookAt = curreentPosition + newPositon;
        // rotate the character to face the positionToLookAt
        // transform.LookAt(positonToLookAt);
        //rotate only body
        rootOfBodyToRotate.transform.LookAt(positonToLookAt);
    }
    void RotateWhenAim()
    {
        Vector3 mouseWorldPosition = Vector3.zero;
        //to Rotate the player when aiming on y axis as not all body rotates becouse of animation rig
        mouseWorldPosition = debugAimTransform.position;
        Vector3 worldAimTarget = mouseWorldPosition;
        worldAimTarget.y = transform.position.y;
        Vector3 aimDirection = (worldAimTarget - transform.position).normalized;
        transform.forward = Vector3.Lerp(transform.forward, aimDirection, Time.deltaTime * 20f);
    }
    void PlayerLookAt(Transform target, float speed)
    {
        Vector3 direction = target.position - transform.position;
        Quaternion rotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, speed * Time.deltaTime);
    }
    void PlayerMoveTowards(GameObject target, float moveSpeed)
    {

        if (target != null)
        {
            Vector3 direction = (target.transform.position - transform.position).normalized;
            Vector3 newPosition = transform.position + direction * moveSpeed * Time.deltaTime;
            transform.position = newPosition;
        }

    }

    void PlayerMoveTowardsCover(Vector3 target, float moveSpeed)
    {

        if (target != null)
        {
            Vector3 direction = (target - transform.position).normalized;
            Vector3 newPosition = transform.position + direction * moveSpeed * Time.deltaTime;
            transform.position = newPosition;
        }

    }
    #endregion
}

#endregion


/*

 // void SendMessageToPause()
 IEnumerator SendMessageToPause()
  {


      yield return new WaitForSeconds(0.5f);
      PauseMenu.pauseTheGamePressed = true;
      Debug.Log("check ture");
  }

  IEnumerator SendMessageToUnPause()
  {

      yield return new WaitForSeconds(0.5f);
      PauseMenu.pauseTheGamePressed = false;
      Debug.Log("check false");
  }
  */



//------old rotation of player works good but not used anymore as character got changed to a humanoid----------
/*
        Vector3 lookInput = look.ReadValue<Vector2>();
        body.transform.Rotate(Vector3.up * lookInput.x  * mouseSensityvityX);

        head.transform.Rotate(Vector3.left * lookInput.y * mouseSensityvityY);
        transform.Rotate(Vector3.up * lookInput.x * mouseSensityvityX);

        gun.transform.Rotate(Vector3.left * lookInput.y * mouseSensityvityY);
*/
//--------------------------------------------------------------------------------------------------------
//Updating rotation to mach new character
/* #region Player Based Rotation

     Vector3 lookInput = look.ReadValue<Vector2>();
     lookAt.transform.rotation *= Quaternion.AngleAxis(lookInput.x * mouseSensityvityX, Vector3.up);
     lookAt.transform.rotation *= Quaternion.AngleAxis(lookInput.y * mouseSensityvityY, Vector3.left);

     var angles = lookAt.transform.localEulerAngles;
     angles.z = 0;
     anglesHolder = angles;
     var angle = lookAt.transform.localEulerAngles.x;

     //Clamp the Up/Down rotation
     if (angle > 180 && angle < 340)
     {
         angles.x = 340;
     }
     else if (angle < 180 && angle > 40)
     {
         angles.x = 40;
     }


     lookAt.transform.localEulerAngles = angles;

     if (moveInput.x != 0 || moveInput.y != 0 || !_rotateOnMove)
     {
         if (lookAt.transform.rotation.y != 0)
         {
             //Set the player rotation based on the look transform
             transform.rotation = Quaternion.Euler(0, lookAt.transform.rotation.eulerAngles.y, 0);
             //reset the y rotation of the look transform
             lookAt.transform.localEulerAngles = new Vector3(anglesHolder.x, 0, 0);
         }
         //  transform.rotation *= Quaternion.AngleAxis(lookInput.x * mouseSensityvityX, Vector3.up);
         // transform.rotation *= Quaternion.AngleAxis(lookInput.y * mouseSensityvityY, Vector3.left);
         //rotate only body
         //HandleRotation();
     }

     //--------Code 05/2023
     if (playerVelocity.magnitude >= 0.1f )
     {
         float targetAngle = Mathf.Atan2(playerVelocity.x, playerVelocity.z) * Mathf.Rad2Deg + cam.transform.eulerAngles.y;
         float smotthedAngle = Mathf.SmoothDampAngle(rootOfBodyToRotate.transform.eulerAngles.y,
             targetAngle, ref turnSmoothVelocity, turnSmoothTime);
     if (!isAming)
     {
         rootOfBodyToRotate.transform.rotation = Quaternion.Euler(0f, smotthedAngle, 0f);
     }


         Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

     // Vector3 moveVector = transform.TransformDirection(moveInput);
     //  moveDir = moveDir * CurrentSpeed;// * Time.deltaTime;
     //  rb.velocity = transform.TransformDirection(moveDir);  

     //  moveVector= moveVector* CurrentSpeed *Time.deltaTime;                                                                //characterController.Move(moveVector * CurrentSpeed * Time.deltaTime);
     //   velocity = velocity * Time.deltaTime;

 }





 base.Move(head, gun, body);
 #endregion

*/









// else if (coverCS.inHighCover && isAming)
//{

//    animator.SetBool("inCoverHigh", true);

//    //check if any height cover side is free to aim from and upadate animator
//    animator.SetBool("inLeftSide", coverCS.inLeftHightSightCover);
//    animator.SetBool("inRightSide", coverCS.inRightHightSightCover);




//    animator.SetLayerWeight(6, 0f);
//    animator.SetLayerWeight(3, 0f);
//    animator.SetLayerWeight(4, 1f);
//    this.gameObject.GetComponent<RigBuilder>().layers[0].active = true;
//    aimRigWeight = 0f;





//    if (coverCS.inLeftHightSightCover || coverCS.inRightHightSightCover)
//    {


//        animator.SetLayerWeight(3, 0f);
//        animator.SetLayerWeight(4, 0f);
//        animator.SetLayerWeight(6, 1f);
//        animator.SetBool("isAming", true);
//        animator.SetBool("inCoverHight", coverCS.inHighCover);
//        animator.SetBool("inLeftSide", coverCS.inLeftHightSightCover);
//        animator.SetBool("inRightSide", coverCS.inRightHightSightCover);

//        aimRigWeight = 1f;

//    }


//    if (move.ReadValue<Vector2>() == Vector2.zero)
//    {
//        animator.SetBool("inCoverIdle", true);
//        animator.SetInteger("moveInCover", 0);

//    }
//    else if (move.ReadValue<Vector2>().x > 0 || move.ReadValue<Vector2>().y > 0)
//    {
//        animator.SetInteger("moveInCover", 1);
//        animator.SetBool("inCoverIdle", false);
//    }
//    else if (move.ReadValue<Vector2>().x < 0 || move.ReadValue<Vector2>().y < 0)
//    {
//        animator.SetInteger("moveInCover", -1);
//        animator.SetBool("inCoverIdle", false);
//    }

//}










// else if (coverCS.inHighCover && !isAming)
//{
//    // rigRun.gameObject.SetActive(true);


//    animator.SetBool("TakeCoverLow", false);
//    animator.SetBool("TakeCoverHigh", false);
//    animator.SetBool("inCoverLow", false);
//    animator.SetBool("inCoverHigh", true);

//    //check if any height cover side is free to aim from and upadate animator
//    animator.SetBool("inLeftSide", coverCS.inLeftHightSightCover);
//    animator.SetBool("inRightSide", coverCS.inRightHightSightCover);


//    //Smooth out transition
//    if (animator.GetLayerWeight(3) > 0)
//    {
//        // animator.SetLayerWeight(6, 0f);
//        SmoothlySetLayerWeightAnimator(3, 0);
//        SmoothlySetLayerWeightAnimator(4, 1);
//        //Smooth out rig Transition
//        rigRun = this.gameObject.GetComponent<RigBuilder>().layers[0].rig;
//        if (this.gameObject.GetComponent<RigBuilder>().layers[0].active == false)
//        {
//            rigRun.weight = 0;
//            this.gameObject.GetComponent<RigBuilder>().layers[0].active = true;

//        }
//        else { SmoothlyTransitionRigLayerWeight(rigRun, 1); }
//    }
//    else { animator.SetLayerWeight(4, 1f); animator.SetLayerWeight(6, 0f); SmoothlySetLayerWeightAnimator(3, 0); }



//    if (move.ReadValue<Vector2>() == Vector2.zero)
//    {
//        animator.SetBool("inCoverIdle", true);
//        animator.SetInteger("moveInCover", 0);

//    }
//    else if (move.ReadValue<Vector2>().x > 0 || move.ReadValue<Vector2>().y > 0)
//    {
//        animator.SetInteger("moveInCover", 1);
//        animator.SetBool("inCoverIdle", false);
//    }
//    else if (move.ReadValue<Vector2>().x < 0 || move.ReadValue<Vector2>().y < 0)
//    {
//        animator.SetInteger("moveInCover", -1);
//        animator.SetBool("inCoverIdle", false);
//    }

//}
#endregion
