using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStamina : MonoBehaviour
{
    public static bool isSprinting = false;
    public int maxStamina = 5;
    private int currentStamina;
    public int staminaGenerationRate;
    private bool chargingStamina = false;
    public StaminaBar staminaBar;
    public ParticleSystem staminaParticles;
    public PlayerFadeTrail fadeTrail;

    void Start()
    {
        // change the associated bar
        currentStamina = maxStamina;
        staminaBar.SetMaxStamina(maxStamina);
    }

    void Update()
    {
        // start and stop particles
        if (isSprinting)
        {
            fadeTrail.enabled = true;
        }
        else
        {
            fadeTrail.enabled = false;
        }

        if (!Player.gamePaused)
        {
            if (Input.GetKeyDown(KeyCode.R) && currentStamina > 0)
            {
                isSprinting = !isSprinting;
                StartCoroutine(StartStamina());
            }
        }

        if (!chargingStamina && currentStamina < 5)
        {
            StartCoroutine(ChargeStamina());
        }
    }

    void UseStamina(int used)
    {
        currentStamina -= used;
        staminaBar.SetStamina(currentStamina);
    }

    private IEnumerator StartStamina()
    {
        for (float s = 0; s < 6; s += 1)
        {
            if (!isSprinting) break;

            yield return new WaitForSeconds(1);
            if (currentStamina < 1)  break;
            if (Player.gamePaused) break;

            UseStamina(1);
        }

        isSprinting = false;
    }

    private IEnumerator ChargeStamina()
    {
        for (float s = 0; s <= maxStamina; s += 1)
        {
            chargingStamina = true;

            if (currentStamina >= 5) break;
            yield return new WaitForSeconds(2);

            if (isSprinting) break;
            if (Player.gamePaused) break;

            currentStamina += staminaGenerationRate;
            staminaBar.SetStamina(currentStamina);
        }
        chargingStamina = false;
    }
}
