using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "Abilities/Elite Ranged")]
public class EliteRangedAbility : AbilitySO
{
    // ========== PRIMARY: Single Projectile ==========
    [Header("Primary — Projectile")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private float projectileDamage = 8f;
    [SerializeField] private float projectileSpeed = 16f;
    [SerializeField] private float projectileCooldown = 0.35f;
    [SerializeField] private Vector3 muzzleLocalOffset = new Vector3(0, 0.6f, 0.8f); // spawn in front of player
    [SerializeField] private LayerMask enemyMask;

    // If true, aim at mouse ground point; else use owner's forward.
    [SerializeField] private bool useMouseAimForPrimary = true;
    [SerializeField] private LayerMask groundMask; // for mouse aim raycast

    // ========== SECONDARY: Ground AoE ==========
    [Header("Secondary — Ground AoE")]
    [SerializeField] private GroundAoEZone aoePrefab;
    [SerializeField] private float aoeWindup = 0.6f;
    [SerializeField] private float aoeDuration = 3f;
    [SerializeField] private float aoeTickInterval = 0.5f;
    [SerializeField] private float aoeDamagePerTick = 4f;
    [SerializeField] private float aoeRadius = 2.6f;
    [SerializeField] private float aoeCooldown = 3.0f;

    // ========== Runtime ==========
    private float _cdPrimary;
    private float _cdSecondary;

    public override void OnPrimary(GameObject owner)
    {
        if (_cdPrimary > 0f || projectilePrefab == null) return;
        _cdPrimary = projectileCooldown;

        // Decide direction
        Vector3 dir = owner.transform.forward;
        if (useMouseAimForPrimary)
        {
            var cam = Camera.main;
            if (cam != null)
            {
                Vector2 m = Mouse.current.position.ReadValue();
                Ray r = cam.ScreenPointToRay(m);
                if (Physics.Raycast(r, out var hit, 500f, groundMask))
                {
                    Vector3 to = (hit.point - owner.transform.position); to.y = 0f;
                    if (to.sqrMagnitude > 1e-4f) dir = to.normalized;
                }
            }
        }

        // Spawn position a bit in front of player
        Vector3 spawn = owner.transform.TransformPoint(muzzleLocalOffset);
        var rot = Quaternion.LookRotation(dir, Vector3.up);

        var p = Object.Instantiate(projectilePrefab, spawn, rot);
        p.Prime(projectileDamage, projectileSpeed, enemyMask);
    }

    public override void OnSecondary(GameObject owner)
    {
        if (_cdSecondary > 0f || aoePrefab == null) return;

        var cam = Camera.main;
        if (cam == null) return;

        // Place AoE where mouse points on ground
        Vector2 m = Mouse.current.position.ReadValue();
        Ray r = cam.ScreenPointToRay(m);
        if (!Physics.Raycast(r, out var hit, 500f, groundMask)) return;

        _cdSecondary = aoeCooldown;

        var aoe = Object.Instantiate(aoePrefab, hit.point, Quaternion.identity);
        aoe.Configure(aoeWindup, aoeDuration, aoeTickInterval, aoeDamagePerTick, aoeRadius, enemyMask);
    }

    public override void Tick(GameObject owner, float deltaTime)
    {
        if (_cdPrimary   > 0f) _cdPrimary   -= deltaTime;
        if (_cdSecondary > 0f) _cdSecondary -= deltaTime;
    }
}
