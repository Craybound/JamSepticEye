using UnityEngine;
using Sirenix.OdinInspector;
using System;
using System.Collections;

public enum EnemyState { Active, Staggered }

public class EnemyController : MonoBehaviour
{
    #region ??? Runtime HUD ?????????????????????????????????????????????

    [Title("Runtime Stats (Live Debug)", bold: true)]
    [InfoBox("These values are populated when the enemy spawns. You can tweak them in Play Mode for balancing.")]

    [BoxGroup("Health")]
    [ShowInInspector, ProgressBar(0, nameof(MaxHealth), ColorGetter = nameof(HealthBarColor), Height = 20)]
    [ReadOnly]
    public int CurrentHealth => _currentHealth;

    [BoxGroup("Health")]
    [ShowInInspector, ReadOnly, GUIColor(0.6f, 1f, 0.6f)]
    public int MaxHealth => _runtimeStats.MaxHealth;

    [BoxGroup("Combat")]
    [ShowInInspector, ReadOnly, GUIColor(1f, 0.7f, 0.2f)]
    public int Damage => _runtimeStats.Damage;

    [BoxGroup("Combat")]
    [ShowInInspector, ReadOnly, GUIColor(0.4f, 0.8f, 1f)]
    public float MoveSpeed => _runtimeStats.MoveSpeed;

    [BoxGroup("Combat")]
    [ShowInInspector, GUIColor(0.4f, 0.8f, 1f)]
    public float _staggerCooldown { get; private set; } = 5f;

    [BoxGroup("Loot")]
    [ShowInInspector, ReadOnly, GUIColor(0.9f, 0.5f, 1f)]
    public int SoulDrop => _runtimeStats.SoulDrop;

    [BoxGroup("Debug")]
    [ShowInInspector, ReadOnly, GUIColor(1f, 0.9f, 0.4f)]
    public float HealthMultiplier { get; private set; } = 1f;

    [BoxGroup("Debug")]
    [ShowInInspector, ReadOnly, GUIColor(1f, 0.9f, 0.4f)]
    public float DamageMultiplier { get; private set; } = 1f;

    private Color HealthBarColor => Color.Lerp(Color.red, Color.green, (float)_currentHealth / Mathf.Max(1, MaxHealth));

    #endregion


    #region ??? Internal State ?????????????????????????????????????????????

    private EnemyStats _runtimeStats;
    private int _currentHealth;
    public EnemyState state { get; private set; }

    public static event Action<GameObject> OnEnemyDeath;
    public event Action OnEnemyStagger;

    private GameObject _player;

    #endregion


    #region ??? Unity Lifecycle ?????????????????????????????????????????????

    private void Awake() => _player = GameObject.FindWithTag("Player");

    private void Start() => state = EnemyState.Active;

    private void Update()
    {
        switch (state)
        {
            case EnemyState.Active:
                Move();
                break;

            case EnemyState.Staggered:
                StartCoroutine(EnemyStagger());
                break;
        }
    }

    #endregion


    #region ??? Initialization ?????????????????????????????????????????????

    public void Initialize(EnemyStats scaledStats, float hpMult = 1f, float dmgMult = 1f)
    {
        _runtimeStats = scaledStats;
        _currentHealth = _runtimeStats.MaxHealth;
        HealthMultiplier = hpMult;
        DamageMultiplier = dmgMult;
    }

    #endregion


    #region ??? Movement ?????????????????????????????????????????????

    private void Move()
    {
        transform.LookAt(new Vector3(_player.transform.position.x, 0, _player.transform.position.z));
        transform.position = Vector3.MoveTowards(transform.position, _player.transform.position, MoveSpeed * Time.deltaTime);
    }

    #endregion


    #region ??? Combat ?????????????????????????????????????????????

    public void TakeDamage(int amount)
    {
        _currentHealth -= amount;
        if (_currentHealth <= MaxHealth * 0.1f)
            state = EnemyState.Staggered;

        if (_currentHealth <= 0)
            Die();
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


    #region ??? Debug Tools ?????????????????????????????????????????????

    [Title("Debug Tools", bold: true)]
    [ButtonGroup("Debug/Combat"), GUIColor(1f, 0.4f, 0.4f)]
    private void KillNow()
    {
        _currentHealth = 0;
        Die();
    }

    [ButtonGroup("Debug/Combat"), GUIColor(0.4f, 1f, 0.4f)]
    private void HealFull() => _currentHealth = _runtimeStats.MaxHealth;

    [ButtonGroup("Debug/Combat"), GUIColor(1f, 0.9f, 0.4f)]
    private void DamageSelf(int amount = 10) => TakeDamage(amount);

    [ButtonGroup("Debug/Combat"), GUIColor(0.4f, 1f, 0.4f)]
    private void StaggerSelf()
    {
        _currentHealth = Mathf.RoundToInt(MaxHealth * 0.1f);
        OnEnemyStagger?.Invoke();
    }

    [FoldoutGroup("Debug/Overrides"), GUIColor(0.6f, 0.8f, 1f)]
    [Button("Reset To Defaults")]
    private void ResetStats() => _currentHealth = _runtimeStats.MaxHealth;

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
            _currentHealth = Mathf.Min(_currentHealth, _runtimeStats.MaxHealth);
        }
    }

    private void SetDamage() => Debug.Log($"{name}: Damage manually overridden to {_runtimeStats.Damage}");
    private void SetHealth() => Debug.Log($"{name}: MaxHealth manually overridden to {_runtimeStats.MaxHealth}");

    #endregion
}
