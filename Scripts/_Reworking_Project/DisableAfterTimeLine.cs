using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class DisableAfterTimeLine : MonoBehaviour
{
    [SerializeField] GameObject TimeLine;
     PlayableDirector playableDirector;

    [SerializeField]
    private GameObject[] DisableList;
   // [SerializeField]
  public float timeLineEndOffset, timeLineDuration, willEndAfterTotalTime;

   
    IEnumerator Start()
    {
        Debug.Log("Starting " + Time.time);
      //  timeLineDuration = ((float)playableDirector.duration);
        willEndAfterTotalTime = timeLineDuration + timeLineEndOffset;
        playableDirector = TimeLine.GetComponent<PlayableDirector>();

     // Start function WaitAndPrint as a coroutine, but don't execute immediately.
        yield return new WaitForSeconds(willEndAfterTotalTime);
          DisableGameObjects();
       
        Debug.Log("After Waiting "+willEndAfterTotalTime+"Seconds " + Time.time);
    }


    public virtual void DisableGameObjects()
    {
        for (int i = 0; i < DisableList.Length; i++)
        {
            DisableList[i].gameObject.SetActive(false);
            Debug.LogWarning("Disableing gameObject " + DisableList[i].name);
        }
    }
}
