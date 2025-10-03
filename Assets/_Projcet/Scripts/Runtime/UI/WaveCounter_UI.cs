using TMPro;
using UnityEngine;

public class WaveCounter_UI : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI _currentWaveCount;
    [SerializeField] private TextMeshProUGUI _remainingEnemies;


    private void OnEnable()
    {
        WaveManager.OnNewWave += UpdateCounter;
        EnemyController.OnEnemyDeath += UpdateEnemyCounter;
    }
    private void OnDisable()
    {
        WaveManager.OnNewWave -= UpdateCounter;
        EnemyController.OnEnemyDeath -= UpdateEnemyCounter;
    }


    private void UpdateCounter()
    {
        var waveCount = WaveManager.Instance._currentWave;

        _currentWaveCount.text = "Current Wave : " + waveCount.ToString();
        UpdateEnemyCounter(null);

    }

    private void UpdateEnemyCounter(GameObject go)
    {
        var enemyCount = WaveManager.Instance._activeEnemies.Count - 1;
        _remainingEnemies.text = "Remaining Enemies" + enemyCount.ToString();

    }


}
