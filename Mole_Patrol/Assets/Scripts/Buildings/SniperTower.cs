using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SniperTower : Tower
{
    private IAttackBehaviour attackStrategy;
    private Enemy targetEnemy;

    public GameObject Turret;

    private Vector3 planetCenter = Vector3.zero; // Assuming the planet is at (0,0,0)

    public float cutOffRange = 6.0f;

    public void Start()
    {
        
    }

    public override void Initialise(TowerData data)
    {
        base.Initialise(data); // Required to ensure clients are properly initialized

        if (data.ID.NullID())
        {
            data.ID = TowerData.GetNewTowerID();
        }

        towerData = data;
        currentHealth.Value = data.MaxHealth;
        attackStrategy = GetComponent<IAttackBehaviour>();
        range.Value = data.Range;
    }


    public override void Update()
    {
        if (!IsServer) return;
        if (!placed.Value) { return; }

        FindTarget();
        if (targetEnemy != null && attackStrategy != null)
        {
            attackStrategy.Attack(targetEnemy.transform, firePoint, towerData.FireRate, towerData.Damage);
            LookAtTarget(targetEnemy.transform);
        }
        if (currentHealth.Value <= 0)
        {
            GameManager.Instance.RemoveTowerFromDataList(towerData.ID);
            GetComponent<NetworkObject>().Despawn();
            //Destroy(gameObject);
            SelectionRing.Instance.DeselectTower();
        }
    }


    void LookAtTarget(Transform targetPos)
    {
        Vector3 aimVector;

        aimVector = Turret.transform.position - targetPos.position; //vector to player pos
        aimVector = Vector3.ProjectOnPlane(aimVector, Turret.transform.up); //project it on an x-z plane 

        Quaternion q_penguin = Quaternion.LookRotation(aimVector, Turret.transform.up);


        Turret.transform.rotation = Quaternion.Slerp(Turret.transform.rotation, q_penguin, 10f * Time.deltaTime); //slowly transition
        UpdateTurretRotationClientRpc(Turret.transform.rotation);
    }

    void FindTarget()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        float shortestDistance = Mathf.Infinity;
        Enemy nearestEnemy = null;

        foreach (Enemy enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);

            // Only consider enemies that are within range and not too close.
            if (distance < shortestDistance && distance <= range.Value && distance > cutOffRange && enemy.animationStat != AnimationState.Dead)
            {
                shortestDistance = distance;
                nearestEnemy = enemy;
            }
        }

        // If there's no current target or the current one is dead, update to the nearest valid enemy.
        if (targetEnemy == null || targetEnemy.animationStat == AnimationState.Dead)
        {
            targetEnemy = nearestEnemy;
        }

        // If the current target moved into the cutOffRange, drop it.
        if (targetEnemy != null)
        {
            float distanceToTrackedEnemy = Vector3.Distance(transform.position, targetEnemy.transform.position);
            Debug.Log("Distance to tracked enemy: " + distanceToTrackedEnemy);

            if (distanceToTrackedEnemy <= cutOffRange)
            {
                targetEnemy = null;
                Debug.Log("swap targets");
            }
        }
    }

    [ClientRpc]
    void UpdateTurretRotationClientRpc(Quaternion newRotation)
    {
        Turret.transform.rotation = newRotation;
    }

}
