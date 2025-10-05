using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(menuName="Abilities/Boss Dasher (Multi Dash)")]
public class BossDasherAbility : AbilitySO
{
    // ----------- Melee (Primary) -----------
    [Header("Melee (Primary)")]
    [SerializeField] private float meleeDamage = 10f;
    [SerializeField] private float meleeRadius = 1.5f;
    [SerializeField] private float meleeHalfAngle = 60f;     // small cone in front
    [SerializeField] private float meleeCooldown = 0.5f;
    [SerializeField] private LayerMask enemyMask;

    private float _meleeCd;
    private readonly List<Collider> _buf = new();

    public override void OnPrimary(GameObject owner)
    {
        if (_meleeCd > 0f) return;
        _meleeCd = meleeCooldown;

        // Aim source (sword/visual forward) or fallback to root forward
        var aim = owner.GetComponent<PlayerAimSource>();
        Vector3 fwd = aim ? aim.Forward : owner.transform.forward;

        int count = CombatUtils.OverlapCone(owner.transform.position, fwd, meleeRadius, meleeHalfAngle, enemyMask, _buf);
        for (int i = 0; i < count; i++)
            CombatUtils.TryDamage(_buf[i].gameObject, meleeDamage);
    }

    // ----------- Multi-dash (Secondary) -----------
    [Header("Dash Chain (Secondary)")]
    [SerializeField] private int dashesPerUse = 3;          // 2–3
    [SerializeField] private float timeBetweenDashes = 0.08f;
    [SerializeField] private float dashDistance = 3.5f;
    [SerializeField] private float dashDuration = 0.10f;
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0,0,1,1);

    [Header("Pass-through Damage")]
    [SerializeField] private float hitRadius = 0.8f;
    [SerializeField] private float dashDamage = 8f;
    [SerializeField] private LayerMask dashEnemyMask;

    [Header("I-frames & Cooldown")]
    [SerializeField] private float iFramesEach = 0.14f;
    [SerializeField] private float dashCooldown = 3f;

    private float _dashCd;

    public override void OnSecondary(GameObject owner)
    {
        if (_dashCd > 0f) return;
        _dashCd = dashCooldown;

        var cc = owner.GetComponent<CharacterController>();
        var host = owner.GetComponent<MonoBehaviour>();
        if (!cc || !host) return;

        host.StartCoroutine(Chain(owner.transform, cc));
    }

    public override void Tick(GameObject owner, float dt)
    {
        if (_meleeCd > 0f) _meleeCd -= dt;
        if (_dashCd  > 0f) _dashCd  -= dt;
    }

    private IEnumerator Chain(Transform t, CharacterController cc)
    {
        var aim = t.GetComponent<PlayerAimSource>();
        var move = t.GetComponent<PlayerMovementState>();

        for (int i = 0; i < dashesPerUse; i++)
        {
            Vector3 dir =
                (move && move.WorldMoveDir.sqrMagnitude > 1e-6f) ? move.WorldMoveDir :
                (aim  ? aim.Forward : t.forward);

            dir.y = 0f;
            if (dir.sqrMagnitude < 1e-6f) dir = Vector3.forward;

            yield return DashOnce(t, cc, dir.normalized);

            if (i < dashesPerUse - 1)
                yield return new WaitForSeconds(timeBetweenDashes);
        }
    }

    private IEnumerator DashOnce(Transform t, CharacterController cc, Vector3 dir)
    {
        float time = 0f;
        Vector3 moved = Vector3.zero;
        var hitset = new HashSet<Collider>();

        while (time < 1f)
        {
            time += Time.deltaTime / dashDuration;
            float stepN     = Mathf.Clamp01(ease.Evaluate(time));
            float targetDst = dashDistance * stepN;
            float frameDst  = targetDst - moved.magnitude;

            if (frameDst > 0f)
            {
                Vector3 delta = dir * frameDst;
                cc.Move(delta);
                moved += delta;

                // pass-through damage (hit each collider once per dash)
                var hits = Physics.OverlapSphere(t.position, hitRadius, dashEnemyMask, QueryTriggerInteraction.Ignore);
                foreach (var h in hits)
                    if (hitset.Add(h)) CombatUtils.TryDamage(h.gameObject, dashDamage);
            }
            yield return null;
        }
    }
}
