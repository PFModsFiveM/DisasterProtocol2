using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(StateMachine))]
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

        if (stateMachine == null)
        {
            stateMachine = gameObject.AddComponent<StateMachine>();
            Debug.LogWarning("StateMachine was missing and has been added automatically.", this);
        }

        if (characterController == null)
        {
            characterController = gameObject.AddComponent<CharacterController>();
            Debug.LogWarning("CharacterController was missing and has been added automatically.", this);
        }

        if (maximumSpeed <= 0f)
        {
            maximumSpeed = 5f;
        }

        if (rotationSpeed <= 0f)
        {
            rotationSpeed = 10f;
        }

        if (jumpButtonGracePeriod <= 0f)
        {
            jumpButtonGracePeriod = 0.2f;
        }

        if (jumpSpeed <= 0f)
        {
            jumpSpeed = 5f;
        }

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
        stateMachine.AddState("idle", new IdleState(this));
        stateMachine.AddState("moving", new MovingState(this));
    }

    void Update()
    {
        HandleMouseLook();
        HandleInput();
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
        private PlayerMovement player;

        public IdleState(PlayerMovement playerMovement)
        {
            player = playerMovement;
        }

        public override void OnEnter()
        {
            SetAnimatorBool("IsMoving", false);
            SetAnimatorFloat("Input Magnitude", 0f);
        }

        public override void Tick()
        {
            // Keep gravity grounded while idle.
            player.ApplyMovement(Vector3.zero, 0f);

            if (player.GetInputMagnitude() > 0.01f)
            {
                stateMachine.SetState("moving");
            }
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

            if (direction.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
                player.transform.rotation = Quaternion.Slerp(
                    player.transform.rotation,
                    targetRotation,
                    player.rotationSpeed * Time.deltaTime
                );
            }
        }
    }
}
