using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerCharacterController : MonoBehaviour
{
    [SerializeField] private CharacterController _controller;
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _gravity = -20f;

    [Header("Rotation")]
    [SerializeField] private float _turnSpeed = 15f; //Higher the number = snappier the rotation

    private Vector2 _moveInput;
    private float _yVel;

    private void Awake()
    {
        if (_controller == null) _controller = GetComponent<CharacterController>();
    }

    //PlayerInput callback
    public void OnMove(InputValue value) => _moveInput = value.Get<Vector2>();

    private void Update()
    {
        Vector3 move = new Vector3(_moveInput.x, 0f, _moveInput.y);
        if (move.sqrMagnitude > 1f) move.Normalize();

        //Gravity handling to make the player stay in the ground
        if (_controller.isGrounded && _yVel < 0f) _yVel = -2f;
        _yVel += _gravity * Time.deltaTime;

        Vector3 velocity = move * _speed + Vector3.up * _yVel;
        _controller.Move(velocity * Time.deltaTime);

        //This is for the rotation of the body on accordance with the movement
        if (move.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(move, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, _turnSpeed * Time.deltaTime);
        }
    }
}
