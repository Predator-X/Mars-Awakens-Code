using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameIsPaused : MonoBehaviour
{
    [SerializeField]
    bool gameIsPaused = true;
    // Start is called before the first frame update
    void Start()
    {
        PauseMenu.GameIsPaused = gameIsPaused;
    }


}
