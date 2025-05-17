using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CanonBallProjectile : Projectile
{
    public override void Initialize(Transform target, float speed, float damage)
    {
        this.target = target;
        this.speed = speed;
        this.damage = damage;
    }

    void Update()
    {
        if (!IsServer) return;

        if (target == null)
        {
            GetComponent<NetworkObject>().Despawn();
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
        if (!IsServer) return;

        if (other.CompareTag("Enemy"))
        {
            ParticleRPCRelay.Instance.PlayHitParticleClientRpc(transform.position);
            GetComponent<NetworkObject>().Despawn();
        }
    }
}
