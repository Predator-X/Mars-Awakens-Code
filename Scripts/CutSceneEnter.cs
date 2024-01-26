using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class CutSceneEnter : MonoBehaviour
{
    public int trackIndex; // The index of the track you want to reference
    public GameObject cutScene;
    public bool loadNextSceneAfterCutScene = false, usePlayersHumanoid = false
        , enableOnTriggerGameObj = false, disableOnTriggerGameObj = false, destroyThisCutSceneWhenEnd=false
        ,destroyCutSceneAfterFinish;
    [SerializeField] GameObject objectToDestroy;
    public float endTimeOffset = 0f;

    public GameObject enableOnStartGameObj, disableOnStartGameObj;
    public GameObject[] enableAfterEndGameObjects;
    public GameObject[] disableAfterEndGameObjects;
    [SerializeField] bool searchForPlayer = false;
    public GameObject player,playerHolder;//player is prefab for timeline and playerHolder is the player 
    public float waitForSecondsToFinishCut = 10f;
    PlayerController playerController;
    PlayableDirector playableDirector;
    RigBuilder playersRigBuilder;

    public bool transformPlayerWhereHumanoidFinishtOnScene = false;
    public GameObject cutScenesHumanoidHolder;

    //For Enabling and geting players virtual camera, also there is a function for enabling player before scene ends 
    public GameObject playersVcam;

    private void Start()
    {     
        playableDirector = cutScene.GetComponent<PlayableDirector>();
    }

    private void SearchForPlayer1()
    {
        playerHolder = GameObject.FindGameObjectWithTag("Player").gameObject;
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playersVcam = other.GetComponent<PlayerController>().fallowCamera;//aimCamera.GetComponent<CinemachineVirtualCamera>()
            if (enableOnTriggerGameObj)
            {
                enableOnStartGameObj.SetActive(true);
            }
            if (disableOnTriggerGameObj)
            {
                disableOnStartGameObj.SetActive(false);
            }

            if (usePlayersHumanoid)
            {
                WhenUsingPlayersHumanoid(other);
            }
            else if (!usePlayersHumanoid)
            {
                    PauseMenu.GameIsPaused = true;

                playerHolder = other.gameObject;

                playersRigBuilder = other.GetComponent<RigBuilder>();
                playersRigBuilder.enabled = false;

                cutScene.SetActive(true);
                playerHolder.SetActive(false);
                playableDirector.Stop();

                waitForSecondsToFinishCut = ((float)playableDirector.duration);

               if(this.gameObject.GetComponent<BoxCollider>() == null)
                {
                    this.gameObject.GetComponent<SphereCollider>().enabled = false;
                }
                else { this.gameObject.GetComponent<BoxCollider>().enabled = false; }

                playerController = other.gameObject.GetComponent<PlayerController>();
                playerController.gameObject.SetActive(false);
                other.gameObject.GetComponent<RayCastViewer>().enabled = false;




                FinishCutOption(loadNextSceneAfterCutScene);
                playableDirector.Play();
                playerController.enabled = false;
            }

        }
    }
   
    public void FinishCutOption(bool decision)
    {
        if (decision)
        {

           
            StartCoroutine(FinishCutAndLoadNextScene());
        }
        else
        {

           
            StartCoroutine(FinishCut());
        }
    }

    public void PlayerTransformEqualsCutSceneHumanoid(GameObject playerT, GameObject Humanoit)
    {
        if(player != null && Humanoit != null)
        {
            playerT.transform.position = Humanoit.transform.position;
            playerT.transform.Find("Main Camera (1)").transform.position = Humanoit.transform.Find("Main Camera (1)").transform.position;
            playerT.transform.Find("Main Camera (1)").transform.rotation = Humanoit.transform.Find("Main Camera (1)").transform.rotation;
            playerT.transform.rotation = Humanoit.transform.rotation;
        }
       
    }
    public void WhenUsingPlayersHumanoid(Collider other)
    {
        Animator playersAnimator = other.GetComponent<Animator>();
        AudioSource playersAudioSource = other.GetComponent<AudioSource>();
        PauseMenu.GameIsPaused = true;
        SetAnimatorReference(playableDirector, 0, playersAnimator);
        //  SetAnimatorReference(playableDirector, 1, playersAudioSource);
        FreezeConstraints(other.GetComponent<Rigidbody>());
        playersRigBuilder = other.GetComponent<RigBuilder>();
        playersRigBuilder.enabled = false;
        cutScene.SetActive(true);
        playableDirector.Stop();

        waitForSecondsToFinishCut = ((float)playableDirector.duration);

        this.gameObject.GetComponent<BoxCollider>().enabled = false;

        playerController = other.gameObject.GetComponent<PlayerController>();

        //  player.transform.position = other.transform.position;
        playerHolder = other.gameObject;
        //  player.SetActive(true);
        //   playerHolder.SetActive(false);


        //cutScene.SetActive(true);
        FinishCutOption(loadNextSceneAfterCutScene);
        playableDirector.Play();
        playerController.enabled = false;
    }

    private void SetAnimatorReference(PlayableDirector director, int trackIndex, Animator animator)
    {
        int outputCount = director.playableAsset.outputs.ToList().Count;
        // Check if the track index is valid
        if (trackIndex >= 0 && trackIndex < outputCount)
        {
            // Retrieve the specific track
            var playableBinding = director.playableAsset.outputs;
            var track = playableBinding.ToList()[trackIndex].sourceObject as TrackAsset;

            // Check if the track is an Animation Track
            if (track is AnimationTrack animationTrack)
            {
                // Set the animator reference for the track
                director.SetGenericBinding(track, animator.gameObject);
            }
        }
    }

   

    private void FreezeConstraints(Rigidbody rb)
    {
        // Retrieve the current constraints
        RigidbodyConstraints constraints = rb.constraints;

        // Freeze position constraints
        constraints |= RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ;

        // Apply the modified constraints to the Rigidbody
        rb.constraints = constraints;
    }

    private void UnfreezeConstraints(Rigidbody rb)
    {
        // Retrieve the current constraints
        RigidbodyConstraints constraints = rb.constraints;

        // Remove position constraints
        constraints &= ~RigidbodyConstraints.FreezePositionX;
        constraints &= ~RigidbodyConstraints.FreezePositionY;
        constraints &= ~RigidbodyConstraints.FreezePositionZ;

        // Apply the modified constraints to the Rigidbody
        rb.constraints = constraints;
    }

    public void EnableDisableGameObjAfterEnd()
    {

        if(enableAfterEndGameObjects != null)
        {
            foreach (GameObject obj in enableAfterEndGameObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(true);
                    print("Object " + obj.name + " Enabled");
                }
            }
        }

        if (disableAfterEndGameObjects != null)
        {
            foreach (GameObject obj in disableAfterEndGameObjects)
            {
                if (obj != null)
                {
                    obj.SetActive(false);
                    print("Object " + obj.name + " Disabled");
                }
            }
        }
       
    }

IEnumerator FinishCut()
    {
        yield return new WaitForSeconds(waitForSecondsToFinishCut - endTimeOffset);
        if (transformPlayerWhereHumanoidFinishtOnScene)
        {
            if (searchForPlayer)
            {
                SearchForPlayer1();
            }
            PlayerTransformEqualsCutSceneHumanoid(playerHolder, cutScenesHumanoidHolder);
        }
        EnableDisableGameObjAfterEnd();
        playerController.enabled = true;
        playersRigBuilder.enabled = true;
        PauseMenu.GameIsPaused = false;
        UnfreezeConstraints(playerHolder.GetComponent<Rigidbody>());
        playerController.cam.gameObject.SetActive(true);
        playerHolder.SetActive(true);
        player.SetActive(false);
        objectToDestroy.SetActive(false);
        if (destroyThisCutSceneWhenEnd)
        {
            Destroy(cutScene.transform.parent.gameObject);
        }
        cutScene.SetActive(false);
        if (destroyCutSceneAfterFinish)
        {
            objectToDestroy.SetActive(false);
        }
        this.gameObject.SetActive(false);
    }

 public  IEnumerator FinishCutAndLoadNextScene()
    {
        yield return new WaitForSeconds(waitForSecondsToFinishCut + endTimeOffset);
        if (transformPlayerWhereHumanoidFinishtOnScene)
        {
            PlayerTransformEqualsCutSceneHumanoid(playerHolder, cutScenesHumanoidHolder);
        }
        EnableDisableGameObjAfterEnd();
        GameObject.FindGameObjectWithTag("GameManager").GetComponent<SavingAndLoading>().LoadNextScene();
        // playerController.enabled = true;
        //   playersRigBuilder.enabled = true;
        
        PauseMenu.GameIsPaused = false;
     //   UnfreezeConstraints(playerHolder.GetComponent<Rigidbody>());
     //   playerHolder.SetActive(true);
       // player.SetActive(false);

       

        cutScene.SetActive(false);
        if (destroyCutSceneAfterFinish)
        {
            objectToDestroy.SetActive(false);
        }
    }

    public void EndCut()
    {
        EnableDisableGameObjAfterEnd();
        playerController.enabled = true;
        playersRigBuilder.enabled = true;
        PauseMenu.GameIsPaused = false;
        UnfreezeConstraints(playerHolder.GetComponent<Rigidbody>());
        playerController.cam.gameObject.SetActive(true);
        playerHolder.SetActive(true);
        player.SetActive(false);
        objectToDestroy.SetActive(false);
        if (destroyThisCutSceneWhenEnd)
        {
            Destroy(cutScene.transform.parent.gameObject);
        }
        cutScene.SetActive(false);
        if (destroyCutSceneAfterFinish)
        {
            objectToDestroy.SetActive(false);
        }
        this.gameObject.SetActive(false);
    }
    public void EnablePlayer()
    {
        playerController.enabled = true;
        playersRigBuilder.enabled = true;
        UnfreezeConstraints(playerHolder.GetComponent<Rigidbody>());
        playerController.cam.gameObject.SetActive(true);
        playerHolder.SetActive(true);

        if (destroyCutSceneAfterFinish)
        {
            objectToDestroy.SetActive(false);
        }
       
    }
    public void EnablePlayerForTimeLine()
    {
        playerController.enabled = true;
        playersRigBuilder.enabled = true;
        UnfreezeConstraints(playerHolder.GetComponent<Rigidbody>());
        playerController.cam.gameObject.SetActive(false);
        playerHolder.SetActive(true);

      //  if (destroyCutSceneAfterFinish)
      //  {
      //      objectToDestroy.SetActive(false);
     //   }

    }
}
