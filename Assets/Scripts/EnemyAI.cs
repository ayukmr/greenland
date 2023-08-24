using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class EnemyAI : MonoBehaviour
{
    private Transform target;
    private float nextWaypointDistance = 3;

    private float speed;
    public int health;

    private Path path;
    private int currentWaypoint = 0;
    private bool triggered = false;

    private Seeker seeker;
    private Collider2D col;

    void Start()
    {
        seeker = GetComponent<Seeker>();
        col = GetComponent<Collider2D>();

        speed = GetComponent<EnemyData>().speed;
        health = GetComponent<EnemyData>().health;

        target = GameObject.FindGameObjectsWithTag("Player")[0].transform;

        InvokeRepeating("UpdatePath", 0f, 0.1f);
    }

    void Update()
    {
        if (health <= 0)
        {
            // random spawn
            float rndX = Random.Range(transform.position.x - 1, transform.position.x + 1);
            float rndY = Random.Range(transform.position.y - 1, transform.position.y + 1);

            // create the collectible
            GameObject collectible = Instantiate(Resources.Load<GameObject>("Prefabs/Collectible"), new Vector3 (rndX, rndY, -1), Quaternion.identity);
            collectible.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>($"Items/{PlayerInventory.StaticGetItemInData(GetComponent<EnemyData>().material).texture}");
            collectible.GetComponent<Collectible>().type = GetComponent<EnemyData>().material;

            Destroy(gameObject);
        }
    }

    void UpdatePath()
    {
        // start a path once the seeker is done processing
        if (seeker.IsDone())
        {
            seeker.StartPath(col.transform.position, target.position, OnPathComplete);
        }
    }

    void OnPathComplete(Path _path)
    {
        // run only if path doesn't raise an error
        if (!_path.error)
        {
            path = _path;
            currentWaypoint = 0;
        }
    }

    void FixedUpdate()
    {
        // stop following when more than 7 units away
        if (Vector2.Distance(transform.position, target.transform.position) >= 7)
        {
            triggered = false;
        }

        // start following if less than 3 units away
        if (Vector2.Distance(transform.position, target.transform.position) <= 3)
        {
            triggered = true;
        }

        if (!triggered) return;
        if (Player.gamePaused) return;

        if (path == null) return;
        if (currentWaypoint >= path.vectorPath.Count) return;

        Vector2 direction = (((Vector2) path.vectorPath[currentWaypoint] - (Vector2) col.transform.position).normalized);
        Vector2 force = direction * speed * Time.deltaTime;

        Vector2 moveSpeed = direction * (Time.deltaTime * speed);

        // slow down enemy when far from player
        if (Vector2.Distance(transform.position, target.transform.position) >= 3)
        {
            moveSpeed /= 1.5f;
        }

        if (PlayerSlowMotion.isSlowMotion)
        {
            moveSpeed /= 2;
        }

        col.transform.position = new Vector3 (col.transform.position.x + moveSpeed.x, col.transform.position.y + moveSpeed.y, -1);

        float distance = Vector2.Distance(col.transform.position, path.vectorPath[currentWaypoint]);

        // continue to the next waypoint when reaching last one
        if (distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }
    }

    public void TakeDamage(int amount)
    {
        health -= amount;

        SpriteRenderer enemySpriteRenderer = GetComponent<SpriteRenderer>();
        StartCoroutine(DamageVisual(enemySpriteRenderer));
    }

    // change the color of the enemy when taking damage
    IEnumerator DamageVisual(SpriteRenderer spriteRenderer)
    {
        /*
        Material originalMaterial = spriteRenderer.material;

        spriteRenderer.material = Resources.Load<Material>("Materials/Flash Material");
        yield return new WaitForSeconds(0.125f);
        spriteRenderer.material = originalMaterial;
        */

        Color originalColor = spriteRenderer.color;
        spriteRenderer.color = new Color (1, 0, 0);

        yield return new WaitForSeconds(0.125f);
        spriteRenderer.color = originalColor;
    }
}
