using Sirenix.OdinInspector;
using UnityEngine;

public class EliteDasherInteractable : MonoBehaviour, IInteractable
{
    [Title("Ability Set To Grant")]
    [SerializeField, Required] private AbilitySO primaryAbility;
    [SerializeField, Required] private AbilitySO secondaryAbility;

    [Title("FX")]
    [SerializeField] private ParticleSystem pickupEffect;
    [SerializeField] private AudioClip pickupSound;

    public void Interact(GameObject interactor)
    {
        var controller = interactor.GetComponent<PlayerAbilityController>();
        if (controller == null) return;

        controller.SwapAbilities(primaryAbility, secondaryAbility);

        Debug.Log($"[EliteDasherInteractable] {interactor.name} equipped " +
                  $"{primaryAbility?.name} + {secondaryAbility?.name}");

        if (pickupEffect != null)
            Instantiate(pickupEffect, transform.position, Quaternion.identity);

        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);

        Destroy(gameObject); // consume the pickup
    }

    public Vector3 GetPosition() => transform.position;
}
