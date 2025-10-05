using UnityEngine;

[CreateAssetMenu(menuName="Abilities/Boss Ranged Marksman")]
public class BossRangedAbility : AbilitySO
{
    [Header("Piercing Shot (LMB)")]
    [SerializeField] private PiercingProjectile projectilePrefab;
    [SerializeField] private float damage = 10f;
    [SerializeField] private float speed = 20f;
    [SerializeField] private int pierces = 4;
    [SerializeField] private float cooldown = 0.45f;
    [SerializeField] private LayerMask enemyMask;

    [Header("Volley (RMB)")]
    [SerializeField] private PiercingProjectile volleyPrefab;
    [SerializeField] private int projectileCount = 5;    // spread count
    [SerializeField] private float spreadAngle = 25f;    // total arc
    [SerializeField] private float volleyCooldown = 3.2f;

    private float _cdL, _cdR;

    public override void OnPrimary(GameObject owner)
    {
        if (_cdL > 0f || !projectilePrefab) return; 
        _cdL = cooldown;

        Fire(owner, projectilePrefab, owner.GetComponent<PlayerAimSource>());
    }

    public override void OnSecondary(GameObject owner)
    {
        if (_cdR > 0f || !volleyPrefab) return; 
        _cdR = volleyCooldown;

        var aim = owner.GetComponent<PlayerAimSource>();
        Vector3 forward = aim ? aim.Forward : owner.transform.forward;

        int n = Mathf.Max(1, projectileCount);
        float step  = n > 1 ? spreadAngle / (n - 1) : 0f;
        float start = -spreadAngle * 0.5f;

        for (int i = 0; i < n; i++)
        {
            float ang = start + i * step;
            Quaternion rot = Quaternion.AngleAxis(ang, Vector3.up) * Quaternion.LookRotation(forward, Vector3.up);
            Fire(owner, volleyPrefab, rot * Vector3.forward);
        }
    }

    public override void Tick(GameObject owner, float dt)
    {
        if (_cdL > 0f) _cdL -= dt;
        if (_cdR > 0f) _cdR -= dt;
    }

    private void Fire(GameObject owner, PiercingProjectile prefab, PlayerAimSource aim)
        => Fire(owner, prefab, aim ? aim.Forward : owner.transform.forward);

    private void Fire(GameObject owner, PiercingProjectile prefab, Vector3 dir)
    {
        Vector3 spawn = owner.transform.position + dir.normalized * 0.8f + Vector3.up * 0.6f;
        var rot = Quaternion.LookRotation(dir, Vector3.up);
        var p = Object.Instantiate(prefab, spawn, rot);
        p.Prime(damage, speed, pierces, enemyMask);
    }
}
