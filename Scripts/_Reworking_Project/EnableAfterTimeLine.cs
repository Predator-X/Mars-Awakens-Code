using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnableAfterTimeLine : DisableAfterTimeLine
{
    
    [SerializeField]
    GameObject[] EnableList;
    public override void DisableGameObjects()
    {
        for (int i = 0; i < EnableList.Length; i++)
        {
            EnableList[i].gameObject.SetActive(true);
            Debug.LogWarning("Disableing gameObject " + EnableList[i].name);
        }
    }
}
