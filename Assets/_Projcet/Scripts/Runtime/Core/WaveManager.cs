using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

    #region Events
    public static event Action OnWaveClear;

    private void OnEnable()
    {
        OnWaveClear += WaveCleared;
    }

    private void OnDisable()
    {
        OnWaveClear -= WaveCleared;     // was += by mistake
    }
    #endregion

    #region Serialized Fields
    [Header("Wave Settings")]
    [SerializeField] private List<WaveConfig> _waves = new();

    [Header("Spawn Settings")]
    [SerializeField] private List<Transform> _spawnPoints = new();
    #endregion

    #region Private Fields
    private int _currentWave = 0;
    private readonly List<GameObject> _activeEnemies = new();
    private float _currentTime;
    private bool _isNewWave;
    #endregion

    #region Unity Lifecycle
    private void Start()
    {
        _currentWave = 0;
        Debug.Log("[WaveManager] Ready. Press key to start first wave.");
    }

    private void Update()
    {
        // Check if wave is clear
        _activeEnemies.RemoveAll(e => e == null);
        if (_activeEnemies.Count == 0 && !_isNewWave && _currentWave > 0)
        {
            _isNewWave = true;
            OnWaveClear?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            StartNextWave();
        }


    }
    #endregion

    #region Wave Logic
    private void StartNextWave()
    {
        if (_currentWave >= _waves.Count)
        {
            Debug.Log("[WaveManager] No more waves defined.");
            return;
        }

        Debug.Log($"[WaveManager] Starting Wave #{_currentWave + 1}: {_waves[_currentWave].name}");

        var config = _waves[_currentWave];

        foreach (var spawn in config.Enemies)
        {
            for (int i = 0; i < spawn.Count; i++)
            {
                var enemy = Instantiate(spawn.Prefab, GetSpawnPoint(), Quaternion.identity);
                _activeEnemies.Add(enemy);
            }
        }
        _currentWave++;
        _isNewWave = false;
    }

    private void WaveCleared()
    {
        Debug.Log($"[WaveManager] Wave #{_currentWave} cleared!");
        // TODO: Add delay or UI prompt before next wave
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
