using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAbilityController : MonoBehaviour
{
    [Header("Abilities")]
    [SerializeField] private AbilitySO _primaryAbility;
    [SerializeField] private AbilitySO _secondaryAbility;
    public AudioSource audio;

    private void Awake()
    {
        // clone starting abilities so runtime state is not shared with asset
        if (_primaryAbility)   _primaryAbility   = Instantiate(_primaryAbility);
        if (_secondaryAbility) _secondaryAbility = Instantiate(_secondaryAbility);
    }
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
            audio.PlayOneShot(_primaryAbility.primarySfx, 0.7f);
        }
    }

    public void OnSecondary(InputValue value)
    {
        if (value.isPressed)
        {
            Debug.Log("[Input] Secondary fired!");
            _secondaryAbility?.OnSecondary(gameObject);
            audio.PlayOneShot(_secondaryAbility.secondarySfx, 0.7f);
        }
    }

    public void SwapAbilities(AbilitySO newPrimary, AbilitySO newSecondary)
    {
        _primaryAbility   = newPrimary   ? Instantiate(newPrimary)   : null;
        _secondaryAbility = newSecondary ? Instantiate(newSecondary) : null;

        Debug.Log($"[AbilityController] Equipped: {newPrimary?.AbilityName} / {newSecondary?.AbilityName}");
    }
}
