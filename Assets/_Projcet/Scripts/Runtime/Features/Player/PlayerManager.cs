using System;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{

    [SerializeField] GameObject _player;



    #region Singleton
    public static PlayerManager Instance { get; private set; }
    public static event Action<PlayerManager> OnReady;

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
        _player = GameObject.FindWithTag("Player");
    }
    #endregion

    #region Event Setup

    private void OnEnable()
    {
        GameManager.OnStart += Init;
    }

    private void OnDisable()
    {
        GameManager.OnStart -= Init;

    }

    #endregion

    private void Init()
    {
        _player.SetActive(true);
    }






}
