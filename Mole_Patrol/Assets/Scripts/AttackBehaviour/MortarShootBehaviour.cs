using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MortarBallShootBehaviour : MonoBehaviour, IAttackBehaviour
{
    public GameObject projectilePrefab;
    public float projectileSpeed = 2.5f;

    private float lastFireTime = -Mathf.Infinity;

    public void Attack(Transform target, Transform firePoint, float fireRate, float damage)
    {
        if (Time.time >= lastFireTime + fireRate) 
        {

            GameObject[] triangles = GameObject.FindGameObjectsWithTag("Triangle");
            float closestDistance = 100000;
            Transform closestCentroid = null;
            for(int i =0; i < triangles.Length; i++)
            {
                Transform centroid = triangles[i].transform.Find("centroid");
                float distance = Vector3.Distance(target.position, centroid.transform.position);
                if(distance < closestDistance)
                {
                    closestDistance = distance;
                    closestCentroid = centroid;
                }
            }

            lastFireTime = Time.time;
            GameObject planet = GameObject.Find("Planet");
            GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity, planet.transform);
            if(closestCentroid != null)
            {
                projectile.GetComponent<Projectile>().Initialize(closestCentroid.transform, projectileSpeed, damage);
                projectile.GetComponent<NetworkObject>().Spawn();
            }
            GameManager.Instance.UpdateProjectilesFired();
        }
    }
}
