using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BHTshoot : MonoBehaviour
{
    [SerializeField] float shootTimer = 1.0f;
    [SerializeField] GameObject bullet;
    [SerializeField] Transform shootPoint;
    [SerializeField] float bulletVelocity = 30f;
    public bool isReadyToShoot = false;

    float waitingTime;
    // Update is called once per frame
    void Update()
    {
        if (isReadyToShoot)
        {
            // StartCoroutine(Shoot(shootTimer));
            waitingTime += Time.deltaTime;
            if (waitingTime > shootTimer)
            {
                TakeShoot();
                waitingTime = 0;
            }
        }

      

    }


  public virtual void TakeShoot()
    {
        // Instantiate the bullet prefab.
        GameObject newBullet =
            GameObject.Instantiate(bullet, shootPoint.position, shootPoint.rotation * bullet.transform.rotation) as GameObject;
        // Give it a velocity
        if (newBullet.GetComponent<Rigidbody>() == null)
            // Safeguard test, altough the rigid body should be provided by the
            // prefab to set its weight.
            newBullet.AddComponent<Rigidbody>();

        newBullet.GetComponent<Rigidbody>().velocity = bulletVelocity * shootPoint.forward;
    }

}
