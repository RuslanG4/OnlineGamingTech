using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MortarProjectile : Projectile
{
    public float aoeRadius = 1f;
    public GameObject explosion;

    [Header("Flight Parameters")]
    public float initialAscentTime = 0.5f; // Time to spend in initial upward flight
    public float ascentHeight = 10f; // How high to rise during initial ascent

    private Vector3 initialPosition;
    private float elapsedTime = 0f;
    private bool hasLaunched = false;
    private bool isAscending = true; // Tracks if we're in the initial ascent phase

    protected override void Start()
    {
        base.Start();
    }

    public override void Initialize(Transform target, float speed, float damage)
    {
        this.target = target;
        this.speed = speed;
        this.damage = damage;
        initialPosition = transform.position;
        hasLaunched = true;
    }

    void Update()
    {
        if (!IsServer) return;
        if (!hasLaunched)
            return;

        if (target == null)
        {
            GetComponent<NetworkObject>().Despawn();
            return;
        }

        elapsedTime += Time.deltaTime;

        if (isAscending)
        {
            // Initial ascent phase
            UpdateAscent();
        }
        else
        {
            // Targeting phase
            UpdateTargeting();
        }
    }

    void UpdateAscent()
    {
        // Calculate local up direction (away from planet center)
        Vector3 localUp = (transform.position - planetCenter).normalized;

        // Move upward
        float ascentProgress = Mathf.Clamp01(elapsedTime / initialAscentTime);
        Vector3 moveVector = localUp * ascentHeight * Time.deltaTime / initialAscentTime;

        // Apply movement
        transform.position += moveVector;
        transform.LookAt(transform.position + moveVector);

        // Check if ascent phase is complete
        if (ascentProgress >= 1.0f)
        {
            isAscending = false;
            elapsedTime = 0f; // Reset timer for targeting phase
        }
    }

    void UpdateTargeting()
    {
        // Similar to CanonBallProjectile's approach
        Vector3 direction = (target.position - transform.position).normalized;
        Vector3 gravity = (planetCenter - transform.position).normalized * gravityEffect;
        Vector3 moveVector = (direction + gravity).normalized * speed * Time.deltaTime;

        // Apply movement
        transform.position += moveVector;
        transform.LookAt(transform.position + moveVector);
    }

    void OnImpact(GameObject coll)
    {
        Vector3 normal = (transform.position - coll.transform.position).normalized;
        Quaternion rotation = Quaternion.FromToRotation(Vector3.forward, normal);

        // Create explosion
        GameObject planet = GameObject.Find("Planet");
        ParticleRPCRelay.Instance.PlayExplosionParticleClientRpc(transform.position);

        

        // Apply AOE damage
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, aoeRadius);
        foreach (Collider col in hitColliders)
        {
            if (col.CompareTag("Enemy"))
            {
                Enemy enemy = col.GetComponent<Enemy>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }
            }
        }

        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;
        if (other.CompareTag("Enemy") || other.CompareTag("Triangle"))
        {
            OnImpact(other.gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aoeRadius);
    }
}