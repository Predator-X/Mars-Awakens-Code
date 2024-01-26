using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

public class CutScene_Input : MonoBehaviour
{
    [SerializeField] GameObject timeLineEnable;
    [SerializeField] GameObject timeLineDisable;

    private InputAction playerInput;
    public InputActionAsset inputActionAsset;
    private InputActionMap playerActionMap;

    private void OnEnable()
    {
        playerInput = playerActionMap.FindAction("StealthKill");
        playerActionMap.Enable();
        playerActionMap.FindAction("StealthKill").started += StealthKill;
     
    }

    private void OnDisable()
    {
        playerInput.Disable();
       
        playerActionMap.FindAction("StealthKill").started -= StealthKill;
    }

    private void StealthKill(InputAction.CallbackContext ctx)
    {
        // Action to perform when StealthKill is pressed
        Debug.LogWarning("Sthealth Kill Pressed");
        EnableTimeline();
    }

    private void Awake()
    {
        inputActionAsset = this.GetComponent<PlayerInput>().actions;
        playerActionMap = inputActionAsset.FindActionMap("Player");
        
    }

    private void EnableTimeline()
    {
        Debug.LogWarning("Sthealth Kill Pressed");
        timeLineEnable.SetActive(true);
        timeLineDisable.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown("t"))
        {
           // Debug.LogWarning("Sthealth Kill Pressed");
            EnableTimeline();
        }
    }
}