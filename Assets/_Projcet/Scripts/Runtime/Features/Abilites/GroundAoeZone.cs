using UnityEngine;
using System.Collections;

/// <summary>
/// Ground AoE that appears, waits (windup), then deals periodic damage in an area.
/// </summary>
public class GroundAoeZone : MonoBehaviour
{
    [Header("AoE Config")]
    [SerializeField] private float windup = 0.6f;          // delay before first damage tick
    [SerializeField] private float duration = 3f;          // total lifetime
    [SerializeField] private float tickInterval = 0.5f;    // how often it deals damage
    [SerializeField] private float damagePerTick = 4f;
    [SerializeField] private float radius = 2.5f;
    [SerializeField] private LayerMask enemyMask;

    private void OnEnable() => StartCoroutine(Run());

    private IEnumerator Run()
    {
        // Optional: play telegraph VFX during windup
        yield return new WaitForSeconds(windup);

        float elapsed = 0f;
        float tickTimer = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            tickTimer += Time.deltaTime;

            if (tickTimer >= tickInterval)
            {
                tickTimer = 0f;
                var hits = Physics.OverlapSphere(transform.position, radius, enemyMask, QueryTriggerInteraction.Ignore);
                foreach (var h in hits)
                    CombatUtils.TryDamage(h.gameObject, damagePerTick);
            }

            yield return null;
        }

        Destroy(gameObject);
    }
}

