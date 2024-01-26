using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerKilledOnTimeLine : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        PauseMenu.pauseTheGamePressed = true;
        PauseMenu.GameIsPaused = true;
        PauseMenu.playerHasDied = true;
    }

 
}
