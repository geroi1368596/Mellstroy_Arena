using UnityEngine;

public class LaserGayFighter : Fighter
{
    [Header("Laser Attack")]
    public GameObject laserPrefab;
    public Transform firePoint;
    public float laserCooldown = 2f;
    private float lastLaserTime = -1f;

    [Header("Movement")]
    public float initialSpeed = 2f; // will be used if base doesn't set a velocity

    protected override void OnEnemyContact(UnityEngine.Collision2D collision)
    {
        // LaserGay does not attack on contact; behavior is handled by laser
    }

    protected override void Start()
    {
        base.Start();
        // ensure movement even if something else overwrote it: set after one frame
        StartCoroutine(SetInitialVelocityNextFrame());
        InvokeRepeating(nameof(FireLaser), 0.5f, laserCooldown);
    }

    private System.Collections.IEnumerator SetInitialVelocityNextFrame()
    {
        yield return null; // wait one frame
        var rbLocal = rb; // protected field from base
        if (rbLocal == null)
        {
            Debug.LogWarning($"{name}: Rigidbody2D missing, cannot set velocity");
            yield break;
        }

        if (rbLocal.velocity.sqrMagnitude < 0.01f)
        {
            Vector2 dir = transform.position.x < 0f ? Vector2.right : Vector2.left;
            float sp = (speed > 0f) ? speed : initialSpeed;
            rbLocal.velocity = dir * sp;
            Debug.Log($"{name}: initial velocity forced to {rbLocal.velocity}");
        }
        else
        {
            Debug.Log($"{name}: already moving, velocity={rbLocal.velocity}");
        }
    }

    void FireLaser()
    {
        if (firePoint == null)
        {
            Debug.LogWarning($"{name}: FirePoint not assigned — cannot fire laser");
            return;
        }
        if (laserPrefab == null)
        {
            Debug.LogWarning($"{name}: laserPrefab not assigned — cannot fire laser");
            return;
        }

        var go = Instantiate(laserPrefab, firePoint.position, Quaternion.identity);

        // Ensure laser is in 2D plane
        var p = go.transform.position;
        p.z = 0f;
        go.transform.position = p;

        // Try to make LineRenderer render on top (if present)
        var lr = go.GetComponent<LineRenderer>();
        if (lr != null)
        {
            lr.material = lr.material ?? new Material(Shader.Find("Sprites/Default"));
            lr.sortingLayerName = "Default"; // change to "Laser" if you create that sorting layer
            lr.sortingOrder = 100;
        }

        var beam = go.GetComponent<LaserBeam2D>();
        if (beam != null)
        {
            beam.firePoint = firePoint;
        }
    }

    protected override void Die()
    {
        CancelInvoke();
        base.Die();
    }
}
