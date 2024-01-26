using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeSightFSM : MonoBehaviour
{
    EnemyFSM thisFsm;
    private void Start()
    {
        thisFsm = this.gameObject.GetComponentInParent<EnemyFSM>();
        if (thisFsm == null)
        {
            Debug.LogError("Enemy is Missing or cannot get EnemyFSM script by object" + this.gameObject.name);
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {

            thisFsm.playerTransform = other.gameObject.transform;
            thisFsm.Attack();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            thisFsm.playerTransform = other.gameObject.transform;
            thisFsm.isAttacking = false;

            GameObject objToSpawn = new GameObject("LastSeenAt");
            objToSpawn.transform.position = other.transform.position;
            thisFsm.IsSeenAt(objToSpawn);
            thisFsm.playerTransform = objToSpawn.transform;
           
            Destroy(objToSpawn, thisFsm.lastSeenSpotTimer);

            thisFsm.isChasing = true;

            thisFsm.Chase();

            Coroutine shoot = thisFsm.shootingCoroutine;
            if (shoot != null)
            {
                thisFsm.isAttacking = false;
                StopCoroutine(shoot);
            }
        }
    }
}
