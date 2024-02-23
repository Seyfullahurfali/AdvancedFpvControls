using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class FpvController : MonoBehaviour
{
    [Header("Cursor State")]
    public bool isLocked = false;

    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    [SerializeField] private bool isMoving;

    [Header("Headbob Parameters")]
    [SerializeField] private float walkBobSpeed = 14f;
    [SerializeField] private float walkBobAmount = 0.05f;
    [SerializeField] private float sprintBobSpeed = 18f;
    [SerializeField] private float sprintBobAmount = 0.1f;
    [SerializeField] private float sprintFOV = 75f;
    [SerializeField] private float walkFOV = 60f;
    [SerializeField] private float fovChangeSpeed = 5f;
    [SerializeField] private bool canUseHeadbob = true;
    private float deafoultYPos = 0;
    private float timer;
    private bool isBobbing = false;
    [Range(0, 1)]
    public float bobDelay = 0.2f;

    [Header("Jump Parameters")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    [SerializeField] private bool readyToJump = true;
    bool hasJumped;

    [Header("Sounds")]
    [Range(0, 15f)]
    public int tiredSFXCooldown = 5;
    [Range(0.5f, 1.5f)]
    public float walkPitch = 1f;
    [Range(0.5f, 1.5f)]
    public float sprintPitch = 1f;
    [Range(0.5f, 1.5f)]
    public float hungerSprintPitch = 1f;

    [Header("Keybinds")]
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode jumpKey = KeyCode.Space;
    float horizontalInput;
    float verticalInput;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public bool grounded;
    public float groundDrag;
    private bool isLanded = true;
    public string surfaceTag = "";


    [Header("FOV Parameters")]
    private float targetFOV;
    private float currentFOV;

    private RaycastHit hitInfo;
    public Camera playerCamera;
    public AudioSource audioSource;
    Rigidbody rb;
    public Transform orientation;
    Vector3 moveDirection;

    [HideInInspector] public LandingCameraShake landingCameraShake;
    [HideInInspector] public StepSounds stepSounds;
    [HideInInspector] public StaminaController _staminaController;
    [HideInInspector] public PlayerStatistics _playerStatistics;

    public MovementState state;
    public enum MovementState
    {
        idle,
        walking,
        sprinting,
        air
    }
    private void OnEnable()
    {
        UpdateCursorState(isLocked);
    }
    private void Awake()
    {
        deafoultYPos = playerCamera.transform.localPosition.y;
    }
    private void Start()
    {
        landingCameraShake = GameObject.FindGameObjectWithTag("PlayerCam").GetComponent<LandingCameraShake>();
        stepSounds = GetComponent<StepSounds>();
        _staminaController = GetComponent<StaminaController>();
        _playerStatistics = GetComponent<PlayerStatistics>();
        currentFOV = playerCamera.fieldOfView;
        targetFOV = currentFOV;
        audioSource = GetComponent<AudioSource>();
     }
    private void Update()
    {
        // Ground Check
        GroundCheckFonction();
        
        MyInput();
        StateHandler();
        SpeedControl();
        UpdateFOV();       

        // Handle Drag
        DragHandle();
    }
    private void FixedUpdate()
    {
        MovePlayer();
        if (canUseHeadbob)
        {
            HandleHeadbob();
        }
    }
  
    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // When to Jump
        if (Input.GetKey(jumpKey) && readyToJump && grounded && _staminaController.playerStamina > 20)
        {
            readyToJump = false;
            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }
    private void StateHandler()
    {
        // Mode - Sprinting
        if(grounded && Input.GetKey(sprintKey) && Input.GetKey(KeyCode.W) && !_staminaController.isRegeneratingFromZero)
        {
            if (!_playerStatistics.isHungry && !_playerStatistics.isThirsty)
            {
                state = MovementState.sprinting;
                moveSpeed = sprintSpeed;
                _staminaController.Sprinting();
                CheckGroundPlaySFX();
            }
            else
            {
                state = MovementState.sprinting;
                moveSpeed = walkSpeed;
                _staminaController.Sprinting();
                CheckGroundPlaySFX();
            }
        }

        // Mode - Walking
        else if(grounded && moveDirection.normalized.x != 0)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
            CheckGroundPlaySFX();
        }

        // Mode - Idle
        else if(grounded && moveDirection.normalized.x == 0)
        {
            state = MovementState.idle;
        }

        // Mode- Air
        if(!grounded)
        {
            state = MovementState.air;
            isLanded = false;
        }
    }

    #region Movement and Speed
    private void MovePlayer()
    {
        rb ??= GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        // Calculate Movement Direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // On Ground
        if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        }
        // in Air
        else if(!grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * airMultiplier * 10f, ForceMode.Force);
    }
    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // Limit Velocity If Needed
        if(flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z); 
        }
    }
    #endregion

    private void GroundCheckFonction()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);
    }

    private void DragHandle()
    {
        if (grounded)
        {
            rb.drag = groundDrag;
            if (!isLanded)
            {
                PlayLandingSound(surfaceTag);
                StartCoroutine(landingCameraShake.PerformJumpEffect());
                isLanded = true;
            }
        }
        else
            rb.drag = 0;
    }
    #region Headbob-FOV
    private void HandleHeadbob()
    {
        if (!grounded) return;

        if (Mathf.Abs(moveDirection.x) > 0.1f || Mathf.Abs(moveDirection.z) > 0.1f)
        {
            if (!isBobbing)
            {
                StartCoroutine(StartBobAfterDelay());
            }
        }
        else
        {
            isBobbing = false;
        }

        if (isBobbing)
        {
            UpdateHeadBob();
        }
    }
    private IEnumerator StartBobAfterDelay()
    {
        yield return new WaitForSeconds(bobDelay);
        isBobbing = true;
    }
    private void UpdateHeadBob()
    {
        if (!grounded) return;

        if (Mathf.Abs(moveDirection.x) > 0.1f || Mathf.Abs(moveDirection.z) > 0.1f)
        {
            float _localBobSpeed = 0f;
            float _localBobAmount = 0f;
            if(_playerStatistics.isHungry && _playerStatistics.isThirsty)
            {
                if (state == MovementState.sprinting)
                {
                    _localBobSpeed = _playerStatistics.hungerSprintBobSpeed;
                    _localBobAmount = _playerStatistics.hungerSprintBobAmount;
                }
                else if (state == MovementState.walking)
                {
                    _localBobSpeed = _playerStatistics.hungerWalkBobSpeed;
                    _localBobAmount = _playerStatistics.hungerWalkBobAmount;
                }
            }
            else
            {
                if (state == MovementState.sprinting)
                {
                    _localBobSpeed = sprintBobSpeed;
                    _localBobAmount = sprintBobAmount;
                }
                else if (state == MovementState.walking)
                {
                    _localBobSpeed = walkBobSpeed;
                    _localBobAmount = walkBobAmount;
                }
            }       
            timer += Time.deltaTime * _localBobSpeed;
            playerCamera.transform.localPosition = new Vector3(playerCamera.transform.localPosition.x, deafoultYPos + Mathf.Sin(timer) * _localBobAmount, playerCamera.transform.localPosition.z);
        }
    }
    private void UpdateFOV()
    {
        switch (state)
        {
            case MovementState.walking:
                targetFOV = walkFOV;
                break;
            case MovementState.sprinting:
                targetFOV = sprintFOV;
                break;
        }

        // FOV Lerp is used to smoothly change the values
        currentFOV = Mathf.Lerp(currentFOV, targetFOV, fovChangeSpeed * Time.deltaTime);
        playerCamera.fieldOfView = currentFOV;
    }
    #endregion

    #region Tired
    private IEnumerator UnlockNextTiredEffect(float Cooldown) 
    {
        _staminaController.isTired = true;
        yield return new WaitForSeconds(Cooldown);
        _staminaController.isTired = false;
    }
    #endregion

    #region Jump
    private void ResetJump()
    {
        readyToJump = true;
    }
    public void Jump()
    {
        // Reset y Velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        if (!_playerStatistics.isThirsty && !_playerStatistics.isHungry)
        {
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            _staminaController.StaminaJump();
            PlayRandomJumpSound();
        }
        else
        {
            rb.AddForce(transform.up * _playerStatistics.lowJump, ForceMode.Impulse);
            _staminaController.StaminaJump();
            PlayRandomJumpSound();
        }
    }
    private void PlayRandomJumpSound()
    {
        if (stepSounds.jumpSounds.Length > 0 && !hasJumped)
        {
            int randomIndex = Random.Range(0, stepSounds.jumpSounds.Length);
            audioSource.PlayOneShot(stepSounds.jumpSounds[randomIndex], 1.5f);
            hasJumped = true;

            StartCoroutine(ResetHasJumped());
        }
    }
    private void PlayLandingSound(string surfaceTag)
    {
        // Sound Effect for Different Surfaces
        switch (surfaceTag)
        {
            case "Ground":
                audioSource.PlayOneShot(stepSounds.landSounds[0], 2);
                break;
        }
    }
    private IEnumerator ResetHasJumped()
    {
        yield return new WaitForSeconds(0.5f);
        hasJumped = false;
    }
    #endregion

    private static void UpdateCursorState(bool isLocked)
    {
        Cursor.lockState = isLocked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = isLocked ? Cursor.visible = false : Cursor.visible = true;
    }
    private void CheckGroundPlaySFX()
    {
        MovementState _ms = MovementState.idle;

        if (Physics.Raycast(transform.position, Vector3.down, out hitInfo))
        {
            surfaceTag = hitInfo.collider.tag;
        }
        if (state == MovementState.walking)
        {
            stepSounds.audioSource.pitch = walkPitch;
            _ms = MovementState.walking;
        }
        else if (state == MovementState.sprinting && !_playerStatistics.isHungry && !_playerStatistics.isThirsty)
        {
            stepSounds.audioSource.pitch = sprintPitch;
            _ms = MovementState.sprinting;
        }
        else if(state == MovementState.sprinting)
        {
            stepSounds.audioSource.pitch = hungerSprintPitch;
            _ms = MovementState.sprinting;
        }
        stepSounds.PlayFootstepSound(surfaceTag, _ms);
    }
}
