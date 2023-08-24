using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Sprite projectileSprite;
    private int projectileDamage;
    private Vector2 startingPosition;
    private Vector2 velocityDirection;

    private float timePassed = 0;

    // initialize the variables using a function
    public void Initialize(Sprite _projectileSprite, int _projectileDamage, Vector2 _startingPosition, Vector2 _velocityDirection)
    {
        projectileSprite = _projectileSprite;
        projectileDamage = _projectileDamage;
        startingPosition = _startingPosition;
        velocityDirection = _velocityDirection;
    }

    void Start()
    {
        Vector3 rotation = new Vector3 (0, 0, 0);

        // rotate towards velocity
        if (Mathf.Abs(velocityDirection.x) > Mathf.Abs(velocityDirection.y))
        {
            if (Mathf.Sign(velocityDirection.x) == -1)
            {
                rotation = new Vector3 (0, 0, 90);
            }

            if (Mathf.Sign(velocityDirection.x) == 1)
            {
                rotation = new Vector3 (0, 0, 270);
            }
        }
        else
        {
            if (Mathf.Sign(velocityDirection.y) == -1)
            {
                rotation = new Vector3 (0, 0, 180);
            }

            if (Mathf.Sign(velocityDirection.y) == 1)
            {
                rotation = new Vector3 (0, 0, 360);
            }
        }

        // set up the gameObject
        gameObject.name = "Projectile";
        transform.eulerAngles = rotation;
        transform.position = startingPosition;
        GetComponent<SpriteRenderer>().sprite = projectileSprite;
    }

    void FixedUpdate()
    {
        if (!Player.gamePaused)
        {
            timePassed += Time.deltaTime;
            transform.position += (Vector3) velocityDirection * Time.deltaTime;
        }

        // destroy the projectile after half a second
        if (timePassed >= 0.5)
        {
            Destroy(gameObject);
        }
    }

    void OnCollisionStay2D(Collision2D col)
    {
        // deal damage on collision with enemy
        if (col.gameObject.layer == 9)
        {
            col.gameObject.GetComponent<EnemyAI>().TakeDamage(projectileDamage);

            // scale knockback depending on enemy speed
            Vector2 knockbackPosition = velocityDirection / 7;
            knockbackPosition *= col.gameObject.GetComponent<EnemyData>().speed / 5;

            // deal knockback over a short period of time
            StartCoroutine(Player.MoveInDirection(col.gameObject, velocityDirection / 7));
        }

        Destroy(gameObject);
    }
}
