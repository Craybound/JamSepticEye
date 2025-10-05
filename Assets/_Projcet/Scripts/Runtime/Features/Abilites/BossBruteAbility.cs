using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Abilities/Boss Brute (Captain)")]
public class BossBruteAbility : AbilitySO
{
    // ==========================
    // === Primary: Fast Heavy Swing
    // ==========================
    [Header("Primary (Fast Heavy Swing)")]
    [SerializeField] private float heavyDamage = 24f;
    [SerializeField] private float heavyRadius = 2.2f;
    [Tooltip("Half-angle for the front cone (deg). 45 = 90° total cone.")]
    [SerializeField] private float heavyHalfAngle = 45f;
    [Tooltip("Lower than Elite Brute to feel snappier.")]
    [SerializeField] private float heavyCooldown = 0.8f;     // faster than elite
    [SerializeField] private float heavyKnockback = 7f;
    [SerializeField] private LayerMask enemyMask;

    [Header("Animator/Hitbox (optional)")]
    [Tooltip("Use Animator trigger + WeaponHitbox (like your Dasher/Brute). Will fallback to cone if missing.")]
    [SerializeField] private bool useWeaponHitboxForPrimary = true;
    [SerializeField] private string attackTriggerName = "Attack";  // set to your actual trigger
    [SerializeField] private float fallbackHitDelay = 0.18f;       // roughly contact frame

    // ==========================
    // === Secondary: Ground Slam (360° AoE)
    // ==========================
    [Header("Secondary (Ground Slam)")]
    [Tooltip("Delay before the slam applies (windup / telegraph).")]
    [SerializeField] private float slamWindup = 0.35f;
    [SerializeField] private float slamRadius = 3.2f;
    [SerializeField] private float slamDamage = 22f;
    [SerializeField] private float slamKnockback = 10f;
    [SerializeField] private float slamCooldown = 3.0f;
    [SerializeField] private LayerMask slamEnemyMask;

    [Header("Optional FX hooks")]
    [SerializeField] private GameObject slamTelegraphPrefab; // spawned at feet during windup (auto-destroyed on slam)
    [SerializeField] private GameObject slamImpactPrefab;    // spawned on slam

    // ==========================
    // === Runtime
    // ==========================
    private float _cdPrimary;
    private float _cdSecondary;

    // --------- API ----------
    public override void OnPrimary(GameObject owner)
    {
        if (_cdPrimary > 0f) return;
        _cdPrimary = heavyCooldown;

        // Preferred path: Animator + WeaponHitbox (matches your other abilities)
        if (useWeaponHitboxForPrimary)
        {
            if (owner.TryGetComponent(out Animator anim))
                anim.SetTrigger(attackTriggerName);

            var hitbox = owner.GetComponentInChildren<WeaponHitbox>();
            if (hitbox != null)
            {
                hitbox.Init(owner, heavyDamage);
                Debug.Log("[Boss Brute] Primary via WeaponHitbox.");
                return;
            }
            else
            {
                Debug.LogWarning("[Boss Brute] WeaponHitbox not found—using cone fallback.");
            }
        }

        // Fallback: timed cone
        var host = owner.GetComponent<MonoBehaviour>();
        if (host != null)
            host.StartCoroutine(FallbackConeHit(owner, heavyDamage, heavyRadius, heavyHalfAngle, heavyKnockback, enemyMask, fallbackHitDelay));
    }

    public override void OnSecondary(GameObject owner)
    {
        if (_cdSecondary > 0f) return;

        var host = owner.GetComponent<MonoBehaviour>();
        if (host == null) return;

        _cdSecondary = slamCooldown;
        host.StartCoroutine(SlamRoutine(owner));
    }

    public override void Tick(GameObject owner, float dt)
    {
        if (_cdPrimary   > 0f) _cdPrimary   -= dt;
        if (_cdSecondary > 0f) _cdSecondary -= dt;
    }

    // --------- Internals ----------
    private IEnumerator FallbackConeHit(GameObject owner, float damage, float radius, float halfAngleDeg, float knock, LayerMask mask, float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        int hits = DoConeDamage(owner.transform, radius, halfAngleDeg, damage, knock, mask);
        Debug.Log($"[Boss Brute] Primary fallback cone hit {hits} target(s) for {damage}.");
    }

    private IEnumerator SlamRoutine(GameObject owner)
    {
        Transform t = owner.transform;

        // Telegraph
        GameObject tele = null;
        if (slamTelegraphPrefab != null)
            tele = Instantiate(slamTelegraphPrefab, t.position, Quaternion.identity);

        // Optional: play slam windup anim here (anim.SetTrigger("SlamWindup"))
        if (slamWindup > 0f) yield return new WaitForSeconds(slamWindup);

        if (tele != null) Destroy(tele);

        // Impact FX
        if (slamImpactPrefab != null)
            Instantiate(slamImpactPrefab, t.position, Quaternion.identity);

        // Apply 360° AoE damage + knockback
        int hits = DoRadialDamage(t.position, slamRadius, slamDamage, slamKnockback, slamEnemyMask);
        Debug.Log($"[Boss Brute] Ground slam hit {hits} target(s).");
    }

    // 360° AoE
    private static int DoRadialDamage(Vector3 center, float radius, float damage, float knockback, LayerMask mask)
    {
        int count = 0;
        var hits = Physics.OverlapSphere(center, radius, mask, QueryTriggerInteraction.Ignore);
        foreach (var h in hits)
        {
            // Damage
            if (h.TryGetComponent<EnemyController>(out var ec)) ec.TakeDamage(Mathf.RoundToInt(damage));
            else if (h.TryGetComponent<IDamageable>(out var id)) id.ApplyDamage(Mathf.RoundToInt(damage), h.ClosestPoint(center), (h.transform.position - center).normalized);

            // Knockback
            Vector3 dir = (h.transform.position - center); dir.y = 0f;
            if (dir.sqrMagnitude > 1e-4f)
            {
                if (h.attachedRigidbody) h.attachedRigidbody.AddForce(dir.normalized * knockback, ForceMode.VelocityChange);
                else if (h.TryGetComponent<CharacterController>(out var cc))
                    cc.Move(dir.normalized * knockback * Time.deltaTime);
            }
            count++;
        }
        return count;
    }

    // Front cone (used by primary fallback)
    private static int DoConeDamage(Transform owner, float radius, float halfAngleDeg, float damage, float knockback, LayerMask mask)
    {
        Vector3 origin  = owner.position;
        Vector3 forward = owner.forward;
        float minDot    = Mathf.Cos(halfAngleDeg * Mathf.Deg2Rad);

        int count = 0;
        var hits = Physics.OverlapSphere(origin, radius, mask, QueryTriggerInteraction.Ignore);
        foreach (var h in hits)
        {
            Vector3 to = h.transform.position - origin; to.y = 0f;
            if (to.sqrMagnitude < 1e-4f) continue;
            if (Vector3.Dot(forward, to.normalized) < minDot) continue;

            if (h.TryGetComponent<EnemyController>(out var ec)) ec.TakeDamage(Mathf.RoundToInt(damage));
            else if (h.TryGetComponent<IDamageable>(out var id)) id.ApplyDamage(Mathf.RoundToInt(damage), h.ClosestPoint(origin), -forward);

            if (knockback > 0f)
            {
                if (h.attachedRigidbody) h.attachedRigidbody.AddForce(to.normalized * knockback, ForceMode.VelocityChange);
                else if (h.TryGetComponent<CharacterController>(out var cc))
                    cc.Move(to.normalized * knockback * Time.deltaTime);
            }
            count++;
        }
        return count;
    }
}
