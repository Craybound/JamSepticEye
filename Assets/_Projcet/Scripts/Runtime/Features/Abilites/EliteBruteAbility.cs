using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[CreateAssetMenu(menuName="Abilities/Elite Brute")]
public class EliteBruteAbility : AbilitySO
{
    [Header("Basic Heavy (LMB)")]
    [SerializeField] private float damage = 18f;
    [SerializeField] private float radius = 2.0f;
    [SerializeField] private float halfAngle = 45f;
    [SerializeField] private float cooldown = 1.2f;
    [SerializeField] private LayerMask enemyMask;

    [Header("Charged Heavy (RMB)")]
    [SerializeField] private float minCharge = 0.2f;
    [SerializeField] private float maxCharge = 1.5f;
    [SerializeField] private float minDamage = 20f;
    [SerializeField] private float maxDamage = 45f;
    [SerializeField] private float chargedCooldown = 2.0f;

    private float _cdL, _cdR;
    private bool _charging;
    private readonly List<Collider> _buf = new();

    public override void OnPrimary(GameObject owner)
    {
        if (_cdL > 0f) return; 
        _cdL = cooldown;

        var aim = owner.GetComponent<PlayerAimSource>();
        Vector3 f = aim ? aim.Forward : owner.transform.forward;

        int n = CombatUtils.OverlapCone(owner.transform.position, f, radius, halfAngle, enemyMask, _buf);
        for (int i = 0; i < n; i++) CombatUtils.TryDamage(_buf[i].gameObject, damage);
    }

    public override void OnSecondary(GameObject owner)
    {
        if (_cdR > 0f || _charging) return;
        var host = owner.GetComponent<MonoBehaviour>();
        if (host) host.StartCoroutine(Charge(owner));
    }

    public override void Tick(GameObject owner, float dt)
    {
        if (_cdL > 0f) _cdL -= dt;
        if (_cdR > 0f) _cdR -= dt;
    }

    private IEnumerator Charge(GameObject owner)
    {
        _charging = true;
        float t = 0f;

        // hold RMB to charge
        while (UnityEngine.InputSystem.Mouse.current != null &&
               UnityEngine.InputSystem.Mouse.current.rightButton.isPressed &&
               t < maxCharge)
        {
            t += Time.deltaTime;
            yield return null;
        }
        _charging = false;

        float c   = Mathf.Clamp(Mathf.Max(minCharge, t), minCharge, maxCharge);
        float dmg = Mathf.Lerp(minDamage, maxDamage, (c - minCharge) / (maxCharge - minCharge));

        var aim = owner.GetComponent<PlayerAimSource>();
        Vector3 f = aim ? aim.Forward : owner.transform.forward;

        int n = CombatUtils.OverlapCone(owner.transform.position, f, radius, halfAngle, enemyMask, _buf);
        for (int i = 0; i < n; i++) CombatUtils.TryDamage(_buf[i].gameObject, dmg);

        _cdR = chargedCooldown;
    }
}
