using System.Collections;
using System.Collections.Generic;
using BBUnity.Actions;
using UnityEngine;
using Pada1.BBCore;           // Code attributes
using Pada1.BBCore.Tasks;     // TaskStatus
using BBUnity.Actions;        // GOAction
using UnityEngine.UIElements;
using UnityEngine.AI;


[Action("TakeCover")]
[Help("Takes Cover.")]
public class TakeCover : GOAction
{
    [InParam("HidingSpots")]
    [Help("Target to check the distance")]
    public GameObject hideObjects;

    Transform destinationPoint;
    bool destinationSet = false , gotToDestination=false;
 

    private Transform DeterminClosestCoverPoint()
    {
        
        if (hideObjects == null)
            hideObjects = GameObject.FindGameObjectWithTag("Cover");
        //return TaskStatus.FAILED;

        
            this.gameObject.GetComponent<NavMeshAgent>().SetDestination(GameObject.FindGameObjectWithTag("Cover").transform.position);
            // DeterminClosestCoverPoint();
            this.gameObject.GetComponent<myHealth>().health = 90;


        /*
        float distance = -0.01f;
        for (int i=0; i<hideObjects.Length; i++)
        {  
            if (distance > Vector3.Distance(this.gameObject.transform.position, hideObjects[i].transform.position))
            {
                distance = Vector3.Distance(this.gameObject.transform.position, hideObjects[i].transform.position);
                destinationPoint = hideObjects[i].transform; //.GetChild(0).gameObject.transform;
            }
        }
        if(destinationPoint != null)
        {
            this.gameObject.GetComponent<NavMeshAgent>().SetDestination(destinationPoint.position);
            destinationSet = true;
        }
        /////////////
        */
         return hideObjects.transform;//destinationPoint;
        
    }

    public override void OnStart()
    {
        
    
    

        //hideObjects.transform.position);
        DeterminClosestCoverPoint();
        
        base.OnStart();

    }

    public override TaskStatus OnUpdate()
    {
        if (this.gameObject.transform.position != hideObjects.transform.position)
        {
            gotToDestination = false;
            return TaskStatus.RUNNING;
        }
       // if (hideObjects == null) { return TaskStatus.FAILED; }
        //DeterminClosestCoverPoint();
       // if (destinationSet && this.gameObject.transform.position == destinationPoint.position)
       // {
          //  return TaskStatus.COMPLETED;
      //  }//else return TaskStatus.RUNNING;
      else if (this.gameObject.transform.position == hideObjects.transform.position)
        {
            this.gameObject.GetComponent<myHealth>().health = 90;
            gotToDestination = true;
            return TaskStatus.COMPLETED;
        }
        return gotToDestination ? TaskStatus.FAILED : TaskStatus.COMPLETED;//this.gameObject.transform.position == hideObjects.transform.position ? TaskStatus.RUNNING : TaskStatus.COMPLETED; //destinationPoint.position ? TaskStatus.RUNNING : TaskStatus.COMPLETED;
    }



    /////////////////////////////////////////////////////////////////////

}