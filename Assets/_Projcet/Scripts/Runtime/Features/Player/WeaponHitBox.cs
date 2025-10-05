using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class WeaponHitbox : MonoBehaviour
{
    [SerializeField] private Collider _collider;
    private float _damage;
    private string _targetTag = "Enemy";
    [SerializeField] private GameObject _owner;

    private void Awake()
    {
        _collider = GetComponent<Collider>();
    }

    public void Init(GameObject owner, float damage, string targetTag = "Enemy")
    {
        _owner = owner;
        _damage = damage;
    }




    private void OnTriggerEnter(Collider other)
    {
        // Ignore self
        if (other.gameObject == _owner)
            return;

        // Optional: tag filter (only hit enemies, etc.)
        if (!string.IsNullOrEmpty(_targetTag) && !other.CompareTag(_targetTag))
            return;

        // --- Damage Handling ---
        // Try damage via EnemyController
        if (other.TryGetComponent(out EnemyController enemy))
        {
            if (enemy.IsInteractable)
            {
                return; 
            }
            else
            {
                enemy.TakeDamage((int)_damage);
            }
        }

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
