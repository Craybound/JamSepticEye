using UnityEngine;

/// <summary>
/// Simple projectile that damages the first IDamageable it collides with.
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("Projectile Config")]
    [SerializeField] private float speed = 15f;            // m/s
    [SerializeField] private float lifeTime = 3f;          // seconds before auto-destroy
    [SerializeField] private float damage = 10f;           // base damage
    [SerializeField] private LayerMask hitMask;            // what can it hit (set to Enemy)
    [SerializeField] private bool destroyOnHit = true;     // whether to disappear on impact

    private float _timer;

    private void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
        _timer += Time.deltaTime;

        if (_timer >= lifeTime)
            Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & hitMask) == 0) return;

        CombatUtils.TryDamage(other.gameObject, damage);

        if (destroyOnHit)
            Destroy(gameObject);
    }

    /// <summary>
    /// Initializes projectile from an ability at runtime.
    /// </summary>
    public void Prime(float dmg, float spd, LayerMask mask)
    {
        damage = dmg;
        speed = spd;
        hitMask = mask;
    }
}
