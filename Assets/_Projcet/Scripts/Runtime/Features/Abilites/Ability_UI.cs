using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
public class Ability_UI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image _indicator;

    [Header("Timing")]
    [SerializeField] private float _activeTime; // how long it stays visible

    private EnemyController _enemyController;

    private void Awake()
    {
        _enemyController = GetComponentInParent<EnemyController>();
    }


    private void Start()
    {
        _activeTime = _enemyController._staggerCooldown;
        _indicator.enabled = false;
    }

    private void OnEnable()
    {
        _enemyController.OnEnemyStagger += ActivateIndicator;
    }

    private void OnDisable()
    {
        _enemyController.OnEnemyStagger -= ActivateIndicator;
    }

    private void ActivateIndicator()
    {
        _indicator.enabled = true;
        StartCoroutine(IndicatorRuntime());
    }

    private IEnumerator IndicatorRuntime()
    {
        _indicator.enabled = true;

        yield return new WaitForSeconds(_activeTime);

        _indicator.enabled = false;
    }
}
