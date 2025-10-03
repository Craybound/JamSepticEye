using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    #region Singleton
    public static WaveManager Instance { get; private set; }
    public static event Action<WaveManager> OnReady;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Prevent duplicates
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        OnReady?.Invoke(this);
    }
    #endregion

    #region Serialized Fields
    [Title("Wave Settings")]
    [InfoBox("Defines the sequence of waves. Each WaveConfig contains enemy spawns.")]
    [SerializeField] private List<WaveConfig> _waves = new();

    [Title("Spawn Settings")]
    [InfoBox("Points in the scene where enemies can spawn. One is chosen at random.")]
    [SerializeField] private List<Transform> _spawnPoints = new();
    #endregion

    #region Private Fields
    [ShowInInspector, ReadOnly, FoldoutGroup("Runtime State")]
    [LabelText("Current Wave")] private int _currentWave = 0;

    [ShowInInspector, ReadOnly, FoldoutGroup("Runtime State")]
    [LabelText("Active Enemies")] private readonly List<GameObject> _activeEnemies = new();

    [ShowInInspector, ReadOnly, FoldoutGroup("Runtime State")]
    [LabelText("Elapsed Time (s)")] private float _currentTime;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        _currentWave = 0;
        _currentTime = 0;
        Debug.Log("[WaveManager] Ready. Press debug buttons or call StartNextWave().");
    }

    private void Update()
    {
        _currentTime += Time.deltaTime;

#if UNITY_EDITOR
        // Debug Hotkeys
        if (Input.GetKeyDown(KeyCode.N)) StartNextWave();
        if (Input.GetKeyDown(KeyCode.C)) ForceClearEnemies();
#endif
    }
    #endregion

    #region Wave Logic
    /// <summary>
    /// Spawns the next wave of enemies from the config list.
    /// </summary>
    [Button(ButtonSizes.Medium), GUIColor(0.6f, 1f, 0.6f)]
    private void StartNextWave()
    {
        if (_currentWave >= _waves.Count)
        {
            Debug.Log("[WaveManager] No more waves defined.");
            return;
        }

        var config = _waves[_currentWave];
        Debug.Log($"[WaveManager] Starting Wave #{_currentWave + 1}: {config.name}");

        foreach (var spawn in config.Enemies)
        {
            for (int i = 0; i < spawn.Count; i++)
            {
                var enemyGO = Instantiate(spawn.Config.Prefab, GetSpawnPoint(), Quaternion.identity);
                var enemy = enemyGO.GetComponent<EnemyController>();

                if (enemy != null)
                {
                    // Apply scaling here if needed before Initialize()
                    var scaledStats = ScaleStats(spawn.Config.Stats, _currentWave, _currentTime);
                    enemy.Initialize(scaledStats);
                }

                _activeEnemies.Add(enemyGO);
            }
        }

        _currentWave++;
    }

    [Button(ButtonSizes.Medium), GUIColor(1f, 0.6f, 0.6f)]
    private void WaveCleared()
    {
        Debug.Log($"[WaveManager] Wave #{_currentWave} cleared!");
        StartNextWave(); // Optional: could delay or trigger UI prompt instead
    }

    /// <summary>
    /// Example difficulty scaling function (linear).
    /// Designers can tune growth rates here.
    /// </summary>
    private EnemyStats ScaleStats(EnemyStats baseStats, int waveNumber, float timeAlive)
    {
        float damageMultiplier = 1f + (waveNumber * 0.1f) + (timeAlive / 300f);
        float healthMultiplier = 1f + (waveNumber * 0.15f) + (timeAlive / 300f);

        return new EnemyStats
        {
            MaxHealth = Mathf.RoundToInt(baseStats.MaxHealth * healthMultiplier),
            Damage = Mathf.RoundToInt(baseStats.Damage * damageMultiplier),
            MoveSpeed = baseStats.MoveSpeed,
            SoulDrop = baseStats.SoulDrop
        };
    }
    #endregion

    #region Debug Helpers
    [Button(ButtonSizes.Medium), GUIColor(1f, 0.9f, 0.4f)]
    private void ForceClearEnemies()
    {
        if (_activeEnemies.Count > 0)
        {
            foreach (var go in _activeEnemies)
            {
                if (go != null) Destroy(go);
            }
            _activeEnemies.Clear();
            Debug.Log("[WaveManager] Enemies cleared manually.");
        }
        else
        {
            WaveCleared();
        }
    }

    [Button(ButtonSizes.Medium), GUIColor(0.6f, 0.8f, 1f)]
    private void SkipToWave(int waveNumber)
    {
        if (waveNumber < 0 || waveNumber >= _waves.Count)
        {
            Debug.LogWarning("[WaveManager] Invalid wave number.");
            return;
        }

        _currentWave = waveNumber;
        Debug.Log($"[WaveManager] Skipped to Wave #{_currentWave + 1}.");
        StartNextWave();
    }
    #endregion

    #region Helpers
    private Vector3 GetSpawnPoint()
    {
        if (_spawnPoints == null || _spawnPoints.Count == 0)
        {
            Debug.LogWarning("[WaveManager] No spawn points assigned!");
            return Vector3.zero;
        }

        return _spawnPoints[UnityEngine.Random.Range(0, _spawnPoints.Count)].position;
    }
    #endregion
}
