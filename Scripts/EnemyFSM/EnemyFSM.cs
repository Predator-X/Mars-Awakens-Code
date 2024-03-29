using System.Collections;
using System.Collections.Generic;
//using Unity.IO.LowLevel.Unsafe;
//using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
//using UnityEngine.InputSystem.Android;

public class EnemyFSM : Character
{
    //For this sc Requires Tags( HideSpot , WanderPoint , Player) + LayerMasks( " Obstacles")
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float rotationSpeed = 7f;
    private NavMeshAgent navMeshAgent;

    public float chaseDistance = 10.0f;
    public float attackDistance = 3.0f;
    public float lowHealthThreshold = 50.0f;
    private IEnumerator healthCorutine;
    public float hideTime = 5.0f;
    public LayerMask obstacleMask;

    public GameObject bulletPrefab;
    public Transform gunPoint;
    [SerializeField] float bulletSpeed = 80f;
    public float shootingInterval = 0.5f;

    public Coroutine shootingCoroutine;

    // [SerializeField] private float currentHealth;<--As Inherits
    public Transform playerTransform;
    private int currentHideSpotIndex;
    public bool isHiding;
    public bool isChasing;
    public bool isAttacking;

    //it suppost to be generated by EyesightScript;
    public GameObject lastSeenSpot = null;
    public float lastSeenSpotTimer = 15f;

    [SerializeField] private List<GameObject> hideSpots;
    [SerializeField] private List<GameObject> wanderPoints;
    private GameObject[] patrollPoints;
    [SerializeField] private float wanderRadius = 10f;
    [SerializeField] private float minWanderTime = 1f;
    [SerializeField] private float maxWanderTime = 5f;
    [SerializeField] private Vector3 maxGunRotationOnUse = new Vector3(40, 40, 40);

    private Quaternion getOrginalGunsTransform;

    [SerializeField] float smooth = 5.0f;
    [SerializeField] float tiltAngle = 40.0f;

    private Vector3 wanderTarget;
    private float wanderTimer;
    private bool enemyIsHitOnHead;
   
    

    private void Start()
    {
        currentHealth = 100.0f;
        getOrginalGunsTransform = gun.transform.parent.transform.localRotation; 
        currentHideSpotIndex = -1;
        isHiding = false;
        isChasing = false;
        isAttacking = false;

        navMeshAgent = this.gameObject.GetComponent<NavMeshAgent>();

        if (wanderPoints.Count == 0)
        {
            ArrayToList(GameObject.FindGameObjectsWithTag("WanderPoint"), wanderPoints);  //FindWanderPoints();
        }

        if (hideSpots.Count == 0)
        {
            SearchForHideSpots();
        }
        //playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void SearchForHideSpots()
    {
        GameObject[] CoverGameObjects = GameObject.FindGameObjectsWithTag("Cover");

        for(int i=0; i< CoverGameObjects.Length; i++)
        {
            if (CoverGameObjects[i].transform.childCount != 0)
            {
                for (int j = 0; j < CoverGameObjects[i].transform.childCount; j++)
                {
                    if (CoverGameObjects[i].transform.GetChild(j).CompareTag("HideSpot"))
                    {
                        hideSpots.Add((CoverGameObjects[i].transform.GetChild(j).gameObject));
                    }
                }
            }
            
        }
    }

    private void ArrayToList(GameObject[] find, List<GameObject> returnToList)
    {

        for (int i = 0; i < find.Length; i++)
        {
            returnToList.Add(find[i].gameObject);
        }

    }

    private void Update()
    {
        if (currentHealth <= lowHealthThreshold)
        {
            Hide();
        }
        if(playerTransform == null) { Wander(); }
        else
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            RaycastHit hit;
            bool isObstacleBetween = Physics.Raycast(transform.position, playerTransform.position - transform.position, out hit, distanceToPlayer, obstacleMask);

            if (distanceToPlayer <= chaseDistance && !isObstacleBetween)
            {
                if (!isHiding)
                {
                    if (distanceToPlayer <= attackDistance)
                    {
                        Attack();
                    }
                    else
                    {
                        Chase();
                        ResetGunRotation();
                    }
                }
            }
            else
            {
                if (!isHiding && !isChasing && !isAttacking)
                {
                    Wander();
                    ResetGunRotation();
                }
              
            }
        }
        
    }

    public void Chase()
    {
        isChasing = true;
        isAttacking = false;
        // Stop any previous shooting coroutine
        if (shootingCoroutine != null)
        {
            StopCoroutine(shootingCoroutine);
        }

        // Set the destination of the NavMeshAgent to the player's position
        navMeshAgent.SetDestination(playerTransform.position);
        

        // Rotate towards the move direction
        Vector3 moveDirection = (playerTransform.position - transform.position).normalized;
        if (moveDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDirection), rotationSpeed * Time.deltaTime);
        }
        float dist = navMeshAgent.remainingDistance;
        if (dist != Mathf.Infinity && navMeshAgent.pathStatus == NavMeshPathStatus.PathComplete && navMeshAgent.remainingDistance == 0)
        {
            if (!playerTransform.gameObject.CompareTag("Player"))
            {
                Destroy(playerTransform.gameObject, 1);
                playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
            }
            isChasing = false;
            isAttacking = false;
            isHiding = false;
            Wander();
        }
    }

    public void Attack()
    {
        if (!playerTransform.gameObject.CompareTag("Player"))
        {
            Destroy(playerTransform.gameObject, 1);
            if(GameObject.FindGameObjectWithTag("Player").transform !=null)
            {
                playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
            }
           
            isChasing = false;
            isAttacking = false;
        }
        else
        {
            isChasing = false;
            isAttacking = true;

            // Stop any previous shooting coroutine
            if (shootingCoroutine != null)
            {
                StopCoroutine(shootingCoroutine);
            }


            PointGunAtTarget(playerTransform);
            shootingCoroutine = StartCoroutine(ShootCoroutine());

        }



    }

    public void PointGunAtTargetss(Transform target)
    {
        // Calculate the direction from the gun to the target
        Vector3 direction = target.position - gun.transform.parent.transform.position;

        // Calculate the rotation to point the gun in the direction of the target
        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

        // Limit the rotation to the maximum angle
        float angle = Quaternion.Angle(gun.transform.parent.transform.rotation, targetRotation);
        if (angle <= maxGunRotationOnUse.y && angle>= -maxGunRotationOnUse.y)
        {
            targetRotation = Quaternion.RotateTowards(gun.transform.parent.rotation, targetRotation, maxGunRotationOnUse.y);
            gun.transform.parent.transform.rotation = targetRotation;
        }
        else
        {
            // Restrict the rotation of the gun within the angle range
            float clampedRotationY = Mathf.Clamp(angle, -maxGunRotationOnUse.y, maxGunRotationOnUse.y);
            Vector3 newRotation = new Vector3(gun.transform.parent.transform.rotation.eulerAngles.x, clampedRotationY, gun.transform.rotation.eulerAngles.z);
            gun.transform.parent.transform.rotation = Quaternion.Euler(newRotation);
        }

        // Set the rotation of the gun
       // gun.transform.parent.transform.rotation = targetRotation;
    }

    public void ResetGunRotation()
    {
        // Set the rotation of the gun back to its standard rotation
        gun.transform.parent.transform.localRotation = getOrginalGunsTransform;
    }


    public void PointGunAtTarget(Transform target)
    {
        // Cache frequently used variables
        Transform parentTransform = gun.transform.parent.transform;
        Quaternion parentRotation = gun.transform.parent.rotation;

        // Calculate the direction from the gun to the target
        Vector3 direction = target.position - parentTransform.position;

        // Calculate the rotation to point the gun in the direction of the target
        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

        // Check if the angle is within the maximum range
        float angle = Quaternion.Angle(parentRotation, targetRotation);
        if (IsWithinMaxRotationAngle(angle))
        {
            targetRotation = Quaternion.RotateTowards(parentRotation, targetRotation, maxGunRotationOnUse.y);
        }
        else
        {
            targetRotation = ClampRotation(targetRotation, maxGunRotationOnUse.y);
        }

        // Set the rotation of the gun's parent transform
        parentTransform.rotation = targetRotation;
    }

    private bool IsWithinMaxRotationAngle(float angle)
    {
        return angle <= maxGunRotationOnUse.y && angle >= -maxGunRotationOnUse.y;
    }

    private Quaternion ClampRotation(Quaternion targetRotation, float maxAngle)
    {
        float angle = Quaternion.Angle(gun.transform.parent.rotation, targetRotation);
        float clampedAngle = Mathf.Clamp(angle, -maxAngle, maxAngle);
        Quaternion clampedRotation = Quaternion.RotateTowards(gun.transform.parent.rotation, targetRotation, clampedAngle);
        return clampedRotation;
    }

    void ResetGunsTransfom(Transform objectToRotate, Transform objectTarget)
    {


        // Smoothly tilts a transform towards a target rotation.
        float tiltAroundY = objectTarget.transform.rotation.y * tiltAngle;
        float tiltAroundX = objectTarget.transform.rotation.x * tiltAngle;

        // Rotate the cube by converting the angles into a quaternion.
        Quaternion target = Quaternion.Euler(tiltAroundX, tiltAroundY ,0);

        // Dampen towards the target rotation
        objectToRotate.rotation = Quaternion.Slerp(objectTarget.rotation, target, Time.deltaTime * smooth);
    }

    private IEnumerator ShootCoroutine()
    {
        while (true)
        {
            // Instantiate the bullet prefab at the gun point's position and rotation
            GameObject bulletObject = Instantiate(bulletPrefab, gunPoint.position, gunPoint.rotation);

            // Add a rigidbody to the bullet object
            Rigidbody bulletRigidbody = bulletObject.GetComponent<Rigidbody>(); //bulletObject.AddComponent<Rigidbody>();

            // Set the bullet's velocity to the forward direction of the enemy multiplied by the bullet speed
            bulletRigidbody.velocity = transform.forward * bulletSpeed;
            if (bulletPrefab != null)
            {
                // find ammo or run must implement 
                Debug.LogWarning("bullet Prefab Is Missing on " + this.gameObject.name);
                isAttacking = false;
                // isHiding = true;
            }
            yield return new WaitForSeconds(shootingInterval);
        }
    }


    private void Hide()
    {
        isHiding = true;
        isChasing = false;
        isAttacking = false;

      

        // Find the closest hiding spot
        GameObject closestSpot = null;
        float closestDistance = Mathf.Infinity;
       
        foreach (GameObject spot in hideSpots)
        {
            float distance = Vector3.Distance(transform.position, spot.transform.position);
            if (distance < closestDistance)
            {
                closestSpot = spot;
                closestDistance = distance;

            }
        }

        // Set the destination of the NavMeshAgent to the hiding spot's position
        navMeshAgent.SetDestination(closestSpot.transform.position);

        // Rotate towards the move direction
        Vector3 moveDirection = navMeshAgent.velocity.normalized;
        if (moveDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDirection), rotationSpeed * Time.deltaTime);
        }

        float dist = navMeshAgent.remainingDistance;
        if (dist <= navMeshAgent.stoppingDistance)
        {
            StartCoroutine(StartHealByRate(1f, 5));


        }
        if (dist != Mathf.Infinity && navMeshAgent.pathStatus == NavMeshPathStatus.PathComplete && navMeshAgent.remainingDistance == 0)
        {
         
        }
    }

    private void Unhide()
    {
        isHiding = false;
      
            ShootFromHiding(false);
        
        // Pick a new hide spot
        currentHideSpotIndex = Random.Range(0, hideSpots.Count);
    }

    private IEnumerator UnhideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Unhide();
    }

    private IEnumerator StartHealByRate(float everyTimeAmount, int healAmount)
    {
        while (true)
        {
            yield return new WaitForSeconds(everyTimeAmount);
            Heal(healAmount);
            if(currentHealth>= maxHealthThreshold)
            {
                Unhide();
                isHiding = false;
                break;
            }
        }
    }

    private void Heal(int healAmount)
    {
        if (currentHealth < maxHealthThreshold)
            currentHealth += healAmount;
        else Debug.Log("Max health reached health addition scipped!");
    }

    private void Wander()
    {
        // Check if the enemy has reached the wander target or the timer has expired
        if (Vector3.Distance(transform.position, navMeshAgent.destination) < 1f || navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
        {
            // Select a new random destination within the wander radius
            Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
            Vector3 randomOffset = new Vector3(randomCircle.x, 0f, randomCircle.y);
            Vector3 randomPoint = transform.position + randomOffset;

            // Clamp the destination within the boundaries of the scene
            randomPoint.x = Mathf.Clamp(randomPoint.x, -40f, 40f);
            randomPoint.z = Mathf.Clamp(randomPoint.z, -40f, 40f);

            // Set the destination of the NavMeshAgent to the new point
            navMeshAgent.SetDestination(randomPoint);

            // Reset the timer to a new random duration
            wanderTimer = Random.Range(minWanderTime, maxWanderTime);
        }

        // Rotate towards the move direction
        Vector3 moveDirection = navMeshAgent.velocity.normalized;
        if (moveDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDirection), rotationSpeed * Time.deltaTime);
        }

        // Decrement the wander timer
        wanderTimer -= Time.deltaTime;
    }

    
    public override void Damage(float damageAmount)
    {
        currentHealth -= damageAmount;

        if (currentHealth <= 0.0f)
        {
            Die();
        }


        if (currentHealth <= 0 && enemyIsHitOnHead)
        {
            GameObject body, gun, head;
            body = this.transform.Find("Body").gameObject;
            body.GetComponent<DisActivateAfter>().enabled = true;
            body.AddComponent<Rigidbody>();
            body.transform.parent = null;


            gun = this.transform.Find("GunHolder").gameObject;
            gun.GetComponent<DisActivateAfter>().enabled = true;
            gun.AddComponent<Rigidbody>();
            gun.transform.parent = null;


            head = this.transform.Find("HeadShooted").gameObject;
            head.GetComponent<DisActivateAfter>().enabled = true;
            head.AddComponent<Rigidbody>();
            head.transform.parent = null;

            isDead = true;
            // gameObject.SetActive(false);
            Die();


        }
        else if (currentHealth <= 0 && enemyIsHitOnHead == false)
        {
            isDead = true;
            Die();// gameObject.SetActive(false);
        }
    }
    public virtual void EnemyIsHitOnHead(bool gotHit)
    {
        enemyIsHitOnHead = gotHit;
    }
    private void Die()
    {
        Destroy(this.gameObject);
        // Implement death behavior
        // For example, play death animation and destroy game object
    }

    public virtual void IsSeenAt(GameObject seenPointAdd)
    {
        if (lastSeenSpot == null)
        {
            lastSeenSpot = seenPointAdd;
        }
        else
        {
            Destroy(lastSeenSpot);
            lastSeenSpot = seenPointAdd;
        }
    }

    public virtual void ShootFromHiding(bool isShootingFromHiding)
    {
        Transform s1, s2;
        s1 = this.transform;
        s2 = s1;
        s1.transform.localPosition = head.transform.localPosition;
        s2.transform.localPosition = gun.transform.localPosition;

        float yCalculate = head.transform.position.y *5/100;
        float yGunCalculate = gun.transform.position.y * 40 / 100;
         head.transform.position = new Vector3(head.transform.position.x, head.transform.position.y - yCalculate , head.transform.position.z);
        gun.transform.position = new Vector3(gun.transform.position.x, gun.transform.position.y, gun.transform.position.z + yGunCalculate);
       // head.transform.localPosition = s2.transform.localPosition;
         //   gun.transform.localPosition = s1.transform.localPosition;

       


    }
}

/*
 
private void Wander()
    {
        // Check if the enemy has reached the wander target or the timer has expired
        if (Vector3.Distance(transform.position, wanderTarget) < 1f || wanderTimer <= 0f)
        {
            // Select a new random destination within the wander radius
            Vector2 randomCircle = Random.insideUnitCircle * wanderRadius;
            Vector3 randomOffset = new Vector3(randomCircle.x, 0f, randomCircle.y);
            wanderTarget = transform.position + randomOffset;

            // Clamp the destination within the boundaries of the scene
            wanderTarget.x = Mathf.Clamp(wanderTarget.x, -40f, 40f);
            wanderTarget.z = Mathf.Clamp(wanderTarget.z, -40f, 40f);

            // Reset the timer to a new random duration
            wanderTimer = Random.Range(minWanderTime, maxWanderTime);
        }

        // Move towards the wander target
        Vector3 moveDirection = (wanderTarget - transform.position).normalized;
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        // Rotate towards the move direction
        if (moveDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDirection), rotationSpeed * Time.deltaTime);
        }

        // Decrement the wander timer
        wanderTimer -= Time.deltaTime;
    }
*/