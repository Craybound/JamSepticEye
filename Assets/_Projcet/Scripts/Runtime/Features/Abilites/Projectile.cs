using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed = 16f;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private float damage = 8f;
    [SerializeField] private LayerMask hitMask;

    private float _t;

    private void Reset()
    {
        var c = GetComponent<Collider>();
        c.isTrigger = true;
    }

    private void Update()
    {
        transform.position += transform.forward * (speed * Time.deltaTime);
        _t += Time.deltaTime;
        if (_t >= lifeTime) Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & hitMask) == 0) return;

        // Try typical enemy health paths
        if (other.TryGetComponent(out EnemyController ec))
            ec.TakeDamage(Mathf.RoundToInt(damage));
        else if (other.TryGetComponent(out IDamageable id))
            id.ApplyDamage(Mathf.RoundToInt(damage), other.ClosestPoint(transform.position), transform.forward);

        Destroy(gameObject);
    }

    // Allow ability to set numbers at runtime
    public void Prime(float dmg, float spd, LayerMask mask)
    {
        damage = dmg; speed = spd; hitMask = mask;
    }
}
