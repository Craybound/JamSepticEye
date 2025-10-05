using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "Abilities/Elite Brute (Safe)")]
public class EliteBruteAbility : AbilitySO
{
    // ==========================
    // === Primary: Heavy Swing
    // ==========================
    [Header("Primary (Heavy Swing)")]
    [SerializeField] private float heavyDamage = 22f;
    [SerializeField] private float heavyRadius = 2.1f;
    [Tooltip("Half-angle for the front cone (degrees). 45 = 90° total cone.")]
    [SerializeField] private float heavyHalfAngle = 45f;
    [SerializeField] private float heavyCooldown = 1.25f;  // slower recovery
    [SerializeField] private float heavyKnockback = 7f;    // optional
    [SerializeField] private LayerMask enemyMask;

    [Header("Animator/Hitbox")]
    [Tooltip("If true, uses Animator trigger + WeaponHitbox like Elite Dasher. If hitbox or event is missing, will auto-fallback to cone.")]
    [SerializeField] private bool useWeaponHitboxForPrimary = true;
    [SerializeField] private string attackTriggerName = "Attack"; // <- make this match your Elite Dasher trigger

    [Tooltip("Delay before fallback cone applies (matches contact frame).")]
    [SerializeField] private float fallbackHitDelay = 0.25f;

    // ==========================
    // === Secondary: Charged Cone
    // ==========================
    [Header("Secondary (Charged 45° Cone)")]
    [SerializeField] private float minChargeTime = 0.20f;
    [SerializeField] private float maxChargeTime = 1.50f;
    [SerializeField] private float minChargedDamage = 24f;
    [SerializeField] private float maxChargedDamage = 50f;
    [SerializeField] private float chargedRadius = 2.4f;
    [SerializeField] private float chargedHalfAngle = 45f;
    [SerializeField] private float chargedKnockback = 10f;
    [SerializeField] private float chargedCooldown = 2.4f;

    [SerializeField] private bool scaleRadiusWithCharge = false;
    [SerializeField] private float maxChargedRadiusBonus = 0.6f;

    // ==========================
    // === Runtime
    // ==========================
    private float _cdPrimary;
    private float _cdSecondary;
    private bool _isCharging;

    public override void OnPrimary(GameObject owner)
    {
        if (_cdPrimary > 0f) return;
        _cdPrimary = heavyCooldown;

        // Try Animator + WeaponHitbox path first (same as Elite Dasher)
        if (useWeaponHitboxForPrimary)
        {
            if (owner.TryGetComponent(out Animator anim))
                anim.SetTrigger(attackTriggerName);

            var hitbox = owner.GetComponentInChildren<WeaponHitbox>();
            if (hitbox != null)
            {
                // This should apply damage via animation event timing
                hitbox.Init(owner, heavyDamage);
                Debug.Log("[Elite Brute] Primary via WeaponHitbox.");
                return;
            }
            else
            {
                Debug.LogWarning("[Elite Brute] WeaponHitbox not found—falling back to cone hit.");
            }
        }

        // Fallback: timed cone (no animation event needed)
        var host = owner.GetComponent<MonoBehaviour>();
        if (host != null) host.StartCoroutine(FallbackConeHit(owner, heavyDamage, heavyRadius, heavyHalfAngle, heavyKnockback, fallbackHitDelay));
    }

    public override void OnSecondary(GameObject owner)
    {
        if (_cdSecondary > 0f || _isCharging) return;
        var host = owner.GetComponent<MonoBehaviour>();
        if (host == null) return;
        host.StartCoroutine(ChargeAndStrike(owner));
    }

    public override void Tick(GameObject owner, float deltaTime)
    {
        if (_cdPrimary   > 0f) _cdPrimary   -= deltaTime;
        if (_cdSecondary > 0f) _cdSecondary -= deltaTime;
    }

    // ----- internals -----

    private IEnumerator FallbackConeHit(GameObject owner, float damage, float radius, float halfAngleDeg, float knock, float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);

        int hits = DoConeDamage(owner.transform, radius, halfAngleDeg, damage, knock, enemyMask);
        Debug.Log($"[Elite Brute] Primary fallback cone hit {hits} target(s) for {damage}.");
    }

    private IEnumerator ChargeAndStrike(GameObject owner)
    {
        _isCharging = true;
        float t = 0f;

        // Hold while RMB pressed (jam-safe). You can swap to performed/canceled later.
        while (t < maxChargeTime && Mouse.current != null && Mouse.current.rightButton.isPressed)
        {
            t += Time.deltaTime;
            yield return null;
        }
        _isCharging = false;

        float clamped = Mathf.Clamp(Mathf.Max(minChargeTime, t), minChargeTime, maxChargeTime);
        float alpha   = Mathf.InverseLerp(minChargeTime, maxChargeTime, clamped);

        float dmg     = Mathf.Lerp(minChargedDamage, maxChargedDamage, alpha);
        float radius  = chargedRadius + (scaleRadiusWithCharge ? alpha * maxChargedRadiusBonus : 0f);

        int hits = DoConeDamage(owner.transform, radius, chargedHalfAngle, dmg, chargedKnockback, enemyMask);
        _cdSecondary = chargedCooldown;
        Debug.Log($"[Elite Brute] Charged swing {clamped:0.00}s → {hits} hit(s), dmg {dmg:0}, radius {radius:0.0}");
    }

    // Shared cone damage
    private static int DoConeDamage(Transform owner, float radius, float halfAngleDeg, float damage, float knockback, LayerMask mask)
    {
        Vector3 origin  = owner.position;
        Vector3 forward = owner.forward;
        int count = 0;

        Collider[] hits = Physics.OverlapSphere(origin, radius, mask, QueryTriggerInteraction.Ignore);
        float minDot = Mathf.Cos(halfAngleDeg * Mathf.Deg2Rad);

        foreach (var h in hits)
        {
            Vector3 to = h.transform.position - origin; to.y = 0f;
            if (to.sqrMagnitude < 1e-4f) continue;
            if (Vector3.Dot(forward, to.normalized) < minDot) continue;

            // Damage using your existing APIs (same as Elite/Boss Dasher)
            if (h.TryGetComponent<EnemyController>(out var ec)) ec.TakeDamage(Mathf.RoundToInt(damage));
            else if (h.TryGetComponent<IDamageable>(out var id)) id.ApplyDamage(Mathf.RoundToInt(damage), h.ClosestPoint(origin), -forward);

            // Optional knockback
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
