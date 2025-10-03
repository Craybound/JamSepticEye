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
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        OnReady?.Invoke(this);
    }
    #endregion

    #region Wave & Spawn Settings
    [Title("Wave Settings")]
    [InfoBox("Defines the sequence of waves. Each WaveConfig contains enemy spawns.")]
    [SerializeField] private List<WaveConfig> _waves = new();

    [Title("Spawn Settings")]
    [InfoBox("Points in the scene where enemies can spawn. One is chosen at random.")]
    [SerializeField] private List<Transform> _spawnPoints = new();
    #endregion

    #region Scaling Settings
    [TitleGroup("Scaling Settings", Alignment = TitleAlignments.Centered)]
    [InfoBox("These values define how enemy stats scale as waves progress or as time passes.\n" +
             "Designers: tweak here to adjust difficulty curve.")]

    [HorizontalGroup("Scaling Settings/Split", Width = 120)]
    [VerticalGroup("Scaling Settings/Split/Left")]
    [LabelText("Time Scale (sec/unit)"), Range(1f, 120f)]
    [GUIColor(0.6f, 0.8f, 1f)]
    [SerializeField] private float _timeScale = 30f;

    [VerticalGroup("Scaling Settings/Split/Right")]
    [LabelText("Damage Growth (per wave)"), Range(0f, 0.5f)]
    [GUIColor(1f, 0.7f, 0.2f)]
    [SerializeField] private float _damagePerWave = 0.1f;

    [VerticalGroup("Scaling Settings/Split/Right")]
    [LabelText("Health Growth (per wave)"), Range(0f, 0.5f)]
    [GUIColor(0.6f, 1f, 0.6f)]
    [SerializeField] private float _healthPerWave = 0.15f;

    [VerticalGroup("Scaling Settings/Split/Right")]
    [LabelText("Damage Growth (per min)"), Range(0f, 0.5f)]
    [GUIColor(1f, 0.9f, 0.4f)]
    [SerializeField] private float _damagePerMinute = 0.05f;

    [VerticalGroup("Scaling Settings/Split/Right")]
    [LabelText("Health Growth (per min)"), Range(0f, 0.5f)]
    [GUIColor(0.9f, 1f, 0.4f)]
    [SerializeField] private float _healthPerMinute = 0.05f;
    #endregion

    #region Runtime State
    [FoldoutGroup("Runtime State"), ShowInInspector, ReadOnly]
    [LabelText("Current Wave")] private int _currentWave = 0;

    [FoldoutGroup("Runtime State"), ShowInInspector, ReadOnly]
    [LabelText("Active Enemies")] private readonly List<GameObject> _activeEnemies = new();

    [FoldoutGroup("Runtime State"), ShowInInspector, ReadOnly]
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
        if (Input.GetKeyDown(KeyCode.N)) StartNextWave();
        if (Input.GetKeyDown(KeyCode.C)) ForceClearEnemies();
#endif
    }
    #endregion

    #region Wave Logic
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
                    var scaledStats = ScaleStats(spawn.Config.Stats, _currentWave, _currentTime);
                    enemy.Initialize(scaledStats,
                        1f + (_currentWave * _healthPerWave) + (_currentTime / 60f * _healthPerMinute),
                        1f + (_currentWave * _damagePerWave) + (_currentTime / 60f * _damagePerMinute));
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
        StartNextWave();
    }

    private EnemyStats ScaleStats(EnemyStats baseStats, int waveNumber, float timeAlive)
    {
        float healthMult = 1f + (waveNumber * _healthPerWave) + ((timeAlive / 60f) * _healthPerMinute);
        float damageMult = 1f + (waveNumber * _damagePerWave) + ((timeAlive / 60f) * _damagePerMinute);

        return new EnemyStats
        {
            MaxHealth = Mathf.RoundToInt(baseStats.MaxHealth * healthMult),
            Damage = Mathf.RoundToInt(baseStats.Damage * damageMult),
            MoveSpeed = baseStats.MoveSpeed,
            SoulDrop = baseStats.SoulDrop
        };
    }
    #endregion

    #region Debug Tools
    [Button(ButtonSizes.Medium), GUIColor(1f, 0.9f, 0.4f)]
    private void ForceClearEnemies()
    {
        if (_activeEnemies.Count > 0)
        {
            foreach (var go in _activeEnemies)
                if (go != null) Destroy(go);

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

    [Button(ButtonSizes.Medium), GUIColor(0.9f, 0.7f, 1f)]
    private void PreviewScaling(int waveNumber, float timeMinutes = 0)
    {
        var baseStats = _waves[0].Enemies[0].Config.Stats;
        var preview = ScaleStats(baseStats, waveNumber, timeMinutes * 60f);

        Debug.Log($"[WaveManager] Preview Wave {waveNumber} at {timeMinutes} min -> " +
                  $"HP {preview.MaxHealth}, Damage {preview.Damage}");
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
