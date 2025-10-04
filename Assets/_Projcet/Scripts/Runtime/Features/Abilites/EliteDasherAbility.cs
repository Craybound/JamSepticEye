using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(menuName = "Abilities/Elite Dasher Ability")]
public class EliteDasherAbility : AbilitySO
{
    [Header("Melee Attack")]
    [SerializeField] private float meleeDamage = 10f;
    [SerializeField] private float meleeRange = 1.5f;
    [SerializeField] private float meleeCooldown = 1f;

    [Header("Dash")]
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashCooldown = 2f;
    [SerializeField] private int maxDashCharges = 1;
    [SerializeField] private float iframeDuration = 0.25f;

    // Runtime state
    private float _cooldownLeft;
    private float _cooldownRight;
    private int _currentCharges;

    public override void OnPrimary(GameObject owner)
    {
        if (_cooldownLeft > 0) return;

        Debug.Log("[Elite Dasher] Melee Swing!");

        Collider[] hits = Physics.OverlapSphere(owner.transform.position + owner.transform.forward * meleeRange, 1f);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy")) // or "Player"
            {
                Debug.Log("Hit target with melee!");
                // TODO: Apply damage
            }
        }

        _cooldownLeft = meleeCooldown;
    }

    public override void OnSecondary(GameObject owner)
    {
        if (_currentCharges <= 0)
        {
            Debug.Log("[Elite Dasher] No dash charges left!");
            return;
        }

        _currentCharges--;
        Debug.Log("[Elite Dasher] Dashing!");

        var controller = owner.GetComponent<CharacterController>();
        var host = owner.GetComponent<MonoBehaviour>();

        // figure out dash direction (towards mouse, flattened)
        Vector3 dashDir = owner.transform.forward; // fallback if raycast fails

        Camera cam = Camera.main;
        if (cam != null)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            Ray ray = cam.ScreenPointToRay(mousePos);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f))
            {
                Vector3 lookTarget = hit.point;
                lookTarget.y = owner.transform.position.y; // keep on ground plane
                dashDir = (lookTarget - owner.transform.position).normalized;
            }
        }

        // dash movement
        if (controller != null && host != null)
        {
            host.StartCoroutine(DashCoroutine(controller, dashDir));
        }
        else
        {
            owner.transform.position += dashDir * dashDistance;
        }

        // start i-frames
        if (host != null)
            host.StartCoroutine(GrantIFrames(owner));

        _cooldownRight = dashCooldown;
    }


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
    }

    public override void Tick(GameObject owner, float deltaTime)
    {
        if (_cooldownLeft > 0) _cooldownLeft -= deltaTime;
        if (_cooldownRight > 0) _cooldownRight -= deltaTime;

        if (_currentCharges < maxDashCharges && _cooldownRight <= 0)
        {
            _currentCharges++;
            _cooldownRight = dashCooldown;
        }
    }

    private IEnumerator GrantIFrames(GameObject owner)
    {
        Debug.Log("I-Frames active!");
        yield return new WaitForSeconds(iframeDuration);
        Debug.Log("I-Frames ended.");
    }
}
