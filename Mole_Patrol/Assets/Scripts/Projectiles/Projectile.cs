using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public abstract class Projectile : NetworkBehaviour
{
    public static int HitGoldGain = 5;

    protected Rigidbody rb;
    protected Transform target;
    protected float speed;
    protected float gravityEffect = -0.5f; // Random gravity pull
    public float damage;
    protected Vector3 planetCenter = Vector3.zero; // Assuming the planet is at (0,0,0)

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public abstract void Initialize(Transform target, float speed, float damage);
}
