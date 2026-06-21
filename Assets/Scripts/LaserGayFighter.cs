using UnityEngine;

public class LaserGayFighter : Fighter
{
    [Header("Laser Attack")]
    public GameObject laserPrefab;
    public Transform firePoint;
    public float laserCooldown = 2f;
    private float lastLaserTime = -1f;

    protected override void OnEnemyContact(UnityEngine.Collision2D collision)
    {
        // LaserGay does not attack on contact; behavior is handled by laser
    }

    protected override void Start()
    {
        base.Start();
        InvokeRepeating(nameof(FireLaser), 0.5f, laserCooldown);
    }

    void FireLaser()
    {
        if (firePoint == null || laserPrefab == null) return;
        var go = Instantiate(laserPrefab, firePoint.position, Quaternion.identity);
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
