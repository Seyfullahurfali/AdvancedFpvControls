using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class StaminaController : MonoBehaviour
{
    [Header("Stamina Main Parameters")]
    public float playerStamina = 100.0f;
    [SerializeField] private float maxStamina = 100.0f;
    [SerializeField] private float jumpCost = 20.0f;
    [HideInInspector] public bool hasRegenerated = true;
    public bool isTired = false;


    [Header("Stamina Regen Parameters")]
    [Range(0, 5)] [SerializeField] private float staminaDrain = 1f;
    [Range(0, 5)] [SerializeField] private float staminaRegen = 1f;
    public bool isRegeneratingFromZero;

    [Header("Stamina Speed Parameters")]
    [SerializeField] private int slowedRunSpeed = 4;
    [SerializeField] private int normalRunSpeed = 8;

    private FpvController playerController;
    private void Start()
    {
        playerController = GetComponent<FpvController>();
    }
    private void Update()
    {
        if (playerController.state != FpvController.MovementState.sprinting || playerStamina <= 0)
        {
            if(playerStamina <= maxStamina)
            {
                playerStamina += staminaRegen * Time.deltaTime;

                if (playerStamina >= maxStamina)
                {
                    playerStamina = maxStamina;
                    playerController.sprintSpeed = normalRunSpeed;
                    hasRegenerated = true;
                    if (isRegeneratingFromZero) { isRegeneratingFromZero = false; }                  
                }
            }         
        }
    }
    public void Sprinting()
    {
        if (hasRegenerated)
        {
            playerStamina -= staminaDrain * Time.deltaTime;

            if(playerStamina <= 0)
            {
                hasRegenerated = false;
                playerController.sprintSpeed = slowedRunSpeed;
            }
        }
    }
    public void StaminaJump()
    {
        if(playerStamina >= (maxStamina * jumpCost / maxStamina))
        {
            playerStamina -= jumpCost;
        }
    }
}
