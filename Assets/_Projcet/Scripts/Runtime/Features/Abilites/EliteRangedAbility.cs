using UnityEngine;

[CreateAssetMenu(menuName="Abilities/Elite Ranged")]
public class EliteRangedAbility : AbilitySO
{
    [Header("Projectile (LMB)")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private float projectileDamage = 8f;
    [SerializeField] private float projectileSpeed = 16f;
    [SerializeField] private float projectileCooldown = 0.35f;
    [SerializeField] private LayerMask enemyMask;

    [Header("Ground AoE (RMB)")]
    [SerializeField] private GroundAoeZone aoePrefab;
    [SerializeField] private float aoeCooldown = 3f;
    [SerializeField] private LayerMask groundMask;

    private float _cdL, _cdR;

    public override void OnPrimary(GameObject owner)
    {
        if (_cdL > 0f || !projectilePrefab) return; 
        _cdL = projectileCooldown;

        var aim = owner.GetComponent<PlayerAimSource>();
        Vector3 dir = aim ? aim.Forward : owner.transform.forward;

        Vector3 spawn = owner.transform.position + dir.normalized * 0.8f + Vector3.up * 0.6f;
        var rot = Quaternion.LookRotation(dir, Vector3.up);
        var p = Object.Instantiate(projectilePrefab, spawn, rot);
        p.Prime(projectileDamage, projectileSpeed, enemyMask);
    }

    public override void OnSecondary(GameObject owner)
    {
        if (_cdR > 0f || !aoePrefab) return; 
        _cdR = aoeCooldown;

        var cam = Camera.main; if (!cam) return;
        var ray = cam.ScreenPointToRay(UnityEngine.InputSystem.Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out var hit, 200f, groundMask))
            Object.Instantiate(aoePrefab, hit.point, Quaternion.identity);
    }

    public override void Tick(GameObject owner, float dt)
    {
        if (_cdL > 0f) _cdL -= dt;
        if (_cdR > 0f) _cdR -= dt;
    }
}
