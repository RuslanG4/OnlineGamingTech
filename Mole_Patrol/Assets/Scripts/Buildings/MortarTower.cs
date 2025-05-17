using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Mortar : Tower
{
    private IAttackBehaviour attackStrategy;
    private Enemy targetEnemy;

    public GameObject Turret;

    public override void Initialise(TowerData data)
    {
        base.Initialise(data); // Ensures client gets data and base setup is completed

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

        aimVector = targetPos.position - Turret.transform.position; //vector to player pos
        aimVector = Vector3.ProjectOnPlane(aimVector, Turret.transform.up); //project it on an x-z plane 

        Quaternion q_penguin = Quaternion.LookRotation(aimVector, Turret.transform.up);


        Turret.transform.rotation = Quaternion.Slerp(Turret.transform.rotation, q_penguin, 10f * Time.deltaTime); //slowly transition
        UpdateTurretRotationClientRpc(Turret.transform.rotation);
    }


    void FindTarget()
    {
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        float shortestDistance = range.Value;
        Enemy nearestEnemy = null;

        foreach (Enemy enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance < shortestDistance && distance <= range.Value && enemy.animationStat != AnimationState.Dead)
            {
                shortestDistance = distance;
                nearestEnemy = enemy;
            }
        }
        if (targetEnemy == null)
        {
            targetEnemy = nearestEnemy;
        }
    }

    [ClientRpc]
    void UpdateTurretRotationClientRpc(Quaternion newRotation)
    {
        Turret.transform.rotation = newRotation;
    }
}
