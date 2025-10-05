using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM
    [RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        [Header("Player Movement")]
        public float MoveSpeed = 4.0f;
        public float SprintSpeed = 6.0f;
        public float RotationSpeed = 15.0f;
        public float SpeedChangeRate = 10.0f;

        [Header("Jumping & Gravity")]
        public float JumpHeight = 1.2f;
        public float Gravity = -15.0f;
        public float JumpTimeout = 0.50f;
        public float FallTimeout = 0.15f;

        [Header("Grounded")]
        public bool Grounded = true;
        public float GroundedOffset = -0.14f;
        public float GroundedRadius = 0.28f;
        public LayerMask GroundLayers;

        [Header("Camera / Mouse Raycast")]
        public GameObject CinemachineCameraTarget;
        public LayerMask MouseRaycastLayers = ~0; // everything by default

        [Header("Audio")]
        public AudioClip LandingAudioClip;
        public AudioClip[] FootstepAudioClips;
        [Range(0, 1)] public float FootstepAudioVolume = 0.5f;

        private float _speed;
        private float _animationBlend;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;

#if ENABLE_INPUT_SYSTEM
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private Camera _mainCamera;

        private const float _threshold = 0.01f;
        private bool _hasAnimator;

        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        private void Start()
        {
            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<StarterAssetsInputs>();
#if ENABLE_INPUT_SYSTEM
            _playerInput = GetComponent<PlayerInput>();
#endif
            AssignAnimationIDs();

            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;
        }

        private void Update()
        {
            _hasAnimator = TryGetComponent(out _animator);
            GroundedCheck();
            HandleJumpAndGravity();
            HandleMovement();
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
        }

        // ---------------- TOP-DOWN MOVEMENT ----------------
        private void HandleMovement()
        {
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;
            if (_input.move == Vector2.zero) targetSpeed = 0.0f;

            float currentSpeed = new Vector3(_controller.velocity.x, 0f, _controller.velocity.z).magnitude;
            _speed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * SpeedChangeRate);
            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // --- Top-down: move directly on world XZ plane ---
            Vector3 moveDirection = new Vector3(_input.move.x, 0f, _input.move.y);
            moveDirection.Normalize();

            // Apply movement
            _controller.Move(moveDirection * (_speed * Time.deltaTime) +
                             new Vector3(0f, _verticalVelocity, 0f) * Time.deltaTime);

            // Rotate player toward mouse position
            RotateTowardMouse();

            // Update animations
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, _input.move.magnitude);
            }
        }

        // ---------------- ROTATION TOWARD MOUSE ----------------
        private void RotateTowardMouse()
        {
#if ENABLE_INPUT_SYSTEM
            if (_mainCamera == null) return;

            Vector2 mousePos = Mouse.current != null
                ? Mouse.current.position.ReadValue()
                : new Vector2(Screen.width / 2f, Screen.height / 2f);

            Ray ray = _mainCamera.ScreenPointToRay(mousePos);
            if (Physics.Raycast(ray, out RaycastHit hit, 300f, MouseRaycastLayers))
            {
                Vector3 direction = hit.point - transform.position;
                direction.y = 0f;

                if (direction.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * RotationSpeed);
                }
            }
#endif
        }

        // ---------------- JUMP / GRAVITY ----------------
        private void HandleJumpAndGravity()
        {
            if (Grounded)
            {
                _fallTimeoutDelta = FallTimeout;

                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                if (_verticalVelocity < 0.0f)
                    _verticalVelocity = -2f;

                if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
                    if (_hasAnimator)
                        _animator.SetBool(_animIDJump, true);
                }

                if (_jumpTimeoutDelta >= 0.0f)
                    _jumpTimeoutDelta -= Time.deltaTime;
            }
            else
            {
                _jumpTimeoutDelta = JumpTimeout;

                if (_fallTimeoutDelta >= 0.0f)
                    _fallTimeoutDelta -= Time.deltaTime;
                else if (_hasAnimator)
                    _animator.SetBool(_animIDFreeFall, true);

                _input.jump = false;
            }

            if (_verticalVelocity < _terminalVelocity)
                _verticalVelocity += Gravity * Time.deltaTime;
        }

        // ---------------- GROUND CHECK ----------------
        private void GroundedCheck()
        {
            Vector3 spherePos = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
            Grounded = Physics.CheckSphere(spherePos, GroundedRadius, GroundLayers, QueryTriggerInteraction.Ignore);
            if (_hasAnimator)
                _animator.SetBool(_animIDGrounded, Grounded);
        }

        // ---------------- AUDIO EVENTS ----------------
        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
            {
                if (FootstepAudioClips.Length > 0)
                {
                    var i = Random.Range(0, FootstepAudioClips.Length);
                    AudioSource.PlayClipAtPoint(FootstepAudioClips[i],
                        transform.TransformPoint(_controller.center), FootstepAudioVolume);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (animationEvent.animatorClipInfo.weight > 0.5f)
                AudioSource.PlayClipAtPoint(LandingAudioClip,
                    transform.TransformPoint(_controller.center), FootstepAudioVolume);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Grounded ? new Color(0, 1, 0, 0.35f) : new Color(1, 0, 0, 0.35f);
            Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                              GroundedRadius);
        }
    }
}
