using Andtech.ProTracer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShootWithProTracer : ShootWithRaycast
{
    [Header("Prefabs")]
    [SerializeField]
    [Tooltip("The Bullet prefab to spawn.")]
    private Bullet bulletPrefab = default;
    [SerializeField]
    [Tooltip("The Smoke Trail prefab to spawn.")]
    private SmokeTrail smokeTrailPrefab = default;

    [SerializeField]
    [Tooltip("The speed of the tracer graphics.")]
    [Range(1, 10)]
    private int tracerSpeed = 3;
    public float Speed => 10.0F + (tracerSpeed - 1) * 50.0F;

    [Tooltip("Should tracer graphics use gravity while moving?")]
    private bool useGravity = true;

    public override void Shoot()
    {

        // Instantiate the tracer graphics
        Bullet bullet = Instantiate(bulletPrefab);
        SmokeTrail smokeTrail = Instantiate(smokeTrailPrefab);

        // Setup callbacks
        bullet.Completed += OnCompleted;
        smokeTrail.Completed += OnCompleted;
        //-------------------------------------------------------------------------------------------
        Transform child = transform.GetChild(transform.childCount - 1);


        // Update the time when our player can fire next
        nextFire = Time.time + fireRate;

        // Start our ShotEffect coroutine to turn our laser line on and off
        StartCoroutine(ShotEffect());

        //Transform gOrgin;
        Vector3 rayOrigin, gOrgin;

        

            // Create a vector at the center of our camera's viewport
            rayOrigin = fpsCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));
            gOrgin = fpsCam.transform.forward;

        // Setup callbacks
        bullet.Completed += OnCompleted;
        smokeTrail.Completed += OnCompleted;

        // Declare a raycast hit to store information about what our raycast has hit
        RaycastHit hit;

        // Set the start position for our visual effect for our laser to the position of gunEnd
      //  laserLine.SetPosition(0, gunEnd.position);

        //| Check if our raycast has hit anything----THIS HAS BEEN CHANGED AS MOVED FROM BEHAVIOR TREES TO ENEMIE AI-------IF COMING BACK TO BHT PASTE THE CODE FROM BELLOW HERE--->| Paste_Behaviour_Tree_Check_if_Raycast_have_Hit_Anything
        //V                    ALESO--CHANGE_TAG"Enemys back to Enemy WHEN MOVING BACK TO BHT.BELOW                                                                                 V
        if (Physics.Raycast(rayOrigin, gOrgin//gOrgin.transform.forward// gOrgin.transform.forward//fpsCam.transform.forward
          , out hit, weaponRange, ~ignoreLayer))


        {
            // Set the end position for our laser line 
          //  laserLine.SetPosition(1, hit.point);
            bullet.DrawLine(gunEnd.position, hit.point,Speed,0);
            smokeTrail.DrawLine(gunEnd.position, hit.point, Speed, 0);

            // Setup impact callback
            bullet.Arrived += OnImpact;

            void OnImpact(object sender, System.EventArgs e)
            {
                // Handle impact event here
                Debug.DrawRay(hit.point, hit.normal, Color.red, 0.5F);
            }

           // Debug.LogWarning("Fire At " + hit.collider.name);

            // Get a reference to a health script attached to the collider we hit
            Character health = hit.collider.GetComponent<Character>();
            if (health == null)
            {
                health = hit.collider.GetComponentInParent<Character>();

            }

            //If script is Attachet to Player
            if (tag == "Player")
            {

                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Enemy"))//.transform.parent.CompareTag("Enemy"))
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
            if (tag == "Enemys" || tag == "EnemyBHT" || tag == "EnemyOther" || tag == "AlienFSM" && health != null)
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

            if (hit.transform.tag == "Wall")
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
          //  laserLine.SetPosition(1, rayOrigin + (gOrgin * weaponRange));
          //  isShootingAtObstacle = false;
         //   print("hitNothing");

           
                // Since we have no end point, use DrawRay
                bullet.DrawRay(transform.position, transform.forward, Speed, weaponRange, 0, useGravity);
                smokeTrail.DrawRay(transform.position, transform.forward, Speed, 25.0F, 0);
            
        }
    }

    protected override IEnumerator ShotEffect()
    {
        // Play the shooting sound effect
        gunAudio.Play();

        // Turn on our line renderer
        //laserLine.enabled = true;

        //Wait for .07 seconds
        yield return shotDuration;

        // Deactivate our line renderer after waiting
        //laserLine.enabled = false;
    }


    private void OnCompleted(object sender, System.EventArgs e)
    {
        // Handle complete event here
        if (sender is TracerObject tracerObject)
        {
            Destroy(tracerObject.gameObject);
        }
    }

    private float CalculateStroboscopicOffset(float speed) => speed * Time.smoothDeltaTime;

}
