//This Class Is setUp for being Parent Class For later creation diffrent Kinds of Wepons BUT so far no class inherits from it.
//INHERITANCE // ShootWithProTracer is extending this class
//ABSTRACTION
//ENCAPSULATION

using UnityEngine;
using System.Collections;
using Cinemachine.Utility;
using EnemyAI;

public class ShootWithRaycast : MonoBehaviour
{
    public int gunDamage = 1, damageExtra=100;                                           // Set the number of hitpoints that this gun will take away from shot objects with a health script
    public float fireRate = 0.25f;
    [SerializeField]protected float weaponRange = 50f;// Number in seconds which controls how often the player can fire
                                   // Distance in Unity units over which the player can fire
    public float hitForce = 100f;                                       // Amount of force which will be added to objects with a rigidbody shot by the player
    public Transform gunEnd;                                            // Holds a reference to the gun end object, marking the muzzle location of the gun
   
    [SerializeField]
    protected Camera fpsCam;                                              // Holds a reference to the first person camera
    protected WaitForSeconds shotDuration = new WaitForSeconds(0.07f);    // WaitForSeconds object used by our ShotEffect coroutine, determines time laser line will remain visible
    protected AudioSource gunAudio;                                       // Reference to the audio source which will play our shooting sound effect
    protected LineRenderer laserLine;                                     // Reference to the LineRenderer component which will display our laserline
    protected float nextFire;                                             // Float to store the time the player will be allowed to fire again, after firing

    
    public LayerMask ignoreLayer, obstacleLayer;                                       // Igonre Raycast for shooting 

   protected bool isItPlayer , isShootingAtObstacle=false;

    //ENCAPSULATION
    public float WeaponRange
    {
        get
        {
            return weaponRange;
        }
        set
        {
            if (value < 0.0f)
            {
                Debug.LogError("WeponRange Cannot be set to negative number!");
            }
            else { weaponRange = value; }

        }
    }


  //  string nameOfLastHeadShot = "a";

    private void Awake()
    {
        if (this.tag == "Player")
        {
            isItPlayer = true;

        }
        else isItPlayer = false;
    }

    void Start()
    {
        GetAllNeceserry();

    }


 


   public virtual void GetAllNeceserry()
    {
        // Get and store a reference to our LineRenderer component
        laserLine = GetComponent<LineRenderer>();

        // Get and store a reference to our AudioSource component
        gunAudio = GetComponent<AudioSource>();

        // Get and store a reference to our Camera by searching this GameObject and its parents
        // fpsCam = GetComponentInParent<Camera>();

        // fpsCam = GameObject.FindGameObjectWithTag("CameraPlayer").GetComponent<Camera>();//<----------------As becouse multiplayer
        //  fpsCam = this.transform.GetChild(3).gameObject.GetComponent<Camera>();
       // GameObject obj = this.transform.Find("Main Camera").gameObject;
       // fpsCam = obj.GetComponent<Camera>();//this.transform.Find("Main Camera").gameObject.GetComponent<Camera>();
    }

    void GetLaserLine()
    {
        // Get and store a reference to our LineRenderer component
        laserLine = GetComponent<LineRenderer>();
    }

    void GetGunAudio()
    {
        // Get and store a reference to our AudioSource component
        gunAudio = GetComponent<AudioSource>();
    }

    void FindCam()
    {
        // Get and store a reference to our Camera by searching this GameObject and its parents
        //->as converted to multiplayer I gonna use different fpsCam = GameObject.FindGameObjectWithTag("CameraPlayer").GetComponent<Camera>();
        fpsCam = GetComponentInChildren<Camera>();
    }


    //ABSTRACTION
  public virtual void Shoot()
    {
        Transform child = transform.GetChild(transform.childCount - 1);
      

        // Update the time when our player can fire next
        nextFire = Time.time + fireRate;

        // Start our ShotEffect coroutine to turn our laser line on and off
        StartCoroutine(ShotEffect());

        //Transform gOrgin;
        Vector3 rayOrigin , gOrgin;

       if(isItPlayer == true)
        {

            // Create a vector at the center of our camera's viewport
            rayOrigin = fpsCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));
               gOrgin = fpsCam.transform.forward;
        }
       else if (this.gameObject.CompareTag("EnemyOther") || this.gameObject.CompareTag("AlienFSM"))
        {
            //Create Vector form GunPoint
            rayOrigin = transform.Find("GunHolder").transform.Find("Gun").transform.Find("ShootPoint").transform.position;
            gOrgin = transform.Find("GunHolder").transform.Find("Gun").transform.Find("ShootPoint").transform.forward;
        }
        else 
        {
            //Create Vector form GunPoint
            rayOrigin = transform.Find("GunHolder").transform.Find("Hand").transform.Find("GunShootPosition").transform.position;
            gOrgin = transform.Find("GunHolder").transform.Find("Hand").transform.Find("GunShootPosition").transform.forward;
           
        }
      
        // Declare a raycast hit to store information about what our raycast has hit
        RaycastHit hit;

        // Set the start position for our visual effect for our laser to the position of gunEnd
        laserLine.SetPosition(0, gunEnd.position);

        //| Check if our raycast has hit anything----THIS HAS BEEN CHANGED AS MOVED FROM BEHAVIOR TREES TO ENEMIE AI-------IF COMING BACK TO BHT PASTE THE CODE FROM BELLOW HERE--->| Paste_Behaviour_Tree_Check_if_Raycast_have_Hit_Anything
        //V                    ALESO--CHANGE_TAG"Enemys back to Enemy WHEN MOVING BACK TO BHT.BELOW                                                                                 V
        if (Physics.Raycast(rayOrigin, gOrgin//gOrgin.transform.forward// gOrgin.transform.forward//fpsCam.transform.forward
          , out hit, weaponRange,~ignoreLayer))

         
            {
            // Set the end position for our laser line 
            laserLine.SetPosition(1, hit.point);

            Debug.LogWarning("Fire At " + hit.collider.name);

            // Get a reference to a health script attached to the collider we hit
            Character health = hit.collider.GetComponent<Character>();
            if(health == null)
            {
                health = hit.collider.GetComponentInParent<Character>();
                
            }

            //If script is Attachet to Player
            if(tag == "Player")
            {

                if(hit.collider.gameObject.layer==LayerMask.NameToLayer("Enemy"))//.transform.parent.CompareTag("Enemy"))
                {
                   // hit.collider.transform.GetComponentInParent<EnemyHealth>().TakeDamage(rayOrigin, gOrgin, gunDamage, hit.collider);
                    hit.collider.SendMessageUpwards("HitCallback", new HealthManager.DamageInfo(rayOrigin, gOrgin, gunDamage, hit.collider), SendMessageOptions.DontRequireReceiver);
                    Debug.LogWarning("EnemyGotHIT");
                }
                
                else
                {
                    //Call the alter manager to notify the shot noise
                    GameObject.FindGameObjectWithTag("GameController").SendMessage("RootAlterNearby", rayOrigin, SendMessageOptions.DontRequireReceiver);
                    print("Hit Other : " + hit.collider.name);
                }
            }


            //If Script is attachet to Enemy------------------------CHANGE_TAG"Enemys back to Enemy WHEN MOVING BACK TO BHT.__________________CHANGE TAG HERE WHERE USING BHT.
            if (tag=="Enemys"|| tag=="EnemyBHT" || tag=="EnemyOther" || tag=="AlienFSM" && health != null)
            {
                 if (hit.transform.gameObject.layer != obstacleLayer && !hit.collider.CompareTag("AlienFSM"))
                {
                    health.Damage(gunDamage);
                }
                else 
                { // Call the damage function of that script, passing in our gunDamage variable
                    hit.collider.GetComponentInParent<AlienFSM>().Damage(gunDamage);
                }

            }


            // Check if the object we hit has a rigidbody attached
            if (hit.rigidbody != null)
            {
                // Add force to the rigidbody we hit, in the direction from which it was hit
                hit.rigidbody.AddForce(-hit.normal * hitForce);
            }

            if(hit.transform.tag == "Wall")
            {
                isShootingAtObstacle = true;
            }
            if (hit.transform.tag != "Wall")
            {
                isShootingAtObstacle = false;
            }
        }
        else
        {
           //If we did not hit anything, set the end of the line to a position directly in fornt of the weapon at the distance weponRange
            laserLine.SetPosition(1, rayOrigin + (gOrgin * weaponRange));
            isShootingAtObstacle = false;
            print("hitNothing");
        }
    }


    protected virtual IEnumerator ShotEffect()
    {
        // Play the shooting sound effect
        gunAudio.Play();

        // Turn on our line renderer
        laserLine.enabled = true;

        //Wait for .07 seconds
        yield return shotDuration;

        // Deactivate our line renderer after waiting
        laserLine.enabled = false;
    }

    public virtual bool IsItShootingAtObstacle()
    {
        return isShootingAtObstacle;
    }


    IEnumerator DestroyAfterTime(GameObject g , float afterTime)
    {
        yield return new WaitForSeconds(afterTime);
        Destroy(g);
        g.SetActive(false);
    }


}




/////______________________________if_not_using_Enemy_AI__use_The _Code_Underneath_______________________
///Paste_It_in_Shoot_Method_WhereIsNote("Paste_Behaviour_Tree_Check_if_Raycast_have_Hit_Anything")
///
/*  if (Physics.Raycast(rayOrigin, gOrgin//gOrgin.transform.forward// gOrgin.transform.forward//fpsCam.transform.forward
          , out hit, weaponRange,~ignoreLayer))

      
            {
            // Set the end position for our laser line 
            laserLine.SetPosition(1, hit.point);

            // Get a reference to a health script attached to the collider we hit
            Character health = hit.collider.GetComponent<Character>();
            if(health == null)
            {
                health = hit.collider.GetComponentInParent<Character>();
                
            }

            //If script is Attachet to Player
            if(tag == "Player")
            {

                if (hit.collider.name == "Head" )
                {
                   
                    hit.collider.name = "HeadShooted";
                    if (hit.collider.gameObject.transform.parent.CompareTag("Enemy"))
                    {
                        
                        health = hit.collider.GetComponentInParent<Enemy>();
                        //damageExtra = 10;
                        //hit.collider.GetComponentInParent<Enemy>().enemyIsHitOnHead = true;
                        hit.collider.GetComponentInParent<Enemy>().EnemyIsHitOnHead(true);
                        if (health != null)
                        {
                            //  hit.collider.GetComponentInChildren<DestroyAfterTime>().StartCounting();
                            /////  StartCoroutine(DestroyAfterTime(hit.transform.gameObject, 3));
                            health.Damage(gunDamage + damageExtra);
                            GetComponent<PlayerController>().AddScore(gunDamage + damageExtra);

                        }
                        else if (health == null) { Debug.LogError("health: When Shoot With head Probably canot find character script in Parent!----ShootWithRayCast c#"); }

                    }
                    else if (hit.collider.gameObject.transform.parent.CompareTag("EnemyBHT"))
                    {
                        health = hit.collider.GetComponentInParent<EnemyBHT>();
                       // damageExtra = 10;

                        hit.collider.GetComponentInParent<EnemyBHT>().EnemyIsHitOnHead(true);
                        if (health != null)
                        {
               
                            health.Damage(gunDamage + damageExtra);
                            GetComponent<PlayerController>().AddScore(gunDamage + damageExtra);

                        }
                        else if (health == null) { Debug.LogError("health: When Shoot With head Probably canot find character script in Parent!----ShootWithRayCast c#"); }
                    }
                    else if (hit.collider.gameObject.transform.parent.CompareTag("EnemyOther"))
                    {   
                            health = hit.collider.GetComponentInParent<EnemyFSM>();
                        
                        //damageExtra = 10;
                        hit.collider.GetComponentInParent<EnemyFSM>().EnemyIsHitOnHead(true);
                        if (health != null)
                        {
                          
                            health.Damage(gunDamage + damageExtra);
                            GetComponent<PlayerController>().AddScore(gunDamage + damageExtra);

                        }
                        else if (health == null) { Debug.LogError("health: When Shoot With head Probably canot find character script in Parent!----ShootWithRayCast c#"); }
                    }
                    else if (hit.collider.gameObject.transform.parent.CompareTag("AlienFSM"))
                    {
                        AlienFSM alienHealth= hit.collider.GetComponentInParent<AlienFSM>();
                  
                      //  damageExtra = 100;

                        alienHealth.enemyIsHitOnHead=true;
                        if (alienHealth != null)
                        {
                            alienHealth.Damage(gunDamage + damageExtra);
                            GetComponent<PlayerController>().AddScore(gunDamage + damageExtra);

                        }
                        else if (alienHealth == null) { Debug.LogError("AlienFSMhealth: When Shoot With head Probably canot find character script in Parent!----ShootWithRayCast c#"); }
                    }

                }
*///------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------