using System.Collections;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class LaserBeam2D : MonoBehaviour
{
    public Transform firePoint;
    public LayerMask wallLayer;
    public LayerMask boxerLayer;
    public float travelSpeed = 60f;
    public int maxBounces = 3;
    public float angleSpreadDeg = 6f;
    public float maxSegmentDistance = 50f;
    public float damage = 25f;

    LineRenderer lr;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;
    }

    void Start()
    {
        if (firePoint != null) transform.position = firePoint.position;
        StartCoroutine(RunBeam());
    }

    IEnumerator RunBeam()
    {
        Vector2 start = transform.position;
        Vector2 dir = (FindTargetDirection() ?? (Vector2)transform.right).normalized;
        int remaining = maxBounces;

        Vector3 curStart = start;
        Vector2 curDir = dir;

        while (remaining >= 0)
        {
            RaycastHit2D hit = Physics2D.Raycast(curStart + (Vector3)curDir * 0.01f, curDir, maxSegmentDistance, wallLayer | boxerLayer);
            Vector2 segEnd;
            bool hitBoxer = false;
            if (hit.collider != null)
            {
                segEnd = hit.point;
                if (((1 << hit.collider.gameObject.layer) & boxerLayer) != 0)
                {
                    hitBoxer = true;
                }
            }
            else
            {
                segEnd = curStart + (Vector3)curDir * maxSegmentDistance;
            }

            float dist = Vector2.Distance(curStart, segEnd);
            float t = 0f;
            float duration = Mathf.Max(0.01f, dist / travelSpeed);

            lr.positionCount = 2;
            lr.SetPosition(0, curStart);
            lr.SetPosition(1, curStart);

            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                Vector2 pos = Vector2.Lerp(curStart, segEnd, t);
                lr.SetPosition(1, pos);
                yield return null;
            }
            lr.SetPosition(1, segEnd);

            if (hit.collider != null && hitBoxer)
            {
                var f = hit.collider.GetComponent<Fighter>();
                if (f != null) f.TakeDamage(damage);
                Destroy(gameObject);
                yield break;
            }

            if (hit.collider != null && (((1 << hit.collider.gameObject.layer) & wallLayer) != 0))
            {
                Vector2 normal = hit.normal;
                Vector2 reflected = Vector2.Reflect(curDir, normal).normalized;
                float rand = Random.Range(-angleSpreadDeg, angleSpreadDeg);
                reflected = Rotate(reflected, rand).normalized;

                curStart = segEnd;
                curDir = reflected;
                remaining--;

                curStart += (Vector3)curDir * 0.01f;

                lr.SetPosition(0, curStart);
                lr.SetPosition(1, curStart);
                yield return null;
                continue;
            }

            curStart = segEnd;
            yield return null;
        }

        Destroy(gameObject);
    }

    Vector2? FindTargetDirection()
    {
        var boxer = GameObject.FindWithTag("Boxer");
        if (boxer != null) return (Vector2)(boxer.transform.position - transform.position);
        return null;
    }

    static Vector2 Rotate(Vector2 v, float degrees)
    {
        float r = degrees * Mathf.Deg2Rad;
        float c = Mathf.Cos(r), s = Mathf.Sin(r);
        return new Vector2(v.x * c - v.y * s, v.x * s + v.y * c);
    }
}
