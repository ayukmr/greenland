using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSlowMotion : MonoBehaviour
{
    public static bool isSlowMotion = false;
    public int maxSlowMotion = 5;
    private int currentSlowMotion;
    public int slowMotionGenerationRate;
    private bool chargingSlowMotion = false;
    public SlowMotionBar slowMotionBar;
    public ParticleSystem slowMotionParticles;

    void Start()
    {
        // change the associated bar
        currentSlowMotion = maxSlowMotion;
        slowMotionBar.SetMaxSlowMotion(maxSlowMotion);
    }

    void Update()
    {
        // start and stop particles
        if (isSlowMotion)
        {
            slowMotionParticles.Play();
        }
        else
        {
            slowMotionParticles.Stop();
        }

        if (!Player.gamePaused)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                isSlowMotion = !isSlowMotion;
                StartCoroutine(StartSlowMotion());
            }
        }

        if (!chargingSlowMotion && currentSlowMotion < 5) 
        {
            StartCoroutine(ChargeSlowMotion());
        }
    }

    void UseSlowMotion(int used)
    {
        currentSlowMotion -= used;
        slowMotionBar.SetSlowMotion(currentSlowMotion);
    }

    private IEnumerator StartSlowMotion()
    {
        for (float s = 0; s < 6; s += 1)
        {
            if (currentSlowMotion < 1)  break;

            yield return new WaitForSeconds(1);
            if (!isSlowMotion) break;
            if (Player.gamePaused) break;

            UseSlowMotion(1);
        }

        isSlowMotion = false;
    }

    private IEnumerator ChargeSlowMotion()
    {
        for (float s = 0; s <= maxSlowMotion; s += 1)
        {
            chargingSlowMotion = true;
            if (currentSlowMotion >= 5) break;

            yield return new WaitForSeconds(2);
            if (isSlowMotion) break;
            if (Player.gamePaused) break;
            
            currentSlowMotion += slowMotionGenerationRate;
            slowMotionBar.SetSlowMotion(currentSlowMotion);
        }
        chargingSlowMotion = false;
    }
}
