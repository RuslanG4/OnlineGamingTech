using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AeroPlaneUnit : MonoBehaviour
{
    private AeroPlaneTower parentTower;
    private float rotationRadius = 10f;
    private float rotationSpeed = 25.0f;
    private float angle;
    private Vector3 normal;
    Vector3 initialOffset;

    public Transform firePoint;

    private IAttackBehaviour attackBehaviour;
    private TowerData towerData;

    public void Initialise(AeroPlaneTower tower, TowerData towerData, Vector3 normal)
    {
        this.parentTower = tower;
        this.normal = normal.normalized;
        this.towerData = towerData;
        attackBehaviour = GetComponent<IAttackBehaviour>();
        initialOffset = parentTower.transform.right * rotationRadius;

    }

    void UpdateMovement()
    {
        Vector3 planetCenter = GameObject.Find("Planet").transform.position;

        Vector3 trueNormal = (parentTower.transform.position - planetCenter).normalized;

        angle += rotationSpeed * Time.deltaTime;

        Vector3 initialOffset = Vector3.Cross(trueNormal, Vector3.right).normalized * rotationRadius;

        Vector3 offset = Quaternion.AngleAxis(angle, trueNormal) * initialOffset;

        transform.position = parentTower.transform.position + offset;

        Vector3 tangent = Vector3.Cross(trueNormal, offset).normalized;
        transform.rotation = Quaternion.LookRotation(tangent, trueNormal);
    }


    void Update()
    {
        if (parentTower == null) return;

        UpdateMovement();

        Enemy target = FindClosestEnemy();
        if (target != null && attackBehaviour != null)
        {
            attackBehaviour.Attack(target.transform, firePoint, towerData.FireRate, towerData.Damage);
        }
    }

    Enemy FindClosestEnemy()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        Enemy closest = null;
        float minDistance = Mathf.Infinity;

        foreach (Enemy enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < minDistance && distance <= towerData.Range && enemy.animationStat != AnimationState.Dead)
            {
                minDistance = distance;
                closest = enemy;
            }
        }
        return closest;
    }
}