//using System;
using System.Collections;
using creepycat.scifikitvol4;
//using System.Collections.Generic;
//using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class SimpleFSM : FSM
{
    public enum FSMState
    {
        None, Patrol, Chase, Attack, Dead,
    }

    //Current state that the NPC is reaching
    public FSMState curentState = FSMState.Patrol;

    //Speed of NPC
    [SerializeField] private float curSpeed = 150.0f;

    //Rotation Speed
    [SerializeField] private float curRotationSpeed = 2.0f;

    //bullet Prefab
    public GameObject bullet;

    //Whether the NPC is Destroyed or not 
    private bool bDead = false;
    [SerializeField]private int health = 100;

    //We overwrite the deprecated build in rigidbody 
    //variable
    new private Rigidbody rigidbody;

    //Player Transform
    protected Transform playerTransform;

    //Next destination position of the NPC 
    protected Vector3 destPos;

    //List of points for patrolling 
    protected GameObject[] patrollPointList;

    //Shooting rate
    [SerializeField] protected float shootingRate = 3.0f;
    protected float elapsedTime = 0.0f;
    public float maxFireAimError = 0.001f;

    //Status Radius
    public float patrollingRadius = 100.0f;
    public float attackingRadius = 200.0f;
    public float playerNearRadius = 300.0f;

    //Tank Turrent
    public Transform turrent;
    public Transform shootPoint;


    NavMeshAgent navMeshAgent;


    //Initialize the Finite state machine for the NPC 
    protected override void Initialize()
    {
        //Get the list of Patroll points
        patrollPointList = GameObject.FindGameObjectsWithTag("WanderPoint");

        //Set Random destination point first
        FindNextPoint();

        //get the target enemy(Player)
        GameObject objPlayer = GameObject.FindGameObjectWithTag("Player");

        //Get the rigidbody
        rigidbody = GetComponent<Rigidbody>();
        playerTransform = objPlayer.transform;

        navMeshAgent = GetComponent<NavMeshAgent>();

        if (!playerTransform)
        {
            print("Player doesn't exist. Plaese add one with Tag named 'Player' ");
        }


    }

    protected override void FSMUpdate()
    {
        switch (curentState)
        {
            case FSMState.Patrol:
                UpdatePatrolState();
                break;

            case FSMState.Chase:
                UpdateChaseState();
                break;

            case FSMState.Attack:
                UpdateAttackState();
                break;

            case FSMState.Dead:
                UpdateDeadState();
                break;
        }

        //Update the Time
        elapsedTime += Time.deltaTime;

        //Go to dead state if no health left
        if(health <= 0)
        {
            curentState = FSMState.Dead;
        }
    }


    protected void FindNextPoint()
    {
        print("Finding next point");
        int rndIndex = Random.Range(0, patrollPointList.Length);
        float rndRadius = 10.0f;
        Vector3 rndPosition = Vector3.zero;
        destPos = patrollPointList[rndIndex].transform.position + rndPosition;

        //Check Range to decide the random point as the same 
        //as before
        if (IsInCurrentRange(destPos))
        {
            rndPosition = new Vector3(Random.Range(-rndRadius, rndRadius), 0.0f , Random.Range(-rndRadius, rndRadius));
            destPos = patrollPointList[rndIndex].transform.position + rndPosition;

        }

    }

    protected bool IsInCurrentRange(Vector3 pos)
    {
        float xPos = Mathf.Abs(pos.x - transform.position.x);
        float zPos = Mathf.Abs(pos.z - transform.position.z);

        if (xPos <= 50 && zPos <= 50) return true;

        return false;
    }

    protected void UpdatePatrolState()
    {
        if(Vector3.Distance(transform.position , destPos) <= patrollingRadius)
        {
            print("Reached to the destination point\n calculating the next point");

            FindNextPoint();
        }else if(Vector3.Distance(transform.position , playerTransform.position) <= playerNearRadius)
        {
            print("Switch to chase Position");
            curentState = FSMState.Chase;
        }

        //Rotate to target point 
        Quaternion targetRotation = Quaternion.LookRotation(destPos - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * curRotationSpeed);

        //Go Forward
       // transform.Translate(Vector3.forward * Time.deltaTime * curSpeed);

        navMeshAgent.SetDestination(destPos);

    }

    protected void UpdateChaseState()
    {
        //Set the target position as the player position
        destPos = playerTransform.position;

        //Check the distance with player tank When
        // the distane is near , transform to atteck state 
        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if(distance <= attackingRadius)
        {
            curentState = FSMState.Attack;
        } else if( distance >= playerNearRadius)
        {
            curentState = FSMState.Patrol;
        }

        //transform.Translate(Vector3.forward * Time.deltaTime * curSpeed);

        navMeshAgent.SetDestination(playerTransform.position);
    }

    protected void UpdateAttackState()
    {
        destPos = playerTransform.position;

        Vector3 frontVector = Vector3.forward;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        if(distance >= attackingRadius && distance < playerNearRadius)
        {
            Quaternion turrentRotation = Quaternion.FromToRotation(destPos , transform.position); //<--(destPos - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, turrentRotation, Time.deltaTime * curRotationSpeed);
            transform.Translate(Vector3.forward * Time.deltaTime * Time.deltaTime * curSpeed);

            curentState = FSMState.Attack;

        } else if(distance >= playerNearRadius)
        {
            curentState = FSMState.Patrol;
        }

        //Rotate the turrent to the target point 
        // The rotation is only around the verticall axis of the tank.
        Quaternion targetRotation = Quaternion.FromToRotation(frontVector, destPos - transform.position);
        turrent.rotation = Quaternion.Slerp(turrent.rotation, targetRotation, Time.deltaTime * curRotationSpeed);

        navMeshAgent.SetDestination(playerTransform.position);

        //Shoot the bullets
        if (Mathf.Abs(Quaternion.Dot(targetRotation, turrent.rotation)) > 1.0f - maxFireAimError)
        {
            ShootBullet();
        }

    }

    private void OnCollisionEnter(Collision collision)
    {
        //Reduce health
        if(collision.gameObject.tag == "Bullet")
        {
           // health -= collision.gameObject.GetComponent<Bullet>().damage;
        }
    }

    private void ShootBullet()
    {
        if(elapsedTime >= shootingRate)
        {
            Instantiate(bullet, shootPoint.position, shootPoint.rotation);
            elapsedTime = 0.0f;
        }
    }

    protected void UpdateDeadState()
    {
        //Show the dead animation with some physics effect
        if (!bDead)
        {
            bDead = true;
            Explode();
        }
    }

    protected void Explode()
    {
        float rndX = Random.Range(10.0f, 30.0f);
        float rndZ = Random.Range(10.0f, 30.0f);
        for(int i = 0;  i<3; i++)
        {
            rigidbody.AddExplosionForce(10000.0f, transform.position - new Vector3(rndX, 10.0f, rndZ), 40.0f, 10.0f);
            rigidbody.velocity =transform.TransformDirection(new Vector3 (rndX, 20.0f, rndZ));
        }

        Destroy(gameObject, 1.5f);

    }

    
}
