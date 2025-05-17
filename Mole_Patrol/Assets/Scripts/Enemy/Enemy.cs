using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.UIElements;
using static JSONEnemyInformation;

public enum AnimationState { Idle, Walking, Attacking, Dead  }

// Placeholder Enemy Class
public class Enemy : NetworkBehaviour
{
  
    [HideInInspector] public int damage = 10;
    [HideInInspector] public float speed = 1.25f;
    public GameObject planet;
 

    public AnimationState animationStat = AnimationState.Idle;
    public Animator animator;
    public GameObject target;


    protected Rigidbody rb;
    [HideInInspector]  protected float health = 15f;

    public bool attacking = false;
    protected bool grounded = false;
    protected int KillGoldGain;
    public float spawnTimeWait;

    protected bool isReadyToUpdate = false;

    CapsuleCollider capsuleCollider;

    public void Initialise(EnemyStats _stats)
    {
        damage = _stats.maxDamage;
        speed = _stats.maxSpeed;
        health = _stats.maxHealth;
        KillGoldGain = _stats.goldGainOnDeath;
        spawnTimeWait = _stats.spawnInterval;

        animator.SetFloat("SpeedMultiplier", speed);
    }
    private void Awake()
    {
        rb = gameObject.GetComponent<Rigidbody>();
        capsuleCollider = gameObject.GetComponent<CapsuleCollider>();

        StartCoroutine(DelayedStart(0.15f));
    }

    private IEnumerator DelayedStart(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        isReadyToUpdate = true;
    }

    private void Update()
    {
        if (!IsServer) return;
        if (health <= 0 && isReadyToUpdate)
        {
            // need to find different way as this will cause enemies to fall through planet
            //capsuleCollider.enabled = false;
            GameManager.Instance.UpdateEnemyKilled();
            GameManager.Instance.UpdateCoinCount(KillGoldGain);
            //animationStat = AnimationState.Dead;



            ParticleRPCRelay.Instance.PlayDeathParticleClientRpc(transform.position);
            GetComponent<NetworkObject>().Despawn();
        }
    }

    public virtual void attackTarget()
    {
       if (attacking && target != null)
        {
            target.GetComponent<Tower>().TakeDamage(damage);
        }
    }

 
    protected void findClosestTarget()
    {
        Tower closestTower = null;
        Tower[] possibleTargets = FindObjectsOfType<Tower>();
        float closestDistance = Mathf.Infinity; // Start with a large number
            
        foreach (Tower newTarget in possibleTargets)
        {
            if (!newTarget.placed.Value || !newTarget.targetable.Value)
            {
                continue;
            }
            
            float distance = Vector3.Distance(transform.position, newTarget.transform.position);

            if( distance < closestDistance )
            {
                closestTower = newTarget;
                closestDistance = distance; 
            }
        }
        
        
        if( closestTower != null )
        {
            target = closestTower.gameObject;
        }
        else
        {
            target = GameObject.FindObjectOfType<TownHallTower>().gameObject;
        }

    }

    protected void MoveToTarget()
    {
        Vector3 moveDirection = getPlanetRelativeTargetDirection(target.transform);

        moveDirection += seperationForce();
        Vector3 avoidanceForce = AvoidObstacles(moveDirection);
        if (avoidanceForce != Vector3.zero)
        {
            moveDirection += avoidanceForce * 0.75f;
        }

        Vector3 planetCenter = planet.transform.position;
        Vector3 currentUp = (transform.position - planetCenter).normalized; 
        moveDirection = Vector3.ProjectOnPlane(moveDirection, currentUp).normalized; 

        moveDirection += AvoidObstacles(moveDirection);

        rb.linearVelocity = moveDirection * speed;

        Vector3 targetDirection = target.transform.position - transform.position;
        targetDirection = Vector3.ProjectOnPlane(targetDirection, currentUp).normalized;
        Quaternion newRotation = Quaternion.LookRotation(targetDirection, currentUp);

        transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, 10f * Time.deltaTime);
    }

    protected Vector3 AvoidObstacles(Vector3 moveDirection)
    {
        float detectionDistance = 1.0f;
        float boxHeight = 0.55f;
        Vector3 avoidanceDirection = Vector3.zero;

        Vector3 startPosition = transform.position + transform.up * 0.5f;

        RaycastHit hit;
        if (Physics.BoxCast(startPosition, new Vector3(0.5f, boxHeight, 0.5f), moveDirection, out hit, Quaternion.identity, detectionDistance))
        {
            if (hit.collider.CompareTag("Environment"))
            {
                Vector3 obstacleNormal = hit.normal;

                Vector3 reflectedDirection = Vector3.Reflect(moveDirection, obstacleNormal);

                Vector3 planetUp = (transform.position - planet.transform.position).normalized;
                avoidanceDirection = Vector3.ProjectOnPlane(reflectedDirection, planetUp);
            }
        }

        return avoidanceDirection.normalized;
    }

    protected Vector3 seperationForce()
    {
        Vector3 separation = Vector3.zero;
        Collider[] nearbyEnemies = Physics.OverlapSphere(transform.position, 1.5f); 
        foreach (Collider col in nearbyEnemies)
        {
            if (col.CompareTag("Enemy") && col.gameObject != this.gameObject)
            {
                separation += (transform.position - col.transform.position).normalized;
            }
        }

        return separation.normalized * 0.5f;
    }

    protected Vector3 getPlanetRelativeTargetDirection(Transform target)
    {
        Vector3 gravityDir = (transform.position - planet.transform.position).normalized;

        Vector3 targetDir = (target.transform.position - transform.position).normalized;

        Vector3 planetRelativePos = Vector3.ProjectOnPlane(targetDir, gravityDir).normalized;

        Vector3 moveDirection = Vector3.Cross(gravityDir, Vector3.Cross(planetRelativePos, gravityDir)).normalized;

        return moveDirection;
    }


    public void TakeDamage(float damageNumber) => health -= damageNumber;


    protected void OnDestroy()
    {
        attacking = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        if (other.gameObject.tag == "Projectile")
        {
            TakeDamage(other.GetComponent<Projectile>().damage);
        }
    }
}

