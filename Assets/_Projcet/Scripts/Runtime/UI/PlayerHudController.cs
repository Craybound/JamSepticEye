using UnityEngine;
using UnityEngine.UI;

public class PlayerHudController : MonoBehaviour
{
    [SerializeField] private GameObject _interactionUI;
    [SerializeField] private Button _killButton;
    [SerializeField] private Button _consumeButton;

    [SerializeField]private PlayerAbilityController controller;


    private void OnEnable()
    {
        AbilityInteractable.OnInteracted += ShowInteractionUI;
    }

    private void OnDisable()
    {
        AbilityInteractable.OnInteracted -= ShowInteractionUI;
    }


    private void Awake()
    {
        if( _interactionUI != null )
        _interactionUI.SetActive(false);
        if (controller == null)
        {
            controller = GameObject.FindWithTag("Player").GetComponent<PlayerAbilityController>();
        }

    }

    public void ShowInteractionUI(EnemyController enemy,AbilitySO primary, AbilitySO secondary)
    {
        _interactionUI.SetActive(true);

        if (_consumeButton != null)
        {
            _consumeButton.onClick.RemoveAllListeners();
            _consumeButton.onClick.AddListener(() =>
            {
                controller.SwapAbilities(primary, secondary);
                enemy.Die();
                _interactionUI.SetActive(false);
                Time.timeScale = 1f; // Resume time after choosing
            });
        }

        _killButton.onClick.RemoveAllListeners();
        _killButton.onClick.AddListener(() =>
        {
            _interactionUI.SetActive(false);
            enemy.Die();
            Time.timeScale = 1f;
        });




        Time.timeScale = 0f; // Pause game while interaction UI is active
    }







}
