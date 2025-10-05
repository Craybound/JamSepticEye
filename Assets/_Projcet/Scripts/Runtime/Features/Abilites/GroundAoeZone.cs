using UnityEngine;
using System.Collections;

public class GroundAoEZone : MonoBehaviour
{
    [SerializeField] private float windup = 0.6f;
    [SerializeField] private float duration = 3f;
    [SerializeField] private float tickInterval = 0.5f;
    [SerializeField] private float damagePerTick = 4f;
    [SerializeField] private float radius = 2.5f;
    [SerializeField] private LayerMask enemyMask;
    [SerializeField] private GameObject telegraphVfx;
    [SerializeField] private GameObject activeVfx;

    private Coroutine _run;

    private void OnEnable() { _run = StartCoroutine(Run()); }
    private void OnDisable() { if (_run != null) StopCoroutine(_run); }

    public void Configure(float windupSec, float dur, float tick, float dmg, float r, LayerMask mask)
    {
        windup = windupSec; duration = dur; tickInterval = tick; damagePerTick = dmg; radius = r; enemyMask = mask;
    }

    private IEnumerator Run()
    {
        // Telegraph phase
        if (telegraphVfx) telegraphVfx.SetActive(true);
        if (activeVfx)    activeVfx.SetActive(false);
        if (windup > 0f)  yield return new WaitForSeconds(windup);

        // Active phase
        if (telegraphVfx) telegraphVfx.SetActive(false);
        if (activeVfx)    activeVfx.SetActive(true);

        float t = 0f, tick = 0f;
        while (t < duration)
        {
            t += Time.deltaTime; tick += Time.deltaTime;
            if (tick >= tickInterval)
            {
                tick = 0f;
                var hits = Physics.OverlapSphere(transform.position, radius, enemyMask, QueryTriggerInteraction.Ignore);
                foreach (var h in hits)
                {
                    if (h.TryGetComponent(out EnemyController ec))
                        ec.TakeDamage(Mathf.RoundToInt(damagePerTick));
                    else if (h.TryGetComponent(out IDamageable id))
                        id.ApplyDamage(Mathf.RoundToInt(damagePerTick), h.ClosestPoint(transform.position), Vector3.up);
                }
            }
            yield return null;
        }
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
#endif
}
