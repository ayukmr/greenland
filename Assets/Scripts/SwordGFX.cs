using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordGFX : MonoBehaviour
{
    public GameObject swordGFX;
    public Animator playerAnimator;
    private Animator swordAnimator;

    private Vector3 leftPosition;
    private Vector3 rightPosition;
    private Vector3 upPosition;
    private Vector3 downPosition;

    void Start()
    {
        swordAnimator = GetComponent<Animator>();

        leftPosition = new Vector3 (-0.125f, -0.15f, -7);
        rightPosition = new Vector3 (0.1250f, -0.15f, -7);
        upPosition = new Vector3 (0.3500f, -0.10f, -3);
        downPosition = new Vector3 (-0.325f, -0.10f, -7);
    }

    void Update()
    {
        // make sure the sword is animating in the same direction as the player
        string playerAnimationDirection = playerAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.name.Replace("player-sword-attack-", "");
        string swordAnimatorDirection = swordAnimator.GetCurrentAnimatorClipInfo(0)[0].clip.name.Replace("sword-swing-", "");

        if (playerAnimationDirection != swordAnimatorDirection)
        {
            DisableSwordGFX();
        }
    }

    // functions for use in animator animations
    void SetSwordAtLeftPosition()
    {
        transform.localPosition = leftPosition;
    }

    void SetSwordAtRightPosition()
    {
        transform.localPosition = rightPosition;
    }

    void SetSwordAtUpPosition()
    {
        transform.localPosition = upPosition;
    }

    void SetSwordAtDownPosition()
    {
        transform.localPosition = downPosition;
    }

    void DisableSwordGFX()
    {
        swordGFX.SetActive(false);
    }
}
