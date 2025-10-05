using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    #region Singleton
    public static GameManager Instance { get; private set; }
    public static event Action<GameManager> OnReady;
    public static event Action OnStart;

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


    public void StartGame()
    {
        OnStart?.Invoke();
    }




}
