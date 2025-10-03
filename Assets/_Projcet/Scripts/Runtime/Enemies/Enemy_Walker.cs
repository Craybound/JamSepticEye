using UnityEngine;

public class Enemy_Walker : MonoBehaviour
{
    [SerializeField] float _moveSpeed = 5f;
    private GameObject _player;

    private void Awake()
    {
        _player = GameObject.FindWithTag("Player");
        transform.LookAt(new Vector3(_player.transform.position.x, 0, _player.transform.position.z));
    }


    private void Update()
    {
       transform.position = Vector3.MoveTowards(transform.position, _player.transform.position, _moveSpeed);
    }





}
