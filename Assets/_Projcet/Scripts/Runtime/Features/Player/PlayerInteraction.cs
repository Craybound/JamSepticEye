using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private float _interactionRadius = 3f;
    [SerializeField] private LayerMask _interactionMask;

    private IInteractable _currentTarget;

    private void Update()
    {
        FindClosestInteractable();
    }

    public void OnInteract(InputValue value)
    {
        if (value.isPressed && _currentTarget != null)
        {
            Debug.Log("Interacted");
            _currentTarget.Interact(gameObject);
        }
    }

    private void FindClosestInteractable()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, _interactionRadius, _interactionMask);

        _currentTarget = null;

        float closest = Mathf.Infinity;
        foreach (var hit in hits)
        {
            var interactable = hit.GetComponent<IInteractable>();
            if (interactable != null)
            {
                float dist = Vector3.Distance(transform.position, interactable.GetPosition());
                if (dist < closest)
                {
                    closest = dist;
                    _currentTarget = interactable;
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _interactionRadius);
    }
}
