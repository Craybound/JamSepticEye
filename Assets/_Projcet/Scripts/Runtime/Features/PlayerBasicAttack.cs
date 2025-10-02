using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class PlayerBasicAttack : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private Transform _attackOrigin;      //The weapon which attacks
    [SerializeField] private float _attackRange = 1.8f;    //Range of the attacks in meter
    [Tooltip("Dot of angle between player forward and direction to target. 1=0°, 0=90°, -1=180°. e.g. 0.3 ≈ 72° cone")]
    [SerializeField] private float _attackArcDot = 0.3f;   //Bigger the value = narrower the cone will be
    [SerializeField] private LayerMask _enemyMask;          //Set to enemy layer. This is to mark the enemy

    [Header("Timing")]
    [SerializeField] private float _attackCooldown = 0.35f; //Cooldown for us if needed in seconds
    private float _nextAttackTime = 0f;

    private void Awake()
    {
        if (_attackOrigin == null) _attackOrigin = transform;
    }
    public void OnAttack(InputValue value)
    {
        if (!value.isPressed) return;

        //To respect the cooldown
        if (Time.time < _nextAttackTime) return;
        _nextAttackTime = Time.time + _attackCooldown;

        Debug.Log("player attacked");

        //Checking for enemies in range and in front the arc
        Vector3 origin = _attackOrigin.position;
        Vector3 forward = transform.forward;


        //Colliders in a sphere shape. Can be changed if we want to.
        var hits = Physics.OverlapSphere(origin, _attackRange, _enemyMask, QueryTriggerInteraction.Ignore);
        if (hits != null && hits.Length > 0)
        {
            //To find anying that are inside the front of the arc
            bool facingAny = hits.Any(h =>
            {
                Vector3 to = h.transform.position - origin;
                to.y = 0f;
                if (to.sqrMagnitude < 0.0001f) return false;
                float dot = Vector3.Dot(forward, to.normalized);
                return dot >= _attackArcDot;
            });

            if (facingAny)
            {
                Debug.Log("basic damage applied");
                //Later we have to update with actual functionality
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_attackOrigin == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(_attackOrigin.position, _attackRange);
        Gizmos.DrawLine(_attackOrigin.position, _attackOrigin.position + transform.forward * _attackRange);
    }
#endif
}
