using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Shared helper methods for combat (hit detection, damage, knockback, etc.)
/// </summary>
public static class CombatUtils
{
    /// <summary>
    /// Returns all colliders within a forward cone.
    /// </summary>
    public static int OverlapCone(
        Vector3 origin,
        Vector3 forward,
        float radius,
        float halfAngleDeg,
        LayerMask mask,
        List<Collider> results,
        int max = 64)
    {
        results.Clear();

        Collider[] hits = Physics.OverlapSphere(origin, radius, mask, QueryTriggerInteraction.Ignore);
        float minDot = Mathf.Cos(halfAngleDeg * Mathf.Deg2Rad);

        foreach (var h in hits)
        {
            Vector3 to = h.transform.position - origin;
            to.y = 0f;

            if (to.sqrMagnitude < 1e-4f)
                continue;

            if (Vector3.Dot(forward, to.normalized) >= minDot)
                results.Add(h);

            if (results.Count >= max)
                break;
        }

        return results.Count;
    }

    /// <summary>
    /// Attempts to damage any object that implements IDamageable.
    /// </summary>
    public static void TryDamage(GameObject go, float dmg)
    {
        if (go == null || dmg <= 0) return;

        if (go.TryGetComponent<IDamageable>(out var id))
        {
            id.ApplyDamage(Mathf.RoundToInt(dmg), go.transform.position, Vector3.zero);
            return;
        }
        
        id = go.GetComponentInParent<IDamageable>();
        if (id != null)
        {
            id.ApplyDamage(Mathf.RoundToInt(dmg), go.transform.position, Vector3.zero);
            return;
        }

        // Optional: try children in case the root is passed but health is on a child
        id = go.GetComponentInChildren<IDamageable>();
        if (id != null)
        {
            id.ApplyDamage(Mathf.RoundToInt(dmg), go.transform.position, Vector3.zero);
        }
    }

    /// <summary>
    /// Applies a simple knockback force (if Rigidbody present).
    /// </summary>
    public static void TryKnockback(GameObject go, Vector3 dir, float force)
    {
        if (go == null) return;

        if (go.TryGetComponent<Rigidbody>(out var rb))
        {
            rb.AddForce(dir.normalized * force, ForceMode.VelocityChange);
        }
    }
}


