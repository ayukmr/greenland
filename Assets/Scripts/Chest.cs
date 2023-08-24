using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chest : MonoBehaviour
{
    private Transform player;
    private Animator animator;

    void Start()
    {
        player = GameObject.FindGameObjectsWithTag("Player")[0].transform;
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, player.position) <= 2.5f)
        {
            animator.SetTrigger("Open");
            animator.ResetTrigger("Close");
        }
        else if (Vector3.Distance(transform.position, player.position) > 2.5f)
        {
            animator.SetTrigger("Close");
            animator.ResetTrigger("Open");
        }
    }
}
