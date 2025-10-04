using UnityEngine;
using Sirenix.OdinInspector;
using System;
using System.Collections;


public enum EnemyState { Active, Staggered }


public class EnemyController : MonoBehaviour
{
    #region Runtime HUD
    [Title("Runtime Stats (Live Debug)", bold: true)]
    [InfoBox("These values are populated when the enemy spawns. You can tweak them in Play Mode for balancing.")]

    [BoxGroup("Runtime HUD/Health", ShowLabel = false)]
    [ShowInInspector, ProgressBar(0, nameof(MaxHealth), ColorGetter = nameof(HealthBarColor), Height = 20)]
    [ReadOnly]
    public int CurrentHealth => _currentHealth;

    [BoxGroup("Runtime HUD/Health")]
    [ShowInInspector, ReadOnly, GUIColor(0.6f, 1f, 0.6f)]
    public int MaxHealth => _runtimeStats.MaxHealth;

    [BoxGroup("Runtime HUD/Combat")]
    [ShowInInspector, ReadOnly, GUIColor(1f, 0.7f, 0.2f)]
    public int Damage => _runtimeStats.Damage;

    [BoxGroup("Runtime HUD/Combat")]
    [ShowInInspector, ReadOnly, GUIColor(0.4f, 0.8f, 1f)]
    public float MoveSpeed => _runtimeStats.MoveSpeed;

    [BoxGroup("Runtime HUD/Combat")]
    [ShowInInspector, GUIColor(0.4f, 0.8f, 1f)]
    [SerializeField] public float _staggerCooldown { get; private set; } = 5f;


    [BoxGroup("Runtime HUD/Loot")]
    [ShowInInspector, ReadOnly, GUIColor(0.9f, 0.5f, 1f)]
    public int SoulDrop => _runtimeStats.SoulDrop;

    // Debug: multipliers applied from WaveManager
    [BoxGroup("Runtime HUD/Debug")]
    [ShowInInspector, ReadOnly, GUIColor(1f, 0.9f, 0.4f)]
    public float HealthMultiplier { get; private set; } = 1f;

    [BoxGroup("Runtime HUD/Debug")]
    [ShowInInspector, ReadOnly, GUIColor(1f, 0.9f, 0.4f)]
    public float DamageMultiplier { get; private set; } = 1f;

    private Color HealthBarColor => Color.Lerp(Color.red, Color.green, (float)_currentHealth / Mathf.Max(1, MaxHealth));
    #endregion

    #region Private State
    private EnemyStats _runtimeStats;
    private int _currentHealth;

    public EnemyState state { get; private set; }

    #endregion





    public static event Action<GameObject> OnEnemyDeath;
    public static event Action OnEnemyStagger;

    #region Unity Life Cycle

    private GameObject _player;
    private void Awake()
    {
        _player = GameObject.FindWithTag("Player");
    }

    private void Update()
    {
        switch (state)
        {
            default:
                return;

            case EnemyState.Active:
                Move();
                break;

            case EnemyState.Staggered:
                StartCoroutine(EnemyStagger());
                break;

        }


    }
    #endregion

    #region Initialization
    /// <summary>
    /// Must be called from WaveManager with pre-scaled stats.
    /// </summary>
    public void Initialize(EnemyStats scaledStats, float hpMult = 1f, float dmgMult = 1f)
    {
        _runtimeStats = scaledStats;
        _currentHealth = _runtimeStats.MaxHealth;

        HealthMultiplier = hpMult;
        DamageMultiplier = dmgMult;
    }
    #endregion

    #region Movement

    private void Move()
    {
        transform.LookAt(new Vector3(_player.transform.position.x, 0, _player.transform.position.z));
        transform.position = Vector3.MoveTowards(transform.position, _player.transform.position, MoveSpeed);
    }



    #endregion

    #region Combat
    public void TakeDamage(int amount)
    {
        _currentHealth -= amount;
        if (_currentHealth <= MaxHealth * 0.1f)
        {
            state = EnemyState.Staggered;
        }
        if (_currentHealth <= 0) Die();
    }

    private IEnumerator EnemyStagger()
    {
        OnEnemyStagger?.Invoke();
        yield return new WaitForSeconds(_staggerCooldown);
        state = EnemyState.Active;
    }




    private void Die()
    {
        Debug.Log($"{name} died and dropped {_runtimeStats.SoulDrop} souls.");
        OnEnemyDeath?.Invoke(gameObject);
        Destroy(gameObject);
    }
    #endregion

    #region Debug Tools
    [Title("Debug Tools", bold: true)]
    [InfoBox("Use these buttons in Play Mode to test interactions.")]

    [ButtonGroup("Debug/Combat"), GUIColor(1f, 0.4f, 0.4f)]
    private void KillNow()
    {
        _currentHealth = 0;
        Die();
    }

    [ButtonGroup("Debug/Combat"), GUIColor(0.4f, 1f, 0.4f)]
    private void HealFull()
    {
        _currentHealth = _runtimeStats.MaxHealth;
    }

    [ButtonGroup("Debug/Combat"), GUIColor(1f, 0.9f, 0.4f)]
    private void DamageSelf(int amount = 10)
    {
        TakeDamage(amount);
    }

    [ButtonGroup("Debug/Combat"), GUIColor(0.4f, 1f, 0.4f)]
    private void StaggerSelf()
    {
        _currentHealth = Mathf.RoundToInt(MaxHealth * 0.1f);
    }

    [FoldoutGroup("Debug/Overrides"), GUIColor(0.6f, 0.8f, 1f)]
    [Button("Reset To Defaults")]
    private void ResetStats()
    {
        _currentHealth = _runtimeStats.MaxHealth;
    }

    [FoldoutGroup("Debug/Overrides")]
    [ShowInInspector, GUIColor(1f, 0.7f, 0.2f)]
    [OnValueChanged(nameof(SetDamage))]
    private int OverrideDamage
    {
        get => _runtimeStats.Damage;
        set => _runtimeStats.Damage = Mathf.Max(1, value);
    }

    [FoldoutGroup("Debug/Overrides")]
    [ShowInInspector, GUIColor(0.6f, 1f, 0.6f)]
    [OnValueChanged(nameof(SetHealth))]
    private int OverrideMaxHealth
    {
        get => _runtimeStats.MaxHealth;
        set
        {
            _runtimeStats.MaxHealth = Mathf.Max(1, value);
            if (_currentHealth > _runtimeStats.MaxHealth)
                _currentHealth = _runtimeStats.MaxHealth;
        }
    }

    private void SetDamage() => Debug.Log($"{name}: Damage manually overridden to {_runtimeStats.Damage}");
    private void SetHealth() => Debug.Log($"{name}: MaxHealth manually overridden to {_runtimeStats.MaxHealth}");
    #endregion
}
