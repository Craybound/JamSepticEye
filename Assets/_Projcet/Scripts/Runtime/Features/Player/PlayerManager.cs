using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    [Header("Player Stats")]
    [SerializeField] private float _maxHealth = 100f;
    [SerializeField, ReadOnly] private float _currentHealth;

    [SerializeField] private GameObject _player;
    [SerializeField] public HealthBarController healthBar;

    public static PlayerManager Instance { get; private set; }
    public static event Action<PlayerManager> OnReady;
    public static event Action<float, float> OnHealthChanged; // current, max
    public static event Action OnPlayerDeath;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        _player = GameObject.FindWithTag("Player");
        _currentHealth = _maxHealth;
        healthBar.SetMaxHealth(_maxHealth);

        OnReady?.Invoke(this);
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
    }

    private void OnEnable()
    {
        GameManager.OnStart += Init;
    }

    private void OnDisable()
    {
        GameManager.OnStart -= Init;
    }

    private void Init()
    {
        if (_player != null)
            _player.SetActive(true);
    }

    // === NEW ===
    public void TakeDamage(int amount)
    {
        _currentHealth -= amount;
        _currentHealth = Mathf.Clamp(_currentHealth, 0, _maxHealth);
        healthBar.SetHealth(_currentHealth);

        Debug.Log($"[PlayerManager] Player took {amount} damage! Current HP: {_currentHealth}");

        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);

        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        _currentHealth = Mathf.Clamp(_currentHealth + amount, 0, _maxHealth);
        OnHealthChanged?.Invoke(_currentHealth, _maxHealth);
        healthBar.SetHealth(_currentHealth);
    }

    private void Die()
    {
        Debug.Log("[PlayerManager] Player died!");
        OnPlayerDeath?.Invoke();

        // Add whatever you want to happen when player dies:
        // e.g., GameManager.Instance.SetState(GameState.GameOver);
    }
}
