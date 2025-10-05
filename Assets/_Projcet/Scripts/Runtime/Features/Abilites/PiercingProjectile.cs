using UnityEngine;

/// <summary>
/// Projectile that pierces multiple enemies before despawning.
/// </summary>
public class PiercingProjectile : MonoBehaviour
{
    [Header("Piercing Config")]
    [SerializeField] private float speed = 18f;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private int maxPierces = 4;
    [SerializeField] private LayerMask hitMask;

    private int _hitCount;
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
        _hitCount++;

        if (_hitCount >= maxPierces)
            Destroy(gameObject);
    }

    public void Prime(float dmg, float spd, int pierceCount, LayerMask mask)
    {
        damage = dmg;
        speed = spd;
        maxPierces = pierceCount;
        hitMask = mask;
    }
}
