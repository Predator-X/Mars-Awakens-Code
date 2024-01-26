using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurnOffObjectListOnStart : MonoBehaviour
{
    public GameObject objectScriptHolder;
    List<GameObject> objects;
    // Start is called before the first frame update
    void Start()
    {
        objects = objectScriptHolder.gameObject.GetComponent<GetCamerasFromPlayers>().cameras;

        for (int i = 0; i < objects.Count; i++)
        {
            if (objects[i] !=null)
            objects[i].gameObject.SetActive(false);
        }
    }

  
}
