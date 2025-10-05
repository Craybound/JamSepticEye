using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class AbilityInteractable : MonoBehaviour, IInteractable
{

    [SerializeField] private EnemyController enemy;

    [Title("Ability Set To Grant")]
    [SerializeField, Required] private AbilitySO primaryAbility;
    [SerializeField, Required] private AbilitySO secondaryAbility;

    [Title("FX")]
    [SerializeField] private ParticleSystem pickupEffect;
    [SerializeField] private AudioClip pickupSound;


    public static event Action<EnemyController,AbilitySO, AbilitySO> OnInteracted;

    void Start()
    {
        enemy = GetComponent<EnemyController>();
    }


    public void Interact(GameObject interactor)
    {
        Debug.Log($"[Ability Interactable] {gameObject.name}");

        if (enemy.IsInteractable)
        {
            OnInteracted?.Invoke(enemy,GetPrimary(), GetSecondary());
        }

        Debug.Log($"[EliteDasherInteractable] {interactor.name} equipped " +
                  $"{primaryAbility?.name} + {secondaryAbility?.name}");

        if (pickupEffect != null)
            Instantiate(pickupEffect, transform.position, Quaternion.identity);

        if (pickupSound != null)
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
    }

    public Vector3 GetPosition() => transform.position;

    public AbilitySO GetPrimary() => primaryAbility;
    public AbilitySO GetSecondary() => secondaryAbility; 



}
