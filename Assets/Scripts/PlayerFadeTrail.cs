using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFadeTrail : MonoBehaviour
{
    public float timeBetweenSpawns;
    private float elapsedTimeBetweenSpawns = 0;

    public Player playerScript;
    public SpriteRenderer playerSpriteRenderer;
    public GameObject gfx;

    void Update()
    {
        if (playerScript.moveDirection.sqrMagnitude < 0.001f) return;

        if (elapsedTimeBetweenSpawns <= 0)
        {
            Vector3 gfxGameObjectPosition = new Vector3 (transform.position.x, transform.position.y, -1);
            GameObject gfxGameObject = Instantiate(gfx, gfxGameObjectPosition, Quaternion.identity);

            gfxGameObject.name = "Trail";
            // change the sprite of the trail to match the player's
            gfxGameObject.GetComponent<SpriteRenderer>().sprite = playerSpriteRenderer.sprite;

            Destroy(gfxGameObject, 0.125f);
            elapsedTimeBetweenSpawns = timeBetweenSpawns;
        }
        else
        {
            elapsedTimeBetweenSpawns -= Time.deltaTime;
        }
    }
}
