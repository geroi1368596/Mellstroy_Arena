using UnityEngine;

public class BoxerFighter : Fighter
{
    [Header("Boxer Ability")]
    public float dashCooldown = 1f;
    public float dashForce = 20f;
    private float lastDashTime = -1f;

    protected override void OnEnemyContact(Collision2D collision)
    {
        if (collision.collider.CompareTag(enemyTag))
        {
            if (Time.time >= lastDashTime + dashCooldown)
            {
                DashAttack(collision);
                lastDashTime = Time.time;
            }
            else
            {
                base.OnEnemyContact(collision);
            }
        }
    }

    void DashAttack(Collision2D collision)
    {
        Vector2 dashDir = (collision.transform.position - transform.position).normalized;
        rb.velocity = dashDir * dashForce;

        Fighter enemy = collision.gameObject.GetComponent<Fighter>();
        if (enemy != null)
        {
            enemy.TakeDamage(collisionDamage * 1.5f);
        }

        lastAttackTime = Time.time;
    }
}
