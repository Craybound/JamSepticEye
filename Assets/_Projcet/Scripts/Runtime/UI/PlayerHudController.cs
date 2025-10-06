using UnityEngine;
using UnityEngine.UI;
using TMPro; // use TMP if you prefer

public class PlayerHudController : MonoBehaviour
{
    [Header("UI Root & Buttons")]
    [SerializeField] private GameObject _interactionUI;
    [SerializeField] private Button _killButton;
    [SerializeField] private Button _consumeButton;
    [SerializeField] private PlayerAbilityController controller;

    [Header("Ability Display Texts")]
    [SerializeField] private TMP_Text _primaryNameText;
    [SerializeField] private TMP_Text _secondaryNameText;
    [SerializeField] private TMP_Text _primaryDescText;
    [SerializeField] private TMP_Text _secondaryDescText;

    [Header("Ability Icons (Optional)")]
    [SerializeField] private Image _primaryIcon;
    [SerializeField] private Image _secondaryIcon;

    private void OnEnable()  => AbilityInteractable.OnInteracted += ShowInteractionUI;
    private void OnDisable() => AbilityInteractable.OnInteracted -= ShowInteractionUI;

    private void Awake()
    {
        if (_interactionUI != null)
            _interactionUI.SetActive(false);

        if (controller == null)
            controller = GameObject.FindWithTag("Player")?.GetComponent<PlayerAbilityController>();
    }

    public void ShowInteractionUI(EnemyController enemy, AbilitySO primary, AbilitySO secondary)
    {
        _interactionUI.SetActive(true);

        // ===== TOP BOXES: Ability Names =====
        if (_primaryNameText)
            _primaryNameText.text = primary ? primary.AbilityName : "—";

        if (_secondaryNameText)
            _secondaryNameText.text = secondary ? secondary.AbilityName : "—";

        // ===== BOTTOM BOXES: Descriptions =====
        if (_primaryDescText)
            _primaryDescText.text = primary ? primary.Description : "";

        if (_secondaryDescText)
            _secondaryDescText.text = secondary ? secondary.Description : "";

        // ===== ICONS (optional) =====
        if (_primaryIcon)
        {
            _primaryIcon.sprite = primary && primary.Icon != null ? primary.Icon.sprite : null;
            _primaryIcon.enabled = _primaryIcon.sprite != null;
        }

        if (_secondaryIcon)
        {
            _secondaryIcon.sprite = secondary && secondary.Icon != null ? secondary.Icon.sprite : null;
            _secondaryIcon.enabled = _secondaryIcon.sprite != null;
        }

        // ===== BUTTONS =====
        if (_consumeButton)
        {
            _consumeButton.onClick.RemoveAllListeners();
            _consumeButton.onClick.AddListener(() =>
            {
                controller.SwapAbilities(primary, secondary);
                enemy.Die();
                CloseUI();
            });
        }

        if (_killButton)
        {
            _killButton.onClick.RemoveAllListeners();
            _killButton.onClick.AddListener(() =>
            {
                enemy.Die();
                CloseUI();
            });
        }

        Time.timeScale = 0f; // pause game while choosing
    }

    private void CloseUI()
    {
        if (_interactionUI) _interactionUI.SetActive(false);
        Time.timeScale = 1f;
    }
}
