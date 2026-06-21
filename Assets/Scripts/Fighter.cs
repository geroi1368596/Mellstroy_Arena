using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class Fighter : MonoBehaviour
{
    [Header("Base Stats")]
    public float maxHealth = 100f;
    [HideInInspector]
    public float currentHealth;
    public float speed = 8f;
    public string wallTag = "Wall";
    public string enemyTag = "Enemy";

    [Header("Bounce")]
    [Range(0f, 45f)]
    public float bounceAngleSpread = 6f;

    [Header("Combat")]
    public float collisionDamage = 20f;
    public float attackCooldown = 0.5f;
    public float knockbackForce = 10f;

    protected Rigidbody2D rb;
    protected float lastAttackTime = -1f;
    protected Vector2 currentDirection;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation = true;
    }

    protected virtual void Start()
    {
        currentHealth = maxHealth;
        currentDirection = Random.insideUnitCircle.normalized;
        if (currentDirection == Vector2.zero) currentDirection = Vector2.right;
        rb.velocity = currentDirection * speed;
    }

    protected virtual void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag(wallTag))
        {
            BounceOffWall(collision);
        }

        if (collision.collider.CompareTag(enemyTag))
        {
            OnEnemyContact(collision);
        }
    }

    protected void BounceOffWall(Collision2D collision)
    {
        ContactPoint2D contact = collision.GetContact(0);
        Vector2 normal = contact.normal;
        Vector2 inDir = rb.velocity.normalized;
        Vector2 reflected = Vector2.Reflect(inDir, normal).normalized;

        float randAngle = Random.Range(-bounceAngleSpread, bounceAngleSpread);
        reflected = Rotate(reflected, randAngle).normalized;

        rb.position += reflected * 0.02f;
        rb.velocity = reflected * speed;
        currentDirection = reflected;
    }

    protected virtual void OnEnemyContact(Collision2D collision)
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            Attack(collision);
            lastAttackTime = Time.time;
        }
    }

    protected virtual void Attack(Collision2D collision)
    {
        Fighter enemy = collision.gameObject.GetComponent<Fighter>();
        if (enemy != null)
        {
            enemy.TakeDamage(collisionDamage);
        }

        Vector2 knockbackDir = (collision.transform.position - transform.position).normalized;
        rb.velocity = -knockbackDir * knockbackForce;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        Destroy(gameObject);
    }

    static Vector2 Rotate(Vector2 v, float degrees)
    {
        float rad = degrees * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);
        return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
    }
}
