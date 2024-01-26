using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : HealthManager
{
    public float health = 100f;
    private void Start()
    {
        
    }
    public override void TakeDamage(Vector3 location, Vector3 direction, float damage, Collider bodyPart = null, GameObject origin = null)
    {

        // this.GetComponent<PlayerController>().currentHealth -= damage;
      
            this.GetComponent<PlayerController>().Damage(damage);

        if (this.GetComponent<PlayerController>().currentHealth < 0)
        {
            dead = true;
            this.GetComponent<PlayerController>().currentHealth = 0;
            Debug.LogWarning("PlayerDied");
        }
        else
            Debug.LogWarning(this.GetComponent<PlayerController>().currentHealth);
    }
}
