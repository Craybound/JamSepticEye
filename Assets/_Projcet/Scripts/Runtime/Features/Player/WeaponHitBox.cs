using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WeaponHitbox : MonoBehaviour
{
    private Collider _collider;
    private float _damage;
    private string _targetTag = "Enemy";
    private GameObject _owner;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
        _collider.enabled = false;
        _collider.isTrigger = true;
    }

    public void Init(GameObject owner, float damage, string targetTag = "Enemy")
    {
        _owner = owner;
        _damage = damage;
        _targetTag = targetTag;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == _owner) return;
        if (!other.CompareTag(_targetTag)) return;

        Debug.Log($"[Hitbox] {_owner.name} hit {other.name} for {_damage} damage!");
        // TODO: call other.GetComponent<IDamageable>()?.TakeDamage(_damage);
    }

    public void EnableHitbox() => _collider.enabled = true;
    public void DisableHitbox() => _collider.enabled = false;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (_collider == null) _collider = GetComponent<Collider>();
        Gizmos.color = _collider.enabled ? Color.red : Color.gray;
        Gizmos.DrawWireCube(transform.position, _collider.bounds.size);
    }
#endif

}
