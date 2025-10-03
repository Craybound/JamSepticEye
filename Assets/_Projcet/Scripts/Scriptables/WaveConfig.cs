using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Wave Config")]
public class WaveConfig : ScriptableObject
{
    [Header("Enemy Spawns")]
    [Tooltip("List of enemies that will spawn in this wave (used for normal waves or boss reinforcements).")]
    public List<EnemySpawnData> Enemies;

    [Header("Boss Settings")]
    [Tooltip("Is this wave a boss wave? If true, spawns only the boss at first.")]
    public bool IsBossWave;

    [Tooltip("Boss enemy to spawn if this is a boss wave.")]
    public EnemyConfig BossEnemy;

    [Tooltip("Delay in seconds before reinforcement enemies spawn if boss is still alive.")]
    public float ReinforcementDelay = 10f;
}

[System.Serializable]
public struct EnemySpawnData
{
    [Tooltip("Enemy type to spawn.")]
    public EnemyConfig Config;

    [Tooltip("How many of this enemy should spawn.")]
    public int Count;
}
