using UnityEngine;

public class AnimationDestroyer : MonoBehaviour
{
    public Enemy thisEnemy;

    public void attack()
    {
        if (thisEnemy)
        {
            thisEnemy.attackTarget();
        }
    }

    public void DestroySelf()
    {
        Destroy(transform.parent.gameObject);
    }
}
