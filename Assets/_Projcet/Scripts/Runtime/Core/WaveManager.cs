using System;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : MonoBehaviour
{


    private float _currentTime;

    //Wave
    [SerializeField] private List<WaveConfig> _waves;
    [SerializeField] private int _currentWave;
    [SerializeField] private List<GameObject> _activeEnemies = new();
    [SerializeField] private List<Transform> _spawnPoints;

    public static Action<WaveManager> OnReady { get; private set; }
    public static Action OnWaveStart { get; private set; }
    public static Action OnWaveClear { get; private set; }


    #region Singleton
    public static WaveManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            OnReady.Invoke(this);
            DontDestroyOnLoad(this);
        }
    }
    #endregion

    #region Event Handleing
    private void OnEnable()
    {
        OnWaveStart += StartNextWave;
        OnWaveClear += WaveCleared;
    }
    private void OnDisable()
    {
        OnWaveStart += StartNextWave;
        OnWaveClear += WaveCleared;
    }
    #endregion

    private void Start()
    {
        _currentWave = 0;
        OnWaveStart.Invoke();
    }

    private void Update()
    {
        if (_activeEnemies.Count == 0)
        {
            OnWaveClear.Invoke();
        }
    }



    private void StartNextWave()
    {
        _currentWave++;

        var config = _waves[_currentWave];
        foreach (var spawn in config.Enemies)
        {
            var enemy = Instantiate(spawn.Prefab, GetSpawnPoint(), Quaternion.identity);
            _activeEnemies.Add(enemy);
        }



    }

    private void WaveCleared()
    {

    }


    private Vector3 GetSpawnPoint()
    {
        return _spawnPoints[UnityEngine.Random.Range(0, _spawnPoints.Count)].position;
    }
}
