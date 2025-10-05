using UnityEngine;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    #region Runtime HUD

    [FoldoutGroup("Runtime HUD", expanded: true)]
    [HideLabel, ShowInInspector, GUIColor(0.8f, 0.9f, 1f)]
    [DisplayAsString(false)]
    private string _runtimeHeader = "Runtime Stats (Live Debug)";
    [InfoBox("These values are populated when the enemy spawns. You can tweak them in Play Mode for balancing.")]


    #region === HEALTH ===
    [FoldoutGroup("Runtime HUD/Health", expanded: true)]
    [ShowInInspector, ProgressBar(0, nameof(MaxHealth), ColorGetter = nameof(HealthBarColor), Height = 20)]
    [LabelText("Current HP"), ReadOnly]
    public int CurrentHealth => _currentHealth;

    [FoldoutGroup("Runtime HUD/Health")]
    [ShowInInspector, ReadOnly, LabelText("Max HP"), GUIColor(0.6f, 1f, 0.6f)]
    public int MaxHealth => _runtimeStats.MaxHealth;
    #endregion

    #region === COMBAT ===
    [FoldoutGroup("Runtime HUD/Combat", expanded: true)]
    [ShowInInspector, ReadOnly, LabelText("Damage"), GUIColor(1f, 0.7f, 0.2f), PropertyOrder(0)]
    public int Damage => _runtimeStats.Damage;

    [FoldoutGroup("Runtime HUD/Combat")]
    [ShowInInspector, ReadOnly, LabelText("Move Speed"), GUIColor(0.4f, 0.8f, 1f),PropertyOrder(1)]
    public float MoveSpeed => _runtimeStats.MoveSpeed;


    // === STAGGER (Subgroup under Combat) ===
    [FoldoutGroup("Runtime HUD/Combat/Stagger", expanded: true)]
    [PreviewField(Alignment = ObjectFieldAlignment.Left, Height = 50), PropertyOrder(2)]
    [LabelText("Stagger Indicator")]
    public UnityEngine.UI.Image _indicator;

    [FoldoutGroup("Runtime HUD/Combat/Stagger")]
    [ShowInInspector, LabelText("Is Interactable"), GUIColor(0.8f, 1f, 0.6f), PropertyOrder(3)]
    public bool IsInteractable { get; private set; } = false;

    [FoldoutGroup("Runtime HUD/Combat/Stagger")]
    [ShowInInspector, LabelText("Stagger Cooldown"), SuffixLabel("sec", Overlay = true), GUIColor(1f, 0.9f, 0.5f), PropertyOrder(4)]
    public float _staggerCooldown { get; private set; } = 5f;
    #endregion

    #region === LOOT ===
    [FoldoutGroup("Runtime HUD/Loot", expanded: true)]
    [ShowInInspector, ReadOnly, LabelText("Soul Drop"), GUIColor(0.9f, 0.5f, 1f)]
    public int SoulDrop => _runtimeStats.SoulDrop;
    #endregion

    #region === DEBUG ===
    [FoldoutGroup("Runtime HUD/Debug", expanded: false)]
    [ShowInInspector, ReadOnly, LabelText("Health Multiplier"), GUIColor(1f, 0.9f, 0.4f)]
    public float HealthMultiplier { get; private set; } = 1f;

    [FoldoutGroup("Runtime HUD/Debug")]
    [ShowInInspector, ReadOnly, LabelText("Damage Multiplier"), GUIColor(1f, 0.9f, 0.4f)]
    public float DamageMultiplier { get; private set; } = 1f;
    #endregion

    // === HEALTH BAR COLOR ===
    private Color HealthBarColor => Color.Lerp(Color.red, Color.green, (float)_currentHealth / Mathf.Max(1, MaxHealth));

    #endregion


    #region === Internal State ==============================

    private EnemyStats _runtimeStats;
    private int _currentHealth;
    public EnemyState state { get; private set; }

    public static event Action<GameObject> OnEnemyDeath;
    public event Action OnEnemyStagger;

    
    
    private GameObject _player;
    private NavMeshAgent _agent;




    #endregion


    #region === Unity Lifecycle ========================

    private void Awake() => _player = GameObject.FindWithTag("Player");

    private void Start()
    {
        if (_player == null)
            _player = GameObject.FindWithTag("Player");

        if (_agent == null)
            _agent = GetComponent<NavMeshAgent>();    


        state = EnemyState.Active;
        if(_indicator != null && !IsInteractable)
           _indicator.enabled = false;
    }
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


    #region === Initialization ========================

    public void Initialize(EnemyStats scaledStats, float hpMult = 1f, float dmgMult = 1f)
    {
        _runtimeStats = scaledStats;
        _currentHealth = _runtimeStats.MaxHealth;
        HealthMultiplier = hpMult;
        DamageMultiplier = dmgMult;
    }

    #endregion===========


    #region === Movement =====================

    private void Move()
    {
        if (_player == null || _agent == null)
            return;

        // Get distance between enemy and player
        float distance = Vector3.Distance(transform.position, _player.transform.position);

        // Optional: stop moving when close enough
        if (distance > 1.5f)
        {
            _agent.SetDestination(_player.transform.position);
        }
        else
        {
            _agent.ResetPath(); // stop movement when within attack range
        }
    }


    #endregion


    #region === Combat =======================

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
        _indicator.enabled = true;
        IsInteractable = true;
        _agent.isStopped = true;
        OnEnemyStagger?.Invoke();

        yield return new WaitForSeconds(_staggerCooldown);

        _indicator.enabled = false;
        _agent.isStopped = false;
        IsInteractable = !IsInteractable;
        state = EnemyState.Active;
    }

    public void Die()
    {
        Debug.Log($"{name} died and dropped {_runtimeStats.SoulDrop} souls.");
        OnEnemyDeath?.Invoke(gameObject);
        Destroy(gameObject);
    }

    #endregion


    #region Debug Tools
    [FoldoutGroup("Debug", expanded: true)]
    [HideLabel, ShowInInspector, DisplayAsString(false)]
    private readonly string _debugHeader = "Debug Tools";

    // Buttons
    [FoldoutGroup("Debug/Combat")]
    [ButtonGroup("Debug/Combat/Actions"), GUIColor(1f, 0.4f, 0.4f)]
    private void KillNow()
    {
        _currentHealth = 0;
        Die();
    }

    [FoldoutGroup("Debug/Combat")]
    [ButtonGroup("Debug/Combat/Actions"), GUIColor(0.4f, 1f, 0.4f)]
    private void HealFull() => _currentHealth = _runtimeStats.MaxHealth;

    [FoldoutGroup("Debug/Combat")]
    [ButtonGroup("Debug/Combat/Actions"), GUIColor(1f, 0.9f, 0.4f)]
    private void DamageSelf(int amount = 10) => TakeDamage(amount);

    [FoldoutGroup("Debug/Combat")]
    [ButtonGroup("Debug/Combat/Actions"), GUIColor(0.4f, 1f, 0.4f)]
    private void StaggerSelf()
    {
        _currentHealth = Mathf.RoundToInt(MaxHealth * 0.1f);
        StartCoroutine(EnemyStagger());
        OnEnemyStagger?.Invoke();
    }

    // Override fields
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

    public void Interact(GameObject interactor)
    {
        throw new NotImplementedException();
    }

    public Vector3 GetPosition()
    {
        throw new NotImplementedException();
    }
}
#endregion}
