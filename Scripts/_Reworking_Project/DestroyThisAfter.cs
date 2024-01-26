using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyThisAfter : MonoBehaviour
{
    [SerializeField]
    float destroyAfterSec;

    void Start()
    {
        Destroy(this.gameObject, destroyAfterSec);
    }
  
}
