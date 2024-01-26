using EnemyAI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FindPlayersInLayers : MonoBehaviour
{
    //Verry in Efficient script go with it as timeLine
    //---------------------------------------------------
    [SerializeField]// private LayerMask targetLayers; // Serialized field to select layers
    private GameObject[] players;
    [SerializeField]
    private GameObject[] enemys;

    private void Start()
    {

        FindPlayersAndSetAIMTargetToEniemies();
    }

    private void FindPlayersAndSetAIMTargetToEniemies()//GameObject[] FindPlayersOnLayers()
    {
        players = GameObject.FindGameObjectsWithTag("Player");

        enemys = GameObject.FindGameObjectsWithTag("Enemy");

        for(int i=0; i<players.Length; i++)
        {
            enemys[i].GetComponent<StateController>().aimTarget = players[0].transform;//transform.Find("spine_03.x").transform;
        }

       
    }
}
