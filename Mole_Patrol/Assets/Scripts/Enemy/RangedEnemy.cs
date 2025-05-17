using UnityEngine;

public class RangedEnemy : Enemy
{

    public float inShootingRange = 2;
    public Transform shootingPoint;
    public GameObject bullet;

    private int goldGainOnDeath = 80;

    private void Start()
    {
        KillGoldGain = goldGainOnDeath;
    }

        private void FixedUpdate()
    {
        if (animationStat != AnimationState.Dead)
        {
            // only move if we are grounded
            if (grounded)
            {
                if (target != null)
                {
                    findClosestTarget();

                    if (!attacking)
                    {
                        animationStat = AnimationState.Walking;
                        MoveToTarget();
                       //shootingRangeCheck();
                    }
                    else
                    {
                        rb.linearVelocity = Vector3.zero;
                    }

                }
                else
                {
                    animationStat = AnimationState.Idle;
                    findClosestTarget();
                }


            }


        }
        animator.SetInteger("State", (int)animationStat);
    }
    private void shootingRangeCheck()
    {
        float distance = Vector3.Distance(transform.position, target.transform.position);

        if ( distance < inShootingRange)
        {
            attacking = true;
            animationStat = AnimationState.Attacking;

        }
    }

    public override void attackTarget()
    {
        if (attacking && target != null)
        {

            Debug.Log("Attack");
            //target.GetComponent<Tower>().TakeDamage(damage);
        }
    }

    protected void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Triangle")
        {
            grounded = true;
        }

        if (collision.gameObject.tag == "Tower")
        {
            attacking = true;
            animationStat = AnimationState.Attacking;

        }

    }


    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.tag == "Triangle")
        {
            grounded = true;
        }

        if (collision.gameObject.tag == "Tower")
        {

            attacking = false;

        }
    }
}
