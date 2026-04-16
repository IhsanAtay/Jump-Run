using UnityEngine;
using UnityEngine.InputSystem;

public class Character : MonoBehaviour
{
    private bool isJumping = false;
    private float jumpCooldownTimer;
    private CharacterController controller;
    private InputAction moveAction;
    private InputAction jumpAction;

    [SerializeField]
    private float jumpCooldown;
    [SerializeField]
    private float gravity;
    [SerializeField]
    private float characterSpeed;
    [SerializeField]
    private float jumpSpeed;
    [SerializeField]
    private float dampening;
    [SerializeField]
    private Transform cameraTransform;
    [SerializeField]
    private LayerMask platformLayer; // Im Inspector "Platforms" auswählen!

    private Vector3 characterMovement;
    private Vector3 jumpVelocity;
    private Vector3 characterGravity;
    private Vector3 platformVelocity;

    void Start()
    {
        this.controller = this.GetComponent<CharacterController>();
        this.moveAction = InputSystem.actions.FindAction("Move");
        this.jumpAction = InputSystem.actions.FindAction("Jump");
        this.jumpCooldownTimer = 0.0f;
    }

    void HandleJumping()
    {
        if (this.controller.isGrounded && this.isJumping && this.jumpCooldownTimer <= 0.0f)
        {
            this.jumpVelocity = Vector3.zero;
            this.isJumping = false;
        }

        if (this.controller.isGrounded && !this.isJumping && this.jumpAction.WasPressedThisFrame())
        {
            this.characterGravity = Vector3.zero;
            this.jumpVelocity = Vector3.zero;
            this.jumpVelocity.y = this.jumpSpeed;
            this.jumpCooldownTimer = this.jumpCooldown;
            this.isJumping = true;
        }

        if (this.jumpVelocity.y > 0.0f)
        {
            this.jumpVelocity.y -= Time.fixedDeltaTime;
        }
        else
        {
            this.jumpVelocity = Vector3.zero;
        }

        this.jumpCooldownTimer -= Time.fixedDeltaTime;
    }

    void GetPlatformVelocity()
    {
        // Raycast nach unten — trifft er eine Platform?
        RaycastHit hit;
        if (Physics.Raycast(this.transform.position, Vector3.down, out hit, 2.0f, this.platformLayer))
        {
            MovingPlatform platform = hit.collider.GetComponent<MovingPlatform>();
            if (platform != null)
            {
                this.platformVelocity = platform.GetVelocity();
                return;
            }
        }

        // Keine Platform getroffen → keine Platform-Velocity
        this.platformVelocity = Vector3.zero;
    }
    public void ResetVelocity()
    {
        this.characterMovement = Vector3.zero;
        this.jumpVelocity = Vector3.zero;
        this.characterGravity = Vector3.zero;
        this.platformVelocity = Vector3.zero;
        this.isJumping = false;
    }

    void FixedUpdate()
    {
        this.HandleJumping();
        this.GetPlatformVelocity();

        var inputMovement = this.moveAction.ReadValue<Vector2>();
        var inputRightDirection = this.cameraTransform.right;
        var inputForwardDirection = this.cameraTransform.forward;
        inputRightDirection.y = 0.0f;
        inputForwardDirection.y = 0.0f;
        inputRightDirection.Normalize();
        inputForwardDirection.Normalize();

        if (this.controller.isGrounded)
        {
            this.characterGravity.y = 0.0f;
        }

        this.characterGravity.y += this.gravity * Time.fixedDeltaTime;
        this.characterMovement += this.characterGravity * Time.fixedDeltaTime;
        this.characterMovement += this.jumpVelocity * Time.fixedDeltaTime;
        this.characterMovement += inputRightDirection * inputMovement.x * this.characterSpeed * Time.fixedDeltaTime;
        this.characterMovement += inputForwardDirection * inputMovement.y * this.characterSpeed * Time.fixedDeltaTime;
        this.characterMovement *= (1 - this.dampening);

        Vector3 characterForward = this.characterMovement;
        characterForward.y = 0.0f;
        if (characterForward.sqrMagnitude > 0.0f && characterForward != Vector3.zero)
        {
            this.transform.forward = characterForward.normalized;
        }

        // Platform Velocity nur anwenden wenn nicht am Springen
        var combinedMovement = this.characterMovement;
        if (!this.isJumping)
        {
            combinedMovement = this.characterMovement + this.platformVelocity * Time.fixedDeltaTime;
        }

        this.controller.Move(combinedMovement);
    }
}