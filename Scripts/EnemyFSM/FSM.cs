using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FSM : MonoBehaviour
{
    protected virtual void Initialize() { }

    protected virtual void FSMUpdate() { }

    protected virtual void FSMFixedUpdate() { }

    //Use this to Initialize
    private void Start()
    {
        Initialize();
    }

    //Update is called once per frame 
    private void Update()
    {
        FSMUpdate();
    }

    private void FixedUpdate()
    {
        FSMFixedUpdate();
    }




}
