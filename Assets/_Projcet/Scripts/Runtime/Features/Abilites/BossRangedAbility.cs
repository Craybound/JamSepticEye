using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "Abilities/Boss Ranged Marksman")]
public class BossRangedMarksmanAbility : AbilitySO
{
    // ===== Primary: single piercing shot =====
    [Header("Primary — Piercing Shot")]
    [SerializeField] private PiercingProjectile projectilePrefab;
    [SerializeField] private float shotDamage = 10f;
    [SerializeField] private float shotSpeed = 22f;
    [SerializeField] private int shotPierces = 4;
    [SerializeField] private float shotCooldown = 0.45f;

    [SerializeField] private Vector3 muzzleLocalOffset = new(0f, 0.6f, 0.9f);
    [SerializeField] private LayerMask enemyMask;

    [Tooltip("Aim toward mouse ground point if true; else use player's forward.")]
    [SerializeField] private bool useMouseAim = true;
    [SerializeField] private LayerMask groundMask;

    // ===== Secondary: volley (spread of piercing shots) =====
    [Header("Secondary — Volley")]
    [SerializeField] private PiercingProjectile volleyProjectilePrefab;
    [SerializeField] private int volleyCount = 5;      // number of projectiles
    [SerializeField] private float volleySpread = 28f; // degrees total arc
    [SerializeField] private float volleyCooldown = 3.2f;

    [SerializeField] private float volleyDamage = 10f;
    [SerializeField] private float volleySpeed = 22f;
    [SerializeField] private int volleyPierces = 3;

    // ===== Runtime =====
    private float _cdPrimary;
    private float _cdSecondary;

    public override void OnPrimary(GameObject owner)
    {
        if (_cdPrimary > 0f || projectilePrefab == null) return;
        _cdPrimary = shotCooldown;

        Vector3 dir = AimDirection(owner, useMouseAim);
        FireOne(owner, projectilePrefab, muzzleLocalOffset, dir, shotDamage, shotSpeed, shotPierces);
    }

    public override void OnSecondary(GameObject owner)
    {
        if (_cdSecondary > 0f || volleyProjectilePrefab == null) return;
        _cdSecondary = volleyCooldown;

        Vector3 baseDir = AimDirection(owner, useMouseAim);

        int n = Mathf.Max(1, volleyCount);
        float total = Mathf.Max(0f, volleySpread);
        float step = n > 1 ? total / (n - 1) : 0f;
        float start = -total * 0.5f;

        for (int i = 0; i < n; i++)
        {
            float ang = start + i * step;
            Quaternion rot = Quaternion.AngleAxis(ang, Vector3.up);
            Vector3 dir = (rot * baseDir).normalized;
            FireOne(owner, volleyProjectilePrefab, muzzleLocalOffset, dir, volleyDamage, volleySpeed, volleyPierces);
        }
    }

    public override void Tick(GameObject owner, float dt)
    {
        if (_cdPrimary   > 0f) _cdPrimary   -= dt;
        if (_cdSecondary > 0f) _cdSecondary -= dt;
    }

    // ===== helpers =====
    private void FireOne(GameObject owner, PiercingProjectile prefab, Vector3 localOffset, Vector3 dir, float dmg, float spd, int pierces)
    {
        Vector3 spawn = owner.transform.TransformPoint(localOffset);
        Quaternion rot = Quaternion.LookRotation(dir, Vector3.up);

        var proj = Object.Instantiate(prefab, spawn, rot);
        proj.Prime(dmg, spd, pierces, enemyMask, owner.transform);
    }

    private Vector3 AimDirection(GameObject owner, bool useMouse)
    {
        if (!useMouse) return owner.transform.forward;

        var cam = Camera.main;
        if (cam != null)
        {
            Vector2 m = Mouse.current.position.ReadValue();
            Ray r = cam.ScreenPointToRay(m);
            if (Physics.Raycast(r, out var hit, 500f, groundMask))
            {
                Vector3 to = hit.point - owner.transform.position; to.y = 0f;
                if (to.sqrMagnitude > 1e-4f) return to.normalized;
            }
        }
        return owner.transform.forward;
    }
}
