using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    #region Variables: Movement

    private Vector2 _movementInput;
    private Vector2 _lookInput;
    private CharacterController _characterController;
    private float _originalHeight;
    private Vector3 _direction;
    private bool _isJumping;
    private bool _isSprinting;
    private bool _isCrouching;
    [SerializeField] private float speed = 5f;

    #endregion

    #region Variables: Rotation

    [SerializeField] private float lookSpeed = .1f;
    private Transform playerCamera;
    [SerializeField] private float sprintMultiplier = 2f;
    [SerializeField] private float crouchHeight = 0.5f;
    [SerializeField] private float crouchSpeed = 2.5f;
    private float _pitch = 0f;

    #endregion

    #region Variables: Gravity

    private float _gravity = -9.81f;
    [SerializeField] private float gravityMultiplier = 3.0f;
    private float _velocity;

    #endregion

    #region Variables: Jumping

    [SerializeField] private float jumpPower = 5f;

    #endregion

    private void Awake()
    {
        _characterController = GetComponent<CharacterController>();
        _originalHeight = _characterController.height;

    }

    void Start(){
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        playerCamera = Camera.main.transform;
    }
    void Update()
    {
        ApplyGravity();
        ApplyRotation();
        ApplyMovement();
        ApplyJump();
    }

    private void ApplyGravity()
    {
        if (IsGrounded() && _velocity < 0.0f)
        {
            _velocity = -1.0f; // Small negative value to keep grounded
        }
        else
        {
            _velocity += _gravity * gravityMultiplier * Time.deltaTime;
        }

        _direction.y = _velocity;
    }

    private void ApplyRotation()
    {
        // Mouse look
        float mouseX = _lookInput.x * lookSpeed;
        float mouseY = _lookInput.y * lookSpeed;

        _pitch -= mouseY;
        _pitch = Mathf.Clamp(_pitch, -89f, 89f);

        playerCamera.localRotation = Quaternion.Euler(_pitch, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    // private void ApplyMovement()
    // {
    //     // Basic movement
    //     Vector3 move = transform.right * _movementInput.x + transform.forward * _movementInput.y;
    //     _characterController.Move(move * speed * Time.deltaTime + _direction * Time.deltaTime);
    // }
    private void ApplyMovement()
    {
        // Get direction and speed based on crouch/sprint state
        float currentSpeed = _isSprinting ? speed * sprintMultiplier : speed;

        if (_isCrouching)
        {
            _characterController.height = crouchHeight;
            currentSpeed = crouchSpeed;
        }
        else
        {
            _characterController.height = _originalHeight;
        }

        // Only modify the horizontal components of _direction (x and z), preserving vertical movement (y)
        Vector3 move = transform.right * _movementInput.x + transform.forward * _movementInput.y;

        // Preserve the y component (_direction.y) for gravity and jumping
        _direction.x = move.x * currentSpeed;
        _direction.z = move.z * currentSpeed;

        // Apply movement (including vertical movement)
        _characterController.Move(_direction * Time.deltaTime);
    }


    
    private void ApplyJump()
    {
        if (_isJumping && IsGrounded())
        {
            _velocity = Mathf.Sqrt(jumpPower * -2f * _gravity);
        }
    }
    private bool IsGrounded()
    {
        return _characterController.isGrounded;
    }

    // Input actions
    public void OnMove(InputAction.CallbackContext context)
    {
        _movementInput = context.ReadValue<Vector2>();
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        _lookInput = context.ReadValue<Vector2>();
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        _isJumping = context.ReadValueAsButton();
    }
    public void OnSprint(InputAction.CallbackContext context)
    {
        _isSprinting = context.ReadValueAsButton();
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _isCrouching = !_isCrouching; // Toggle crouch state
        }
    }
}
