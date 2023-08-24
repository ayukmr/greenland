using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMap : MonoBehaviour
{
    public Transform mapCamera;
    public GameObject map;
    public Transform player;
    private bool mapShown = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (!mapShown && !Player.gamePaused)
            {
                mapCamera.transform.position = new Vector3 (player.position.x, player.position.y, -10);
                mapShown = true;
                map.SetActive(true);
                Player.gamePaused = true;
            }
            else if (mapShown)
            {
                mapShown = false;
                map.SetActive(false);
                Player.gamePaused = false;
            }
        }

        if (mapShown)
        {
            // get inputs using GetAxisRaw (WASD and arrow keys both work)
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveY = Input.GetAxisRaw("Vertical");

            Vector3 moveDirection = new Vector3 (moveX, moveY, 0).normalized;
            mapCamera.transform.position += moveDirection * 0.25f;
        }
    }
}
