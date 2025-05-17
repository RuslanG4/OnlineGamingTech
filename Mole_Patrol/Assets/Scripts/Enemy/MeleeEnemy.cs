using UnityEngine;

public class MeleeEnemy :  Enemy
{
    private void FixedUpdate()
    {
        if (!IsServer) return;
        if (animationStat != AnimationState.Dead && isReadyToUpdate)
        {
            // only move if we are grounded
            if (grounded)
            {
                findClosestTarget();
                if (target != null)
                {
                    if (target.GetComponent<Tower>().broken.Value)
                    {
                        attacking = false;
                    }
                    float distance = Vector3.Distance(transform.position, target.transform.position);

                    if (distance > 2 || !attacking)
                    {
                        animationStat = AnimationState.Walking;
                        findClosestTarget();
                        MoveToTarget();
                    }
                    else
                    {
                        rb.linearVelocity = Vector3.zero;
                    }

                }
                else
                {
                    attacking = false;
                    animationStat = AnimationState.Idle;
                }

            }


        }
        animator.SetInteger("State", (int)animationStat);
    }

    public override void attackTarget()
    {
        if (attacking && target != null)
        {
            target.GetComponent<Tower>().TakeDamage(damage);
        }
    }


    protected void OnCollisionEnter(Collision collision)
    {
        if (!IsServer) return;
        if (collision.gameObject.tag == "Triangle")
        {
            grounded = true;
        }

        if (collision.gameObject.tag == "Tower" && target == collision.gameObject)
        {
            attacking = true;
            animationStat = AnimationState.Attacking;
        }
    }


    private void OnCollisionExit(Collision collision)
    {
        if (!IsServer) return;
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
