using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float maximumSpeed;
    public float runSpeedMultiplier = 1.6f;
    public float rotationSpeed;
    public float jumpSpeed;
    public float jumpButtonGracePeriod;
    public Transform cameraTransform;
    public float mouseSensitivity = 2f;
    public float minPitch = -45f;
    public float maxPitch = 75f;

    private StateMachine stateMachine;
    private CharacterController characterController;
    private float ySpeed;
    private float originalStepOffset;
    private float? lastGroundedTime;
    private float? jumpButtonPressedTime;
    private float pitch;
    private float inputMagnitude;
    private bool isRunning;

    void Start()
    {
        stateMachine = GetComponent<StateMachine>();
        characterController = GetComponent<CharacterController>();
        originalStepOffset = characterController.stepOffset;
        
        if (cameraTransform == null && Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        
        SetupStates();
        stateMachine.SetState("idle");
    }

    void SetupStates()
    {
        stateMachine.AddState("idle", new IdleState());
        stateMachine.AddState("moving", new MovingState(this));
    }

    void Update()
    {
        HandleMouseLook();
        HandleInput();
        stateMachine.Update();
    }

    void FixedUpdate()
    {
        stateMachine.FixedUpdate();
    }

    private void HandleMouseLook()
    {
        if (cameraTransform == null)
            return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(0f, mouseX, 0f);

        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        cameraTransform.localEulerAngles = new Vector3(pitch, 0f, 0f);
    }

    private void HandleInput()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 movementDirection = new Vector3(horizontalInput, 0, verticalInput);
        inputMagnitude = Mathf.Clamp01(movementDirection.magnitude);
        isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        if (Input.GetButtonDown("Jump"))
        {
            jumpButtonPressedTime = Time.time;
        }
    }

    public void ApplyMovement(Vector3 direction, float speed)
    {
        ySpeed += Physics.gravity.y * Time.deltaTime;

        if (characterController.isGrounded)
        {
            lastGroundedTime = Time.time;
        }

        if (lastGroundedTime != null && Time.time - lastGroundedTime <= jumpButtonGracePeriod)
        {
            characterController.stepOffset = originalStepOffset;
            ySpeed = -0.5f;

            if (jumpButtonPressedTime != null && Time.time - jumpButtonPressedTime <= jumpButtonGracePeriod)
            {
                ySpeed = jumpSpeed;
                jumpButtonPressedTime = null;
                lastGroundedTime = null;
            }
        }
        else
        {
            characterController.stepOffset = 0;
        }

        Vector3 velocity = direction * speed;
        velocity.y = ySpeed;
        characterController.Move(velocity * Time.deltaTime);
    }

    public float GetInputMagnitude() => inputMagnitude;
    public bool IsRunning() => isRunning;
    public Vector3 GetMovementDirection()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        Vector3 direction = new Vector3(horizontalInput, 0, verticalInput);

        if (direction.sqrMagnitude > 0f && cameraTransform != null)
        {
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;
            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();
            direction = (cameraForward * verticalInput + cameraRight * horizontalInput).normalized;
        }
        else
        {
            direction.Normalize();
        }

        return direction;
    }

    // State classes
    private class IdleState : StateMachine.State
    {
        public override void OnEnter()
        {
            SetAnimatorBool("IsMoving", false);
        }
    }

    private class MovingState : StateMachine.State
    {
        private PlayerMovement player;

        public MovingState(PlayerMovement playerMovement)
        {
            player = playerMovement;
        }

        public override void Tick()
        {
            Vector3 direction = player.GetMovementDirection();
            float inputMag = player.GetInputMagnitude();

            if (inputMag == 0)
            {
                stateMachine.SetState("idle");
                return;
            }

            SetAnimatorBool("IsMoving", true);
            float animMag = player.IsRunning() ? Mathf.Clamp01(inputMag * player.runSpeedMultiplier) : inputMag;
            SetAnimatorFloat("Input Magnitude", animMag);

            float speed = inputMag * player.maximumSpeed * (player.IsRunning() ? player.runSpeedMultiplier : 1f);
            player.ApplyMovement(direction, speed);
        }
    }
}
