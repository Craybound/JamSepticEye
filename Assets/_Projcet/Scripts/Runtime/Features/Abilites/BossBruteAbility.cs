using UnityEngine;

[CreateAssetMenu(menuName="Abilities/Boss Brute")]
public class BossBruteAbility : AbilitySO
{
    [Header("Fast Heavy (LMB)")]
    [SerializeField] private float damage = 20f;
    [SerializeField] private float radius = 2.1f;
    [SerializeField] private float halfAngle = 45f;
    [SerializeField] private float cooldown = 0.8f;
    [SerializeField] private LayerMask enemyMask;

    [Header("Ground Slam 360° (RMB)")]
    [SerializeField] private float slamRadius = 3.2f;
    [SerializeField] private float slamDamage = 22f;
    [SerializeField] private float slamKnockback = 8f;
    [SerializeField] private float slamCooldown = 3.0f;
    [SerializeField] private LayerMask slamEnemyMask;

    private float _cdL, _cdR;

    public override void OnPrimary(GameObject owner)
    {
        if (_cdL > 0f) return; 
        _cdL = cooldown;

        var aim = owner.GetComponent<PlayerAimSource>();
        Vector3 f = aim ? aim.Forward : owner.transform.forward;

        var hits = Physics.OverlapSphere(owner.transform.position, radius, enemyMask, QueryTriggerInteraction.Ignore);
        foreach (var h in hits)
        {
            Vector3 to = h.transform.position - owner.transform.position; to.y = 0f;
            if (Vector3.Dot(f, to.normalized) >= Mathf.Cos(halfAngle * Mathf.Deg2Rad))
                CombatUtils.TryDamage(h.gameObject, damage);
        }
    }

    public override void OnSecondary(GameObject owner)
    {
        if (_cdR > 0f) return; 
        _cdR = slamCooldown;

        var hits = Physics.OverlapSphere(owner.transform.position, slamRadius, slamEnemyMask, QueryTriggerInteraction.Ignore);
        foreach (var h in hits)
        {
            CombatUtils.TryDamage(h.gameObject, slamDamage);
            Vector3 dir = h.transform.position - owner.transform.position; dir.y = 0f;
            CombatUtils.TryKnockback(h.gameObject, dir, slamKnockback);
        }
    }

    public override void Tick(GameObject owner, float dt)
    {
        if (_cdL > 0f) _cdL -= dt;
        if (_cdR > 0f) _cdR -= dt;
    }
}
