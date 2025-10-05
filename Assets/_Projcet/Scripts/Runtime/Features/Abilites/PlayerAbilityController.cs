using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerAbilityController : MonoBehaviour
{
    [Header("Abilities")]
    [SerializeField] private AbilitySO _primaryAbility;
    [SerializeField] private AbilitySO _secondaryAbility;
    [SerializeField] private Image primaryImage;
    [SerializeField] private string primaryDesc;
    [SerializeField] private string primaryName;
    [SerializeField] private Image secondaryImage;
    [SerializeField] private string secondaryDesc;
    [SerializeField] private string secondaryName;

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
        SetPrimaryInfo();
        _secondaryAbility = newSecondary;
        SetSecondaryInfo();

        Debug.Log($"[AbilityController] Equipped: {newPrimary?.AbilityName} / {newSecondary?.AbilityName}");
    }

    public void SetPrimaryInfo()
    {
        primaryImage = _primaryAbility.Icon;
        primaryDesc = _primaryAbility.Description;
        primaryName = _primaryAbility.AbilityName;
    }

    public void SetSecondaryInfo()
    {
        secondaryImage = _secondaryAbility.Icon;
        secondaryDesc = _secondaryAbility.Description;
        secondaryName = _secondaryAbility.AbilityName;
    }
}
