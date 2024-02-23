using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StepSounds : MonoBehaviour
{
    [Header("Step Sound Effects")]
    public AudioClip[] groundFootstepSounds;

    [Header("Jump Sound Effects")]
    public AudioClip[] jumpSounds;
    public AudioClip[] landSounds;

    [Header("Step Sounds Surface Parameters")]
    public MovementSFXCooldowns movementSFXCooldowns;

    private bool _logicBarrier = false;

    public AudioSource audioSource;

    [HideInInspector] public FpvController playerController;
    [HideInInspector] public PlayerStatistics _playerStatistics;
    private void Awake()
    {
        playerController = GetComponent<FpvController>();
        _playerStatistics = GetComponent<PlayerStatistics>();
    }
    private void Start()
    {
        audioSource ??= GetComponent<AudioSource>();
    }
    public void PlayFootstepSound(string surfaceTag, FpvController.MovementState _MovementState)
    {
        AudioClip _audioClip = null;
        float newWalkSpeed = 0;
        float newSprintSpeed = 0;
        float newHungerSprintSpeed = 0;
        if (!_logicBarrier) 
        {
            switch (surfaceTag)
            {
                case "Ground":
                    if (groundFootstepSounds.Length > 1)
                    {
                        int a = Random.Range(1, groundFootstepSounds.Length);
                        _audioClip = groundFootstepSounds[a];
                        groundFootstepSounds[a] = groundFootstepSounds[0];
                        groundFootstepSounds[0] = _audioClip;
                        newWalkSpeed = movementSFXCooldowns.GroundWalking_Cd;
                        newSprintSpeed = movementSFXCooldowns.GroundSprinting_Cd;
                        newHungerSprintSpeed = movementSFXCooldowns.GroundHungerSprinting_Cd;
                    }
                    break;
            }
        }
        

        if (_audioClip != null)
        {
            float _coolDown = 0;
            if (_MovementState == FpvController.MovementState.walking && !_logicBarrier) 
            {
               audioSource.PlayOneShot(_audioClip);
                _coolDown = _audioClip.length * newWalkSpeed;
                StartCoroutine(SFX_Cooldown(_coolDown));
                _logicBarrier = true;
            }
            else if(!_logicBarrier && !_playerStatistics.isHungry && !_playerStatistics.isThirsty)
            {
                audioSource.PlayOneShot(_audioClip);
                _coolDown = _audioClip.length * newSprintSpeed;
                StartCoroutine(SFX_Cooldown(_coolDown));
                _logicBarrier = true;   
            }
            else if(!_logicBarrier)
            {
                audioSource.PlayOneShot(_audioClip);
                _coolDown = _audioClip.length * newHungerSprintSpeed;
                StartCoroutine(SFX_Cooldown(_coolDown));
                _logicBarrier = true;
            }
        }
    }
    private IEnumerator SFX_Cooldown(float _coolDown) 
    {
        yield return new WaitForSeconds(_coolDown);
        _logicBarrier = false;
    }
}
[System.Serializable]
public class MovementSFXCooldowns
{
    [Range(0, 3)]
    public float GroundWalking_Cd;
    [Range(0, 3)]
    public float GroundSprinting_Cd;
    [Range(0, 3)]
    public float GroundHungerSprinting_Cd;
}
