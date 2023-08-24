using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BowGFX : MonoBehaviour
{
    private Vector3 leftPosition;
    private Vector3 rightPosition;
    private Vector3 upPosition;
    private Vector3 downPosition;

    private Vector3 leftRotation;
    private Vector3 rightRotation;
    private Vector3 upRotation;
    private Vector3 downRotation;

    void Start()
    {
        leftPosition = new Vector3 (-0.25f, 0, -7);
        rightPosition = new Vector3 (0.25f, 0, -7);
        upPosition = new Vector3 (0, 0.125f, -3);
        downPosition = new Vector3 (0, -0.2f, -7);

        leftRotation = new Vector3 (0, 0, 135);
        rightRotation = new Vector3 (0, 0, 315);
        upRotation = new Vector3 (0, 0, 45);
        downRotation = new Vector3 (0, 0, 225);

        gameObject.SetActive(false);
    }

    void SetBowAtLeftPosition()
    {
        transform.localPosition = leftPosition;
        transform.eulerAngles = leftRotation;
    }

    void SetBowAtRightPosition()
    {
        transform.localPosition = rightPosition;
        transform.eulerAngles = rightRotation;
    }

    void SetBowAtUpPosition()
    {
        transform.localPosition = upPosition;
        transform.eulerAngles = upRotation;
    }

    void SetBowAtDownPosition()
    {
        transform.localPosition = downPosition;
        transform.eulerAngles = downRotation;
    }

    public IEnumerator UseBow(Vector2 heading)
    {
        // rotate towards heading direction
        if (Mathf.Abs(heading.x) > Mathf.Abs(heading.y))
        {
            if (Mathf.Sign(heading.x) == -1)
            {
                SetBowAtLeftPosition();
            }

            if (Mathf.Sign(heading.x) == 1)
            {
                SetBowAtRightPosition();
            }
        }
        else
        {
            if (Mathf.Sign(heading.y) == -1)
            {
                SetBowAtDownPosition();
            }

            if (Mathf.Sign(heading.y) == 1)
            {
                SetBowAtUpPosition();
            }
        }

        gameObject.SetActive(true);

        // disable after animation is done in 0.3s
        yield return new WaitForSeconds(0.3f);
        gameObject.SetActive(false);
    }
}
