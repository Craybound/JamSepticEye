using System;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyConfig", menuName = "Scriptable Objects/New EnemyConfig")]
public class EnemyConfig : ScriptableObject
{
    public GameObject Prefab;
    public EnemyStats Stats;
}
