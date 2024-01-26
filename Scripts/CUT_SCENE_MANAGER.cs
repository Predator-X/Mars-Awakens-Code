using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Playables;


//[CreateAssetMenu(fileName = "create CUT_SCENE_MANAGER", menuName = " create CUT_SCENE_MANAGER")]

public class CUT_SCENE_MANAGER : MonoBehaviour//ScriptableObject
{
    [SerializeField] GameObject TimeLine;
    [SerializeField] bool enable = false, disable = false,destroy, onAwake, onStart, onTrigger, onTimeLineEnd;
    [SerializeField] GameObject[] DisableList, EnableList,DestroyList;
    [SerializeField] bool finishCut, finishCutAndLoadNextScene;

    PlayableDirector playableDirector;
    double timeLineEndTime;

    // Start is called before the first frame update
    void Awake()
    {
        
        playableDirector = TimeLine.GetComponent<PlayableDirector>();
        timeLineEndTime = ((float)playableDirector.duration);
        if (onTrigger)
        {
            SphereCollider sphereCollider = this.AddComponent<SphereCollider>();
            sphereCollider.radius = 1f;
        }
        if (enable && onAwake)
        {
            EnableGameObjects();
        }
        else if (disable && onAwake)
        {
            DisableGameObjects();
        }
        else if(destroy && onAwake)
        {
            DestroyGameObjects();
        }
    }
    void Start()
    {
        

        if (enable && onStart)
        {
            EnableGameObjects();
        }
        else if (disable && onStart)
        {
            DisableGameObjects();
        }
        else if (destroy && onStart)
        {
            DestroyGameObjects();
        }
    }

   

 
    
    void Update()
    {
        if(onTimeLineEnd && timeLineEndTime == playableDirector.time)
        {
            if (enable)
            {
                EnableGameObjects();
            }
            if (disable)
            {
                DisableGameObjects();
            }
            if (destroy)
            {
                DestroyGameObjects();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (enable && onTrigger)
            {
                EnableGameObjects();
            }
            else if (disable && onTrigger)
            {
                DisableGameObjects();
            }
            else if (destroy && onTrigger)
            {
                DestroyGameObjects();
            }

        }
    }

    public void EnableGameObjects()
    {
        for (int i=0; i< EnableList.Length; i++)
        {
            EnableList[i].gameObject.SetActive(true);

        }
    }

    public void DisableGameObjects()
    {
        for (int i = 0; i < DisableList.Length; i++)
        {
            DisableList[i].gameObject.SetActive(true);

        }
    }

    public void DestroyGameObjects()
    {
        for (int i = 0; i < DestroyList.Length; i++)
        {
            Destroy(DestroyList[i].gameObject);

        }
    }


}
