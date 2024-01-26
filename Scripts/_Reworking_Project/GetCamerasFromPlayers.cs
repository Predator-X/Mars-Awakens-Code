using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetCamerasFromPlayers : MonoBehaviour
{
    
    public List<GameObject> cameras; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
           GameObject cam = other.gameObject.transform.Find("Main Camera (1)").gameObject;
            if(cam != null)
            {
                cameras.Add(cam);
            }
        }
    }
}
