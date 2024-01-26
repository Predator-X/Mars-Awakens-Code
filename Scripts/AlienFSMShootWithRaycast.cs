using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlienFSMShootWithRaycast : ShootWithRaycast
{
    public override void Shoot()
    {
        Transform child = transform.GetChild(transform.childCount - 1);


        // Update the time when our player can fire next
        nextFire = Time.time + fireRate;

        // Start our ShotEffect coroutine to turn our laser line on and off
        StartCoroutine(ShotEffect());

        //Transform gOrgin;
        Vector3 rayOrigin, gOrgin;

        //Create Vector form GunPoint
        rayOrigin = transform.Find("GunHolder").transform.Find("Gun").transform.Find("ShootPoint").transform.position;
        gOrgin = transform.Find("GunHolder").transform.Find("Gun").transform.Find("ShootPoint").transform.forward;

        // Declare a raycast hit to store information about what our raycast has hit
        RaycastHit hit;

        // Set the start position for our visual effect for our laser to the position of gunEnd
        laserLine.SetPosition(0, gunEnd.position);

        // Check if our raycast has hit anything
        if (Physics.Raycast(rayOrigin, gOrgin//gOrgin.transform.forward// gOrgin.transform.forward//fpsCam.transform.forward
           , out hit, weaponRange, ~ignoreLayer))


        {
            // Set the end position for our laser line 
            laserLine.SetPosition(1, hit.point);

            // Get a reference to a health script attached to the collider we hit
            AlienFSM health = hit.collider.GetComponent<AlienFSM>();
            if (health == null)
            {
                health = hit.collider.GetComponentInParent<AlienFSM>();

            }
            //If Script is attachet to Enemy
            if (tag == "Enemy" || tag == "EnemyBHT" || tag == "EnemyOther" && health != null)
            {
                // Call the damage function of that script, passing in our gunDamage variable
                health.Damage(gunDamage);

            }
            // Check if the object we hit has a rigidbody attached
            if (hit.rigidbody != null)
            {
                // Add force to the rigidbody we hit, in the direction from which it was hit
                hit.rigidbody.AddForce(-hit.normal * hitForce);
            }
        }
        else
        {
            //If we did not hit anything, set the end of the line to a position directly in fornt of the weapon at the distance weponRange
            laserLine.SetPosition(1, rayOrigin + (gOrgin * weaponRange));
            isShootingAtObstacle = false;
        }
    }
}
