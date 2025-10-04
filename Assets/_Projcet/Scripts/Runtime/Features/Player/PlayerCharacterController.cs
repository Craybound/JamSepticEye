using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerCharacterController : MonoBehaviour
{
    [SerializeField] private CharacterController _controller;
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _gravity = -20f;

    [Header("Rotation")]
    [SerializeField] private float _turnSpeed = 15f; // Higher = snappier rotation
    [SerializeField] private LayerMask _groundMask;   // Assign "Ground" layer in Inspector
    [SerializeField] private PlayerMovementState _moveState; // To dynamically update the movement of the player
    private Vector2 _moveInput;
    private float _yVel;
    private Camera _mainCam;

    private void Awake()
    {
        if (_controller == null) _controller = GetComponent<CharacterController>();
        _mainCam = Camera.main;
        _moveState = GetComponent<PlayerMovementState>();
        if (_moveState == null) _moveState = gameObject.AddComponent<PlayerMovementState>();
    }

    // InputSystem callback
    public void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>();
        _moveState?.SetMoveInput(_moveInput); // To get the movement in world in real-time
    }
    private void Update()
    {
        HandleMovement();
        HandleRotation();
        Vector3 move = new Vector3(_moveInput.x, 0f, _moveInput.y);
        if (move.sqrMagnitude > 1f) move.Normalize();

        if (_controller.isGrounded && _yVel < 0f) _yVel = -2f;
        _yVel += _gravity * Time.deltaTime;

        Vector3 velocity = move * _speed + Vector3.up * _yVel;
        _controller.Move(velocity * Time.deltaTime);

        if (move.sqrMagnitude > 1e-6f)
        {
            Quaternion targetRot = Quaternion.LookRotation(move, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, _turnSpeed * Time.deltaTime);
        }
    }

    private void HandleMovement()
    {
        Vector3 move = new Vector3(_moveInput.x, 0f, _moveInput.y);
        if (move.sqrMagnitude > 1f) move.Normalize();

        // Gravity
        if (_controller.isGrounded && _yVel < 0f) _yVel = -2f;
        _yVel += _gravity * Time.deltaTime;

        Vector3 velocity = move * _speed + Vector3.up * _yVel;
        _controller.Move(velocity * Time.deltaTime);
    }

    private void HandleRotation()
    {
        if (_mainCam == null) return;

        // Get mouse position in screen space
        Vector2 mousePos = Mouse.current.position.ReadValue();

        // Ray from camera through mouse cursor
        Ray ray = _mainCam.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f, _groundMask))
        {
            Vector3 lookTarget = hit.point;
            lookTarget.y = transform.position.y; // keep rotation flat

            Vector3 direction = (lookTarget - transform.position).normalized;
            if (direction.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(direction, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, _turnSpeed * Time.deltaTime);
            }
        }
    }
}
