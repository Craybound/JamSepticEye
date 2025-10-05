using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Projectile Config")]
    [SerializeField] private float speed = 15f;           
    [SerializeField] private float lifeTime = 3f;         
    [SerializeField] private float damage = 10f;          
    [SerializeField] private LayerMask hitMask;           
    [SerializeField] private float hitRadius = 0.15f;     // NEW: sweep radius
    [SerializeField] private bool destroyOnHit = true;

    private float _timer;
    private Vector3 _prevPos;

    private void OnEnable()
    {
        _prevPos = transform.position;
    }

    private void Update()
    {
        float dt = Time.deltaTime;
        _timer += dt;
        if (_timer >= lifeTime) { Destroy(gameObject); return; }

        Vector3 nextPos = transform.position + transform.forward * speed * dt;
        Vector3 dir = nextPos - transform.position;
        float dist = dir.magnitude;
        if (dist > 0f)
        {
            // Sweep forward this frame; include triggers
            if (Physics.SphereCast(transform.position, hitRadius, dir.normalized, out var hit, dist, hitMask, QueryTriggerInteraction.Collide))
            {
                // Damage the thing we hit (root or child)
                CombatUtils.TryDamage(hit.collider.gameObject, damage);

                // Land at the hit point so VFX look right
                transform.position = hit.point;

                if (destroyOnHit) { Destroy(gameObject); return; }
            }
        }

        transform.position = nextPos;
        _prevPos = transform.position;
    }

    // Ability primes runtime config
    public void Prime(float dmg, float spd, LayerMask mask)
    {
        damage = dmg; speed = spd; hitMask = mask;
    }
}
