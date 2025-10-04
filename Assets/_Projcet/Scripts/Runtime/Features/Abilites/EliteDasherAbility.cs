using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "Abilities/Elite Dasher Ability")]
public class EliteDasherAbility : AbilitySO
{
    // -----------------------------------------------------
    #region === Melee Settings ===
    [Header("=== Melee Attack ===")]
    [Tooltip("Damage dealt per melee swing.")]
    [SerializeField] private float meleeDamage = 10f;

    [Tooltip("Effective range of the melee hit check.")]
    [SerializeField] private float meleeRange = 1.5f;

    [Tooltip("Cooldown between melee attacks (seconds).")]
    [SerializeField] private float meleeCooldown = 1f;

    [Tooltip("Delay between pressing attack and hit registering (to sync with animation).")]
    [SerializeField] private float meleeHitDelay = 0.3f;

    [Tooltip("Enemy detection layer mask for melee hits.")]
    [SerializeField] private LayerMask enemyMask;
    #endregion


    // -----------------------------------------------------
    #region === Dash Settings ===
    [Header("=== Dash ===")]
    [Tooltip("Maximum dash distance in meters.")]
    [SerializeField] private float dashDistance = 5f;

    [Tooltip("Dash travel speed in meters/second.")]
    [SerializeField] private float dashSpeed = 20f;

    [Tooltip("Cooldown between dash activations.")]
    [SerializeField] private float dashCooldown = 2f;

    [Tooltip("Maximum number of dash charges.")]
    [SerializeField] private int maxDashCharges = 1;

    [Tooltip("Duration of invulnerability during dash (seconds).")]
    [SerializeField] private float iframeDuration = 0.25f;
    #endregion


    // -----------------------------------------------------
    #region === Runtime State ===
    private float _cooldownLeft;      // Melee cooldown timer
    private float _cooldownRight;     // Dash cooldown timer
    private int _currentCharges;      // Remaining dash charges
    #endregion


    // -----------------------------------------------------
    #region === Primary (Melee Attack) ===
    public override void OnPrimary(GameObject owner)
    {
        if (_cooldownLeft > 0)
            return;

        _cooldownLeft = meleeCooldown;
        Debug.Log("[Elite Dasher] Melee swing triggered!");

        // Play attack animation if the player has an Animator
        var anim = owner.GetComponent<Animator>();
        if (anim != null)
            anim.SetTrigger("Attack");

        // Initialize the hitbox for the attack (handled by animation events)
        var hitbox = owner.GetComponentInChildren<WeaponHitbox>();
        if (hitbox != null)
            hitbox.Init(owner, meleeDamage);

        // Optional fallback if no animation is set up (old coroutine-based damage check)
        var host = owner.GetComponent<MonoBehaviour>();
        if (host != null)
            host.StartCoroutine(MeleeHitDelayed(owner));
    }

    /// <summary>
    /// Legacy fallback for hit timing if animation events aren't set up yet.
    /// </summary>
    private IEnumerator MeleeHitDelayed(GameObject owner)
    {
        yield return new WaitForSeconds(meleeHitDelay);

        Vector3 origin = owner.transform.position + owner.transform.forward * meleeRange * 0.5f;
        Collider[] hits = Physics.OverlapSphere(origin, meleeRange, enemyMask);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Debug.Log($"[Elite Dasher] Hit {hit.name} for {meleeDamage} damage!");
                
                hit.TryGetComponent<EnemyController>(out EnemyController enemy);
                if (enemy != null)
                    enemy.TakeDamage((int)meleeDamage);
            }
        }
    }
    #endregion


    // -----------------------------------------------------
    #region === Secondary (Dash Ability) ===
    public override void OnSecondary(GameObject owner)
    {
        if (_currentCharges <= 0)
        {
            Debug.Log("[Elite Dasher] No dash charges left!");
            return;
        }

        _currentCharges--;
        _cooldownRight = dashCooldown;
        Debug.Log("[Elite Dasher] Dash started!");

        // // --- Determine dash direction (mouse-based aim) ---
        // Vector3 dashDir = owner.transform.forward;
        // Camera cam = Camera.main;

        // if (cam != null)
        // {
        //     Vector2 mousePos = Mouse.current.position.ReadValue();
        //     Ray ray = cam.ScreenPointToRay(mousePos);

        //     if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        //     {
        //         Vector3 target = hit.point;
        //         target.y = owner.transform.position.y;
        //         dashDir = (target - owner.transform.position).normalized;
        //     }
        // }

        // // --- Execute dash ---
        // var controller = owner.GetComponent<CharacterController>();
        // var host = owner.GetComponent<MonoBehaviour>();

        // if (controller != null && host != null)
        // {
        //     host.StartCoroutine(DashCoroutine(controller, dashDir));
        //     host.StartCoroutine(GrantIFrames(owner));
        // }
        // else
        // {
        //     // Fallback: instant teleport if controller missing
        //     owner.transform.position += dashDir * dashDistance;
        // }
        
        // --- Determine dash direction from movement state (WASD) ---
        Vector3 dashDir = owner.transform.forward; // fallback
        var moveState = owner.GetComponent<PlayerMovementState>();
        if (moveState != null && moveState.WorldMoveDir.sqrMagnitude > 1e-6f)
            dashDir = moveState.WorldMoveDir;

        // --- Execute dash ---
        var controller = owner.GetComponent<CharacterController>();
        var host = owner.GetComponent<MonoBehaviour>();

        if (controller != null && host != null)
        {
            host.StartCoroutine(DashCoroutine(controller, dashDir.normalized));
            host.StartCoroutine(GrantIFrames(owner));
        }
        else
        {
            owner.transform.position += dashDir.normalized * dashDistance;
        }
    }

    /// <summary>
    /// Smooth movement during dash using CharacterController.
    /// </summary>
    private IEnumerator DashCoroutine(CharacterController controller, Vector3 direction)
    {
        float distanceTraveled = 0f;

        while (distanceTraveled < dashDistance)
        {
            float step = dashSpeed * Time.deltaTime;
            controller.Move(direction * step);
            distanceTraveled += step;
            yield return null;
        }

        Debug.Log("[Elite Dasher] Dash complete.");
    }

    /// <summary>
    /// Temporary invulnerability during dash.
    /// </summary>
    private IEnumerator GrantIFrames(GameObject owner)
    {
        Debug.Log("[Elite Dasher] I-Frames active.");
        // TODO: Disable player damage intake here
        yield return new WaitForSeconds(iframeDuration);
        Debug.Log("[Elite Dasher] I-Frames ended.");
        // TODO: Re-enable player damage intake
    }
    #endregion


    // -----------------------------------------------------
    #region === Runtime Tick (Cooldown + Recharge) ===
    public override void Tick(GameObject owner, float deltaTime)
    {
        // Reduce active cooldowns
        if (_cooldownLeft > 0) _cooldownLeft -= deltaTime;
        if (_cooldownRight > 0) _cooldownRight -= deltaTime;

        // Recharge dash charges over time
        if (_currentCharges < maxDashCharges && _cooldownRight <= 0)
        {
            _currentCharges++;
            _cooldownRight = dashCooldown;
        }
    }
    #endregion
}
