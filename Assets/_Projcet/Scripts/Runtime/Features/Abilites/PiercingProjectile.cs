using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Collider))]
public class PiercingProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 20f;
    [SerializeField] private float lifeTime = 3f;
    [SerializeField] private float damage = 10f;
    [SerializeField] private int maxPierces = 4;
    [SerializeField] private LayerMask hitMask;

    // Optional: tiny self-collision buffer
    [SerializeField] private float ignoreOwnerRadius = 0.5f;

    private float _t;
    private Transform _owner;
    private readonly HashSet<Collider> _hit = new();

    private void Reset()
    {
        var c = GetComponent<Collider>();
        c.isTrigger = true; // IMPORTANT
    }

    public void Prime(float dmg, float spd, int pierces, LayerMask mask, Transform owner)
    {
        damage = dmg;
        speed = spd;
        maxPierces = pierces;
        hitMask = mask;
        _owner = owner;
    }

    private void Update()
    {
        transform.position += transform.forward * (speed * Time.deltaTime);
        _t += Time.deltaTime;
        if (_t >= lifeTime) Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        // ignore owner / allies
        if (_owner && (other.transform == _owner || other.transform.IsChildOf(_owner))) return;

        // layer filter
        if (((1 << other.gameObject.layer) & hitMask) == 0) return;

        // avoid double-hitting multi-collider enemies
        if (_hit.Contains(other)) return;
        _hit.Add(other);

        // Deal damage (adapt if your API differs)
        if (other.TryGetComponent(out EnemyController ec))
            ec.TakeDamage(Mathf.RoundToInt(damage));
        else if (other.TryGetComponent(out IDamageable id))
            id.ApplyDamage(Mathf.RoundToInt(damage), other.ClosestPoint(transform.position), transform.forward);

        maxPierces--;
        if (maxPierces <= 0) Destroy(gameObject);
    }
}
