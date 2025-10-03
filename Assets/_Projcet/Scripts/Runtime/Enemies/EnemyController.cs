using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Runtime Stats (populated at spawn)")]
    [SerializeField] private EnemyStats _runtimeStats;  // Copy of config stats
    private int _currentHealth;

    #region API
    /// <summary>
    /// Initialize this enemy from an EnemyConfig. 
    /// Copies values into runtime stats (safe to tweak in Play Mode).
    /// </summary>
    public void Initialize(EnemyStats stats)
    {
        _runtimeStats = stats;              // copy from config
        _currentHealth = _runtimeStats.MaxHealth;
    }

    public void TakeDamage(int amount)
    {
        _currentHealth -= amount;
        if (_currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        Debug.Log($"{name} died and dropped {_runtimeStats.SoulDrop} souls.");
        Destroy(gameObject);
    }
    #endregion

    #region --- Properties for external systems ---
    public int CurrentHealth => _currentHealth;
    public int MaxHealth => _runtimeStats.MaxHealth;
    public float MoveSpeed => _runtimeStats.MoveSpeed;
    public int Damage => _runtimeStats.Damage;
    public int SoulDrop => _runtimeStats.SoulDrop;
    #endregion
}
