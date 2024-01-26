using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBHT : Character
{  //Dead effect
    public bool enemyIsHitOnHead = false;
    public override void Damage(float damageAmount)
    {
        currentHealth -= damageAmount;


        if (currentHealth <= 0 && enemyIsHitOnHead)
        {
            // GameObject body, gun, head;
            //  body = this.transform.Find("Body").gameObject;
            body.AddComponent<DisActivateAfter>().enabled = true;
           // body.GetComponent<DisActivateAfter>().enabled = true;
            body.AddComponent<Rigidbody>();
            body.transform.parent = null;


           // gun = this.transform.Find("GunHolder").gameObject;
            gun.AddComponent<DisActivateAfter>().enabled = true;
         //   gun.GetComponent<DisActivateAfter>().enabled = true;
            gun.AddComponent<Rigidbody>();
            gun.transform.parent = null;


           // head = this.transform.Find("HeadShooted").gameObject;
           // head.GetComponent<DisActivateAfter>().enabled = true;
           head.AddComponent<DisActivateAfter>().enabled = true;
            head.AddComponent<Rigidbody>();
            head.transform.parent = null;

            isDead = true;
            gameObject.SetActive(false);


        }
        else if (currentHealth <= 0 && enemyIsHitOnHead == false)
        {
            isDead = true;
            gameObject.SetActive(false);
        }

    }

    public virtual void EnemyIsHitOnHead(bool gotHit)
    {
        enemyIsHitOnHead = gotHit;
    }
}
