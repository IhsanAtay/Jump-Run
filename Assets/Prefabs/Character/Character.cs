using UnityEngine;
using UnityEngine.InputSystem;

public class Character : MonoBehaviour
{
    private bool isJumping = false;
    private float jumpCooldownTimer;
    private CharacterController controller;
    private Animator animator;
    private AudioSource footstepsAudio;
    private AudioSource jumpAudio;
    private InputAction moveAction;
    private InputAction jumpAction;

    [SerializeField] private float jumpCooldown;
    [SerializeField] private float gravity;
    [SerializeField] private float characterSpeed;
    [SerializeField] private float jumpSpeed;
    [SerializeField] private float dampening;
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private LayerMask platformLayer;
    [SerializeField] private AudioClip footstepsClip;
    [SerializeField] private AudioClip jumpClip;
    [SerializeField] private ParticleSystem dustParticles;

    private Vector3 characterMovement;
    private Vector3 jumpVelocity;
    private Vector3 characterGravity;
    private Vector3 platformVelocity;

    void Start()
    {
        this.controller = this.GetComponent<CharacterController>();
        this.animator = this.GetComponentInChildren<Animator>();
        this.moveAction = InputSystem.actions.FindAction("Move");
        this.jumpAction = InputSystem.actions.FindAction("Jump");
        this.jumpCooldownTimer = 0.0f;

        this.footstepsAudio = this.gameObject.AddComponent<AudioSource>();
        this.footstepsAudio.clip = this.footstepsClip;
        this.footstepsAudio.loop = true;
        this.footstepsAudio.playOnAwake = false;
        this.footstepsAudio.volume = 0.6f;

        this.jumpAudio = this.gameObject.AddComponent<AudioSource>();
        this.jumpAudio.clip = this.jumpClip;
        this.jumpAudio.loop = false;
        this.jumpAudio.playOnAwake = false;
        this.jumpAudio.volume = 0.4f;
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

            this.jumpAudio.PlayOneShot(this.jumpClip, 0.4f);
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

    void UpdateAnimator()
    {
        var inputMovement = this.moveAction.ReadValue<Vector2>();
        float speed = inputMovement.magnitude;

        this.animator.SetFloat("Speed", speed);
        this.animator.SetBool("IsJumping", this.isJumping);
        this.animator.SetBool("IsGrounded", this.controller.isGrounded);

        if (speed > 0.1f && this.controller.isGrounded)
        {
            if (!this.footstepsAudio.isPlaying)
                this.footstepsAudio.Play();

            if (!this.dustParticles.isPlaying)
                this.dustParticles.Play();
        }
        else
        {
            this.footstepsAudio.Stop();
            this.dustParticles.Stop();
        }
    }

    void GetPlatformVelocity()
    {
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

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (!this.isJumping) return;

        EnemyController enemy = hit.gameObject.GetComponent<EnemyController>();
        if (enemy != null)
        {
            // Spieler abprallen lassen
            this.jumpVelocity.y = this.jumpSpeed;
            this.isJumping = true;

            enemy.GetHit();
        }
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
            this.characterGravity.y = 0.0f;

        this.characterGravity.y += this.gravity * Time.fixedDeltaTime;
        this.characterMovement += this.characterGravity * Time.fixedDeltaTime;
        this.characterMovement += this.jumpVelocity * Time.fixedDeltaTime;
        this.characterMovement += inputRightDirection * inputMovement.x * this.characterSpeed * Time.fixedDeltaTime;
        this.characterMovement += inputForwardDirection * inputMovement.y * this.characterSpeed * Time.fixedDeltaTime;
        this.characterMovement *= (1 - this.dampening);

        Vector3 characterForward = this.characterMovement;
        characterForward.y = 0.0f;
        if (characterForward.sqrMagnitude > 0.0f && characterForward != Vector3.zero)
            this.transform.forward = characterForward.normalized;

        var combinedMovement = this.characterMovement;
        if (!this.isJumping)
            combinedMovement = this.characterMovement + this.platformVelocity * Time.fixedDeltaTime;

        this.controller.Move(combinedMovement);
        this.UpdateAnimator();
    }
}