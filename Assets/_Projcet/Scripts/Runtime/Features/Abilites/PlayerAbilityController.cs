using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAbilityController : MonoBehaviour
{
    [Header("Abilities")]
    [SerializeField] private AbilitySO _primaryAbility;
    [SerializeField] private AbilitySO _secondaryAbility;

    private void Update()
    {
        _primaryAbility?.Tick(gameObject, Time.deltaTime);
        _secondaryAbility?.Tick(gameObject, Time.deltaTime);
    }

    public void OnPrimary(InputValue value)
    {
        if (value.isPressed)
        {
            Debug.Log("[Input] Primary fired!");
            _primaryAbility?.OnPrimary(gameObject);
        }
    }

    public void OnSecondary(InputValue value)
    {
        if (value.isPressed)
        {
            Debug.Log("[Input] Secondary fired!");
            _secondaryAbility?.OnSecondary(gameObject);
        }
    }

    public void SwapAbilities(AbilitySO newPrimary, AbilitySO newSecondary)
    {
        _primaryAbility = newPrimary;
        _secondaryAbility = newSecondary;
        Debug.Log($"[AbilityController] Equipped: {newPrimary?.AbilityName} / {newSecondary?.AbilityName}");
    }
}
