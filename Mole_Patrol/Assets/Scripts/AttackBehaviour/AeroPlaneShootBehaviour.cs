using Unity.Netcode;
using UnityEngine;

public class AeroPlaneShootBehaviour : MonoBehaviour, IAttackBehaviour
{
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;

    public GameObject firingSmoke;

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
            Instantiate(firingSmoke, firePoint.transform.position, Quaternion.identity, transform);
        }
    }
}