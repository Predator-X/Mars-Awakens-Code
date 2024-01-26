using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using static SimpleFSM;

public class AlienFSM : FSM 
{
    //Health
    public float maxHealthThreshold = 100f;
    public float currentHealth = 100;
    //Dead effect
    public bool enemyIsHitOnHead = false , isDead=false;

    //Current state that the NPC is reaching
    public FSMState curentState = FSMState.Patrol;

    //For Patrolling 
    protected GameObject[] patrollPointList;
    protected GameObject desitinationTarget;

    //For Targeting Player
    public NavMeshAgent agent;
    public GameObject player;
    public GameObject[] players;
    [SerializeField] private LayerMask obstacleLayer;

    //Status Radius
    public float patrollingRadius = 50.0f;
    public float attackingRadius = 15.0f , chaseRadious = 25f, goPunchRadius = 5f, punch = 2f;
    public float playerNearRadius = 30.0f;

    //for Shooting
    public GameObject bullet;
    public ShootWithRaycast attack;
    public bool alreadyAttacked;
    public float timeBetweenAttacks=3.0f;

    //Shooting rate
    [SerializeField] protected float shootingRate = 3.0f;
    protected float elapsedTime = 0.0f;
    public float maxFireAimError = 0.001f;

    public enum FSMState
    {
        None, Patrol, Chase, Attack,GoAfterTarget,MoveBack,Punch,Hide, Dead,
    }

    protected override void FSMUpdate()
    {
        if (player == null) { player = GameObject.FindGameObjectWithTag("Player"); }

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

            case FSMState.GoAfterTarget:
                UpdateGoAfterState();
                break;

            case FSMState.MoveBack:
                UpdateMoveBackState();
                break;

            case FSMState.Punch:
                UpdatePunchState();
                break;

            case FSMState.Hide:
                UpdateHideState();
                break;

            case FSMState.Dead:
                UpdateDeadState();
                break;
        }
    }

    private void UpdateMoveBackState()
    {
        throw new NotImplementedException();
    }

    private void UpdateHideState()
    {
        throw new NotImplementedException();
    }

    private void UpdatePunchState()
    {
        throw new NotImplementedException();
    }

   

    protected override void Initialize()
    {
        agent = GetComponent<NavMeshAgent>();

        //chose attack method
        attack = GetComponent<ShootWithRaycast>(); 

        //Find patroll points in scene
        patrollPointList = GameObject.FindGameObjectsWithTag("WanderPoint");

        //Set random destination point first
        PickPatrollPoint();

        //Find Player in Scene
        player = GameObject.FindGameObjectWithTag("Player");
    }
    protected void UpdatePatrolState()
    {
        if(Vector3.Distance(transform.position , desitinationTarget.transform.position)<= agent.stoppingDistance)
        {
            //Reached to the destination point, calculating the next point
            PickPatrollPoint();
        }
        else if(Vector3.Distance(transform.position , player.transform.position)<= playerNearRadius)
        {
            //Switch to chase Position
            curentState = FSMState.Chase;
        }
        //Move to Patroll point
        agent.SetDestination(desitinationTarget.transform.position);
    }

    private void UpdateChaseState()
    {
       
        if (player != null)
        {
            //if in attack range change to attack state
            if(Vector3.Distance(transform.position , player.transform.position)<= attackingRadius && 
                !CheckIsSomethingInBetweenTargetAndNPC())
            {
                curentState = FSMState.Attack;
            }
            else if(Vector3.Distance(transform.position , player.transform.position)> playerNearRadius)
            {
                //If Lost the Player go to patrolling state 
                curentState = FSMState.Patrol;
            }

            // chase the player
            agent.SetDestination(player.transform.position);
        }
        else
        {
            player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                //Go to patroll state as no player could be found
                Debug.LogWarning("No player found in ChaseState so go Patroll");
                curentState = FSMState.Patrol;
            }
        }
        
    }

    private void UpdateAttackState()
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);
        if (distance >= chaseRadious)
        {
            curentState = FSMState.Chase;
        }
        else if(distance <=attackingRadius && CheckIsSomethingInBetweenTargetAndNPC())//distance<= goPunchRadius || CheckIsSomethingInBetweenTargetAndNPC()) 
        {
            curentState = FSMState.GoAfterTarget;
        }
        //Stop agent when on Attack range
        agent.SetDestination(transform.position);
        transform.LookAt(player.transform.position);
        //CheckIsSomethingInBetweenTargetAndNPC();
        StartShooting();

    }

    private void UpdateGoAfterState()
    {
        float distance = Vector3.Distance(transform.position, player.transform.position);
       // CheckIsSomethingInBetweenTargetAndNPC();
        if (distance <= goPunchRadius && !CheckIsSomethingInBetweenTargetAndNPC())
        {
            curentState = FSMState.Chase;
        }
        /*
         * if (distance > goPunchRadius)
           {
               curentState = FSMState.Chase;
           }

           if(Vector3.Distance(this.transform.position , player.transform.position) <= punch)
           {
               //Punch Code HERE
               agent.SetDestination(this.transform.position);//<--Stop agent when on Attack range
               transform.LookAt(player.transform.position);
               Debug.Log("Punch!!");
           }
        */

        agent.SetDestination(player.transform.position);

    }



    protected bool CheckIsSomethingInBetweenTargetAndNPC()
    {
        RaycastHit hit; 
        
        float distance = Vector3.Distance(this.transform.position, player.transform.position);
        if (Physics.Raycast(attack.gunEnd.transform.position, transform.forward, out hit,
            distance , obstacleLayer))
        {
            if(hit.collider != null && !hit.collider.CompareTag("Player"))
            {
                curentState = FSMState.GoAfterTarget;
                Debug.Log("Obstacle  In The way !!");
                return true;
            }
            else
            {
                return false;
            }
        }
        else { return false; }

    }

    protected void StartShooting()
    {
        if (!alreadyAttacked)
        {
            //Attack code 
            Transform pointTheGunAt = transform.Find("GunHolder");
            pointTheGunAt.transform.LookAt(player.transform);
            CheckIsSomethingInBetweenTargetAndNPC();//<--Check that there is no obstacle in the way
            attack.Shoot();
            alreadyAttacked = true;

            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void UpdateDeadState()
    {
        throw new NotImplementedException();
    }


    public void ResetAttack()
    {
        alreadyAttacked = false;
    }


    protected void PickPatrollPoint()
    {
        desitinationTarget =patrollPointList[ UnityEngine.Random.Range(0, patrollPointList.Length) ];
    }

    public void Damage(int damageAmount)
    {
        currentHealth -= damageAmount;


        if (currentHealth <= 0 && enemyIsHitOnHead)
        {
            GameObject body, gun, head;
            body = this.transform.Find("Body").gameObject;
            body.AddComponent<DisActivateAfter>().enabled = true;
            body.AddComponent<Rigidbody>();
            body.transform.parent = null;


            gun = this.transform.Find("GunHolder").gameObject;
            gun.AddComponent<DisActivateAfter>().enabled = true;
            gun.AddComponent<Rigidbody>();
            gun.transform.parent = null;


            head = this.transform.Find("HeadShooted").gameObject;
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
}
