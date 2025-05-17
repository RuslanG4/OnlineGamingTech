using UnityEngine;

// Interface for attack behaviour for different buildings
public interface IAttackBehaviour
{
    void Attack(Transform target, Transform firePoint, float fireRate, float damage);
}
