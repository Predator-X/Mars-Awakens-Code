using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public class PlayerSpawnPointManager : MonoBehaviour
{
    private List<PlayerInput> players = new List<PlayerInput>();
    [SerializeField] private List<Transform> startingPoints;
    [SerializeField] private List<LayerMask> playerLayers;
    [SerializeField] private List<LayerMask> playerCamLayers;

    private PlayerInputManager playerInputManager;
    private GameObject playerPrefabForLobby;

    [SerializeField]
    private string player1_VariantName = "sci fi soldier 2";//"MainPlayerVae1 Variant 1";

    //Multiplayer
    private int getNumberOfPlayersToSpawn;
    int NumberOfPlayers,playersCurrentlyJoined;
    GameObject playerBodyHolder;

    //Players body scale (position will require to change when used diffrent scale look down in the code 
    [SerializeField]
    Vector3 playerBodyScale = new Vector3(0.5f, 0.5f, 0.5f);

    private void Awake()
    {
        getNumberOfPlayersToSpawn = PauseMenu.numberOfPlayers;
        playerInputManager = FindObjectOfType<PlayerInputManager>();
    }
    private void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex == 1)//<--------If its In MainMenu 
        {
            playerPrefabForLobby = Resources.Load<GameObject>("sci fi soldier 2 For MainMenu");//"MainPlayerVae1 Variant 1 ForPlayer2 Variant");
                                                                                               // Debug.Log(playerPrefabForLobby.name + "AAAAAAAAAAAAAAA");

            playerInputManager.playerPrefab = playerPrefabForLobby;
            GameObject holder = Instantiate(playerPrefabForLobby.gameObject, startingPoints[0].transform.position, startingPoints[0].rotation);// Quaternion.identity);
            players.Add(holder.GetComponent<PlayerInput>());
            
            // holder.transform.Rotate(0f, 90f, 0f);
            //***********************************************************************************************************************
            EventSystem.current.SetSelectedGameObject(GameObject.FindGameObjectWithTag("GameManager").GetComponent<PauseMenu>().startButtonMainMenu);
            //**********************************************************************************************************************************

        }



        if (SceneManager.GetActiveScene().buildIndex > 1)
        {
            // GameObject.FindGameObjectWithTag("MainCamera").SetActive(false);
            for (int i = 1; i <= getNumberOfPlayersToSpawn; i++)
            {
                Debug.Log(i + " ___________________________");
                if (i == 1)
                {

                    playerPrefabForLobby = Resources.Load<GameObject>(player1_VariantName); //Resources.Load<GameObject>("MainPlayerVae1 Variant 1");

                   playerBodyHolder = Instantiate(playerPrefabForLobby.gameObject, startingPoints[0].transform.position, startingPoints[0].rotation);
                    
                   // GameObject playerBodyHolder = Instantiate(playerPrefabForLobby.gameObject, startingPoints[0].transform.position, startingPoints[0].rotation);// Quaternion.identity);
                                                                                                                               // holder.transform.Rotate(0f, 90f, 0f);
                                                                                                                                                                                                        // GameObject.FindGameObjectWithTag("MainCamera").SetActive(false);
                                                                                                                                                                                                                                       //  playerInputManager.playerPrefab = playerPrefabForLobby;
                                                                                                                                                                                                                                       //   playerInputManager.onPlayerJoined += AddPlayer;
                }
                else if (i == 2)
                {
                    playerPrefabForLobby = Resources.Load<GameObject>("sci fi soldier 2 For Player2");//"MainPlayerVae1 Variant 1 ForPlayer2");

                    playerBodyHolder.transform.Find("Main Camera (1)").GetComponent<Camera>().rect = new Rect(0.0f, 0.5f, 1f, 0.5f); //<- chenge sieze of camera of player 1 to fit both views of players onto the screen

                    GameObject holder = Instantiate(playerPrefabForLobby.gameObject, startingPoints[1].transform.position, startingPoints[1].rotation);
                    
                    holder.transform.Find("Main Camera (1)").GetComponent<Camera>().rect = new Rect(0.0f, 0.0f, 1f, 0.5f);
                    //  GameObject holder = Instantiate(playerPrefabForLobby.gameObject, startingPoints[1].transform.position, Quaternion.identity); ;

                    // Instantiate(playerPrefabForLobby);
                    // playerInputManager.onPlayerJoined += AddPlayer;
                    Debug.Log("1!!!!!!!!!!!!!!!!!!!");
                }
                else if (i == 3)
                {
                    // playerPrefabForLobby = Resources.Load<GameObject>("MainPlayerVae1 Variant 1 ForPlayer2");
                    // Instantiate(playerPrefabForLobby);
                    //  playerInputManager.playerPrefab = playerPrefabForLobby;
                    //  Debug.Log("2!!!!!!!!!!!!!!!!!!!");
                    //  playerInputManager.onPlayerJoined += AddPlayer;
                }
                // AddPlayer(players[i]);

            }
        }
    }

    void OnEnable()
    {
        playerInputManager.onPlayerJoined += AddPlayer;
    }

    void OnDisable()
    {
        playerInputManager.onPlayerJoined -= AddPlayer;
    }


    public void AddPlayer(PlayerInput player)
    {
        PauseMenu.numberOfPlayers = playerInputManager.playerCount;
        if (SceneManager.GetActiveScene().buildIndex == 1)//<--------If its In MainMenu 
        {
            // playerInputManager.playerPrefab = playerPrefabForLobby;
            NumberOfPlayers = playerInputManager.playerCount;
            int i = NumberOfPlayers;//playerInputManager.playerCount;//<---- As playerCount+1  will print 11
            i++;
            GameObject nOPtext;

            nOPtext = GameObject.FindGameObjectWithTag("GameManager").GetComponent<PauseMenu>().numberOfPlayersTMP.gameObject;
            //  nOPtext = GameObject.FindGameObjectWithTag("GameManager").GetComponent<SavingAndLoading>().numberOfPlayersText.gameObject;
            //  if (nOPtext == null) { nOPtext = GameObject.FindGameObjectWithTag("GameManager").GetComponent<PauseMenu>().numberOfPlayersTMP.gameObject; }//try find it in Pause Menu

            //  nOPtext.SetActive(true);
            nOPtext.GetComponent<TextMeshProUGUI>().
                SetText("Number Of Local Players: " + playerInputManager.playerCount);//i);
            Debug.Log(playerInputManager.playerCount + " player count");



            if (i == 3)
            {
                playerPrefabForLobby = Resources.Load<GameObject>("sci fi soldier 2 For MainMenu 2");//"MainPlayerVae1 Variant 1 ForPlayer2");
                GameObject holder = Instantiate(playerPrefabForLobby.gameObject, startingPoints[1].transform.position, startingPoints[1].rotation); ;

                nOPtext.GetComponent<TextMeshProUGUI>().
              SetText("Number Of Local Players: " + 2);//i);
            }


        }
        else if(SceneManager.GetActiveScene().buildIndex != 1)
        {
           
            players.Add(player);
            player.transform.rotation = startingPoints[0].rotation;
            player.transform.position = startingPoints[0].position;
            Debug.LogWarning("Player Added--" + player.gameObject.name);
        }

      

        //need to use the parent due to the structure of the prefab
        //Transform playerParent = player.transform.parent;
        //  playerParent.position = startingPoints[players.Count - 1].position;
       

        /* //For Last Scene Change player Scale to match scene________________________________________
         if (SceneManager.GetActiveScene().buildIndex == 200)
         {
             //it would be better to group the body parts so it would be done by one line of code to scale and no position required
             //but it will require to chenge code in differnt places so I wont do it as wont to finish this project as fast as possible 
             Vector3 calculations = new Vector3(0, 0, 0);

             player.transform.GetChild(0).transform.localScale = playerBodyScale; //Hand

             calculations = player.transform.GetChild(0).transform.localPosition;  // * Multiplay as 0.5 its like / divide
             player.transform.GetChild(0).transform.localPosition = new Vector3(calculations.x * playerBodyScale.x, calculations.y * playerBodyScale.y,
                 calculations.z * playerBodyScale.z);


             player.transform.GetChild(1).transform.localScale = playerBodyScale; //head 

             calculations = player.transform.GetChild(1).transform.localPosition;
             player.transform.GetChild(1).transform.localPosition = new Vector3(calculations.x * playerBodyScale.x, calculations.y * playerBodyScale.y,
                 calculations.z * playerBodyScale.z); //change head position as it's got scaled so its looks proportional

             player.transform.GetChild(2).transform.localScale = playerBodyScale;//Body
         }
         //______________________________________________________________________________________________
        */
        /*------------------------------------------Set the layers to speperate the cameras from chumping to one player but droppt it as done manually and 
         * game changed from many players to only two so I decidet to do it manually as of that 
         * this code can be used to do it automatically but its needs to make shoure to detect the first player as off layer issue in camera layer
          if(SceneManager.GetActiveScene().buildIndex != 1)
          {
              //convert layer mask (bit) to an intiger
              int layerToAdd = (int)Mathf.Log(playerLayers[players.Count - 1].value, 2);
              int camLayerToAdd = (int)Mathf.Log(playerCamLayers[players.Count - 1].value, 2);
              //set the layer
              // playerParent.GetComponentInChildren<Camera>().gameObject.layer = layerToAdd; 
              player.GetComponentInChildren<Camera>().gameObject.layer = layerToAdd + 1;
              player.GetComponentInChildren<Camera>().gameObject.layer = camLayerToAdd + 1;
              //add the layer
              //playerParent.GetComponentInChildren<Camera>().cullingMask |= 1 << layerToAdd;
              player.GetComponentInChildren<Camera>().cullingMask |= 1 << layerToAdd;
              //set the action in the custom cinemachine Input Handler
              // playerParent.GetComponentInChildren<InputHandlerForCinemachine>().horizontal = player.actions.FindAction("Look");
              player.GetComponentInChildren<InputHandlerForCinemachine>().horizontal = player.actions.FindAction("Look");



          }

          Debug.Log(PauseMenu.numberOfPlayers + " ssssssssssssss");
        *///--------------------------------------------------------------------------------------------
    }

}
