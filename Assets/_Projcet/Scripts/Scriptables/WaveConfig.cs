using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/Wave Config")]
public class WaveConfig : ScriptableObject
{
    public List<EnemySpawnData> Enemies;
}

[System.Serializable]
public struct EnemySpawnData
{
    public EnemyConfig Config;
    public int Count;
}
