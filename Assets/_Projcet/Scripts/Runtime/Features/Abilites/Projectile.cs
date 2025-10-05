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
            // Detect all enemies within a radius around current position
            var hits = Physics.OverlapSphere(transform.position, hitRadius, hitMask, QueryTriggerInteraction.Collide);

            bool hitSomething = false;

            foreach (var h in hits)
            {
                // Try to find an enemy controller
                if (h.TryGetComponent<EnemyController>(out EnemyController enemy))
                {
                    enemy.TakeDamage((int)damage);
                    hitSomething = true;
                }
                else
                {
                    // Fallback: try damage utility if not an enemy
                    CombatUtils.TryDamage(h.gameObject, damage);
                    hitSomething = true;
                }
            }

            // Optional: if we hit anything, handle destruction or effects
            if (hitSomething)
            {
                // Optional VFX position (stays where attack started, or you can move)
                // transform.position = ???; // OverlapSphere doesn’t give a hit point, so skip or use nearest enemy.

                if (destroyOnHit)
                {
                    Destroy(gameObject);
                    return;
                }
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
