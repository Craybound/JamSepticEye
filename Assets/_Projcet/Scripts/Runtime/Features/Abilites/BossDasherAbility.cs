using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "Abilities/Boss Dasher Champion")]
public class BossDasherAbility : AbilitySO
{
    // ==========================
    // === Melee (same as elite)
    // ==========================
    [Header("=== Melee (Same as Elite) ===")]
    [SerializeField] private float meleeDamage = 10f;
    [SerializeField] private float meleeCooldown = 1f;

    // Optional only if you want fallback when WeaponHitbox not present:
    [SerializeField] private bool useFallbackHitCheck = false;
    [SerializeField] private float meleeRange = 1.5f;
    [SerializeField] private LayerMask enemyMask;

    private float _cooldownLeft; // melee

    // ==========================
    // === Multi-Dash (RMB)
    // ==========================
    [Header("=== Multi-Dash ===")]
    [Tooltip("How many dashes per use (spec: 2–3).")]
    [SerializeField, Range(1, 5)] private int dashesPerUse = 3;

    [Tooltip("Gap between chained dashes.")]
    [SerializeField] private float timeBetweenDashes = 0.08f;

    [Header("Dash Motion")]
    [Tooltip("Meters traveled per dash.")]
    [SerializeField] private float dashDistance = 3.5f;

    [Tooltip("Meters/second while dashing (used if dashDuration <= 0).")]
    [SerializeField] private float dashSpeed = 24f;

    [Tooltip("If > 0, use a fixed duration instead of speed.")]
    [SerializeField] private float dashDuration = 0.10f;

    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Pass-through Damage")]
    [SerializeField] private float dashDamage = 8f;
    [SerializeField] private float hitRadius = 0.8f;
    [SerializeField] private LayerMask dashEnemyMask;

    [Header("I-Frames & Cooldown")]
    [SerializeField] private float iFramesPerDash = 0.14f;
    [SerializeField] private float multiDashCooldown = 3.0f;

    private float _cooldownRight; // multi-dash
    private bool _isChaining;

    // =====================================================
    // PRIMARY (LMB): same as elite's melee swing behavior
    // =====================================================
    public override void OnPrimary(GameObject owner)
    {
        if (_cooldownLeft > 0f) return;
        _cooldownLeft = meleeCooldown;

        Debug.Log("[Boss Dasher] Melee swing triggered!");

        // Kick attack animation (same trigger name you use on elite)
        if (owner.TryGetComponent(out Animator anim))
            anim.SetTrigger("Attack");

        // Preferred: use your existing WeaponHitbox pipeline (same as EliteDasher)
        var hitbox = owner.GetComponentInChildren<WeaponHitbox>();
        if (hitbox != null)
        {
            hitbox.Init(owner, meleeDamage);
            return;
        }

        // Optional fallback if you want immediate hits without an animation event:
        if (useFallbackHitCheck)
        {
            owner.GetComponent<MonoBehaviour>()?.StartCoroutine(FallbackMelee(owner));
        }
    }

    private IEnumerator FallbackMelee(GameObject owner)
    {
        // tiny sync delay to feel like contact
        yield return new WaitForSeconds(0.1f);

        Vector3 origin = owner.transform.position + owner.transform.forward * (meleeRange * 0.5f);
        var hits = Physics.OverlapSphere(origin, meleeRange, enemyMask, QueryTriggerInteraction.Ignore);
        foreach (var h in hits)
        {
            if (h.CompareTag("Enemy"))
            {
                Debug.Log($"[Boss Dasher] Fallback melee hit {h.name} for {meleeDamage}");
                if (h.TryGetComponent<EnemyController>(out var ec)) ec.TakeDamage(Mathf.RoundToInt(meleeDamage));
                else if (h.TryGetComponent<IDamageable>(out var id)) id.ApplyDamage(Mathf.RoundToInt(meleeDamage), h.transform.position, Vector3.zero);
            }
        }
    }

    // =====================================================
    // SECONDARY (RMB): multi-dash with pass-through damage
    // =====================================================
    public override void OnSecondary(GameObject owner)
    {
        if (_cooldownRight > 0f || _isChaining) return;

        var host = owner.GetComponent<MonoBehaviour>();
        var cc   = owner.GetComponent<CharacterController>();
        if (host == null || cc == null) return;

        _cooldownRight = multiDashCooldown;
        _isChaining    = true;
        Debug.Log("[Boss Dasher] Multi-dash started");
        host.StartCoroutine(ChainRoutine(owner.transform, cc));
    }

    public override void Tick(GameObject owner, float dt)
    {
        if (_cooldownLeft  > 0f) _cooldownLeft  -= dt;
        if (_cooldownRight > 0f) _cooldownRight -= dt;
    }

    // ---- chain controller ----
    private IEnumerator ChainRoutine(Transform t, CharacterController cc)
    {
        for (int i = 0; i < dashesPerUse; i++)
        {
            yield return DashOnce(t, cc);
            if (i < dashesPerUse - 1) yield return new WaitForSeconds(timeBetweenDashes);
        }
        _isChaining = false;
        Debug.Log("[Boss Dasher] Multi-dash complete");
    }

    // ---- single dash with pass-through damage ----
    private IEnumerator DashOnce(Transform t, CharacterController cc)
    {
        // Direction: use last WASD move dir if available, else facing
        Vector3 dir = t.forward;
        if (t.TryGetComponent<PlayerMovementState>(out var moveState) && moveState.WorldMoveDir.sqrMagnitude > 1e-6f)
            dir = moveState.WorldMoveDir;
        dir.y = 0f; if (dir.sqrMagnitude < 1e-6f) dir = Vector3.forward;

        // i-frames during this dash
        //if (t.TryGetComponent<PlayerHurtbox>(out var hurt)) hurt.SetInvulnerable(iFramesPerDash);

        var hitThisDash = new HashSet<Collider>();
        float distanceTraveled = 0f;
        float tNorm = 0f;

        // Choose motion model: speed or duration
        if (dashDuration > 0f)
        {
            // duration-based dash with easing
            Vector3 moved = Vector3.zero;
            while (tNorm < 1f)
            {
                tNorm += Time.deltaTime / dashDuration;
                float stepNorm   = Mathf.Clamp01(ease.Evaluate(tNorm));
                float targetDist = dashDistance * stepNorm;
                float frameDist  = targetDist - moved.magnitude;

                if (frameDist > 0f)
                {
                    Vector3 delta = dir.normalized * frameDist;
                    cc.Move(delta);
                    moved += delta;
                }

                PassThroughDamage(t.position, hitRadius, dashEnemyMask, hitThisDash);
                yield return null;
            }
        }
        else
        {
            // speed-based dash
            while (distanceTraveled < dashDistance)
            {
                float step = dashSpeed * Time.deltaTime;
                cc.Move(dir.normalized * step);
                distanceTraveled += step;

                PassThroughDamage(t.position, hitRadius, dashEnemyMask, hitThisDash);
                yield return null;
            }
        }

        // little tail sweep to catch edge cases
        PassThroughDamage(t.position, hitRadius, dashEnemyMask, hitThisDash);
    }

    private static void PassThroughDamage(Vector3 pos, float radius, LayerMask mask, HashSet<Collider> alreadyHit)
    {
        var hits = Physics.OverlapSphere(pos, radius, mask, QueryTriggerInteraction.Ignore);
        foreach (var h in hits)
        {
            if (alreadyHit.Contains(h)) continue;
            alreadyHit.Add(h);

            // Apply damage via your existing enemy API
            if (h.TryGetComponent<EnemyController>(out var ec)) ec.TakeDamage(8); // will be overridden below
            else if (h.TryGetComponent<IDamageable>(out var id)) id.ApplyDamage(8, h.transform.position, Vector3.zero);
        }
    }
}
