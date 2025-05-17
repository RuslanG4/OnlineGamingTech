using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CannonBallShootBehaviour : MonoBehaviour, IAttackBehaviour
{
    public GameObject projectilePrefab;
    public GameObject firingSmoke;

    public float projectileSpeed = 5f;

    private float lastFireTime = -Mathf.Infinity;

    public void Attack(Transform target, Transform firePoint, float fireRate, float damage)
    {
        if (Time.time >= lastFireTime + fireRate) 
        {
            lastFireTime = Time.time;
            GameObject planet = GameObject.Find("Planet");
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity, planet.transform);
            projectile.GetComponent<Projectile>().Initialize(target, projectileSpeed, damage);
            projectile.GetComponent<NetworkObject>().Spawn();
            GameManager.Instance.UpdateProjectilesFired();
            //Instantiate(firingSmoke, firePoint.transform.position, Quaternion.identity, transform);
        }
    }


}
