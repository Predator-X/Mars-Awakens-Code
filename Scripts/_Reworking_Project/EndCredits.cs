using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class EndCredits : MonoBehaviour
{
    GameObject gameManager;

    [SerializeField] float loadMainMenuAfter = 60f;//if wona change credits dispplay time go to PauseMenu.cs creditsTimer or GameManagerPrefab creditsTimer
    // Start is called before the first frame update
    void Start()
    {
        gameManager = GameObject.FindGameObjectWithTag("GameManager");

        gameManager.GetComponent<PauseMenu>().EndGamePause();

      
         gameManager.GetComponent<PauseMenu>().SavePlayer(); 


        StartCoroutine(AfterCreditsLoadMainMenu());
    }

    IEnumerator AfterCreditsLoadMainMenu()
    {
        yield return new WaitForSeconds(loadMainMenuAfter);
        gameManager.GetComponent<PauseMenu>().Resume();
        gameManager.GetComponent<PauseMenu>().SetMainMenuON();
        gameManager.GetComponent<SavingAndLoading>().LoadMenu();
       

    }
   
}
