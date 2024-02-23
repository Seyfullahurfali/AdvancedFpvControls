using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStatistics : MonoBehaviour
{
    private float maxHunger = 100f;
    private float maxThirst = 100f;

    [Header("Settings")]
    [Range(0, 1)]
    [Space(5)]
    public float hungerRate = 1.0f;
    [Range(0, 1)]
    public float thirstRate = 1.5f;

    [Header("Humanoid AudioClips")]
    public AudioClip[] hungerClips;
    public AudioClip[] thirstClips;

    [Header("Headbob Parameters")]
    [SerializeField] public float hungerWalkBobSpeed = 14f;
    [SerializeField] public float hungerWalkBobAmount = 0.05f;
    [SerializeField] public float hungerSprintBobSpeed = 18f;
    [SerializeField] public float hungerSprintBobAmount = 0.1f;

    private bool hasPlayedHungerSound = false;
    private bool hasPlayedThirstSound = false;

    private float sprintHungerRate() 
    {
        return hungerRate * 1.8f;
    }
    private float sprintThirstRate() 
    {
        return thirstRate * 2;
    }

    [Header("Humanoid Parameters")]
    [Space(5)]
    public bool isHungry;
    public bool isThirsty;
    public float lowJump = 2f;

    public float currentHunger = 100f;
    public float currentThirst = 100f;

    private FpvController playerController;
    private void Start()
    {
        playerController = GetComponent<FpvController>();
    }
    private void Update()
    {
        ReduceHunger();
    }
    public void PlayerCurrentStats()
    {

    }
    public void ReduceHunger()
    {
        float time = Time.deltaTime;

        if(playerController.state == FpvController.MovementState.sprinting)
        {
            currentHunger -= sprintHungerRate() * time;
            currentThirst -= sprintThirstRate() * time;
        }
        else
        {
            currentHunger -= hungerRate * time;
            currentThirst -= thirstRate * time;
        }

        if (currentHunger <= 30)
        {
            PlayRandomSound(hungerClips, hasPlayedHungerSound);
            hasPlayedHungerSound = true;
            isHungry = true;
        }
        else 
        {
            isHungry = false;
            hasPlayedHungerSound = false;
        }

        if (currentThirst <= 30)
        {
            PlayRandomSound(thirstClips, hasPlayedThirstSound);
            hasPlayedThirstSound = true;
            isThirsty = true; 
        }
        else
        { 
            isThirsty = false;
            hasPlayedThirstSound = false;
        }


        if (currentHunger > maxHunger) { currentHunger = maxHunger; }
        if(currentHunger <= 0) { currentHunger = 0; }
        if (currentThirst > maxThirst) { currentThirst = maxThirst; }
        if (currentThirst <= 0) { currentThirst = 0; }
    }
    private void PlayRandomSound(AudioClip[] clips, bool Id)
    {
        if (clips.Length == 0) return;
        if (!Id)
        {
            playerController.audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)]);
        }
    }
}
