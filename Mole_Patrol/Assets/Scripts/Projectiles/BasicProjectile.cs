using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicProjectile : Projectile
{
    protected override void Start()
    {
        base.Start();
    }

    public override void Initialize(Transform target, float speed, float damage)
    {
        this.target = target;
        this.speed = speed;
        this.damage = damage;
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        UpdateProjectile();
    }

    void UpdateProjectile()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Vector3 gravity = (planetCenter - transform.position).normalized * gravityEffect;

        Vector3 moveVector = (direction + gravity).normalized * speed * Time.deltaTime;
        transform.position += moveVector;

        transform.LookAt(transform.position + moveVector);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            ParticleManager.Instance.PlayHitParticle(gameObject.transform.position);
            Destroy(gameObject);
        }
    }
}
