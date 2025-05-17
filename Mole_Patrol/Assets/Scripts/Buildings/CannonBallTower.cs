using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CannonBallTower : Tower
{
    private IAttackBehaviour attackStrategy;
    private Enemy targetEnemy;

    public GameObject Turret;

    public override void Initialise(TowerData data)
    {
        // Always call base first, regardless of server/client
        base.Initialise(data);

        if (!IsServer) return;

        if (data == null)
        {
            Debug.Log("Null data");
            return;
        }

        if (data.ID.NullID())
        {
            data.ID = TowerData.GetNewTowerID();
        }

        currentHealth.Value = data.MaxHealth;
        attackStrategy = GetComponent<IAttackBehaviour>();

        Debug.Log(currentHealth.Value);
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

            if (distance < shortestDistance && distance <= range.Value && enemy.animationStat != AnimationState.Dead)
            {
                shortestDistance = distance;
                nearestEnemy = enemy;
            }
        }
        if (targetEnemy == null || targetEnemy.animationStat == AnimationState.Dead)
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
