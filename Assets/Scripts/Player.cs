using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Player : MonoBehaviour
{
    [Header("General")]
    public SpriteRenderer sprite;
    private Rigidbody2D playerRigidbody;
    private BoxCollider2D playerCollider;
    public Animator animator;
    public Camera mainCamera;
    public Canvas canvas;
    public TextMeshProUGUI nicknameText;

    [Header("Movement")]
    public float moveSpeed;
    [HideInInspector] public Vector2 moveDirection;
    private Vector2 lastingMoveDirection;
    public float timeBetweenDashes;
    private float elapsedTimeBetweenDashes;

    [Header("Health")]
    public int maxHealth = 5;
    public int currentHealth;
    public HealthBar healthBar;

    [Header("Invincibility")]
    private bool isInvincible = false;
    public float invicibilityDuration;
    public Material mainMaterial;
    public Material flashMaterial;

    [Header("Layers")]
    public LayerMask enemyLayers;
    public LayerMask obstacleLayers;
    public LayerMask npcLayers;

    [Header("Attacking")]
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public int attackDamage = 1;
    public float timeBetweenAttacks;
    private float elapsedTimeBetweenAttacks = 0;
    public Sprite projectileSprite;
    public GameObject projectilePrefab;
    public GameObject swordGFX;
    public Animator swordAnimator;
    public GameObject bowGFX;

    [Header("NPCs")]
    public float detectRange = 0.25f;
    public DialogNpcs dialogNpcs;
    public TradingNpcs tradingNpcs;

    [Header("Menus")]
    public static bool gamePaused;
    public PlayerInventory playerInventory;
    public GameObject pauseMenu;
    public GameObject gameOverMenu;

    [Header("Cursors")]
    public Texture2D defaultCursor;
    public Texture2D dialogCursor;
    public Texture2D tradeCursor;
    public Texture2D storageCursor;

    void Start()
    {
        // change the cursor texture
        Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);

        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);

        nicknameText.text = (string) MainMenu.nickname;

        playerRigidbody = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<BoxCollider2D>();

        canvas.pixelPerfect = true;

        gamePaused = false;
    }

    void Update()
    {
        // make sure there is always a movement direction in the animator
        if (moveDirection.sqrMagnitude >= 0.001f)
        {
            animator.SetFloat("Horizontal", moveDirection.x);
            animator.SetFloat("Vertical", moveDirection.y);

            lastingMoveDirection.x = moveDirection.x;
            lastingMoveDirection.y = moveDirection.y;
        }

        animator.SetFloat("Speed", moveDirection.sqrMagnitude);

        // stop player movement on game pause
        if (gamePaused)
        {
            playerRigidbody.velocity = Vector2.zero;
            Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
        }

        if (!gamePaused)
        {
            ProcessInputs();
            CursorHandler();

            if (elapsedTimeBetweenDashes <= 0)
            {
                if (Input.GetKeyDown(KeyCode.Space) && moveDirection.sqrMagnitude >= 0.001f)
                {
                    elapsedTimeBetweenDashes = timeBetweenDashes;
                    StartCoroutine(Dash());
                }
            }
            else
            {
                elapsedTimeBetweenDashes -= Time.deltaTime;
            }

            if (Input.GetMouseButtonDown(1))
            {
                string hotbarItem = playerInventory.itemNames[playerInventory.selectedHotbarSlot+30];
                string hotbarItemInItemData = playerInventory.GetItemInData(hotbarItem).type;

                if (!CheckForNpc())
                {
                    if (!CheckForStorage())
                    {
                        playerInventory.UseHotbarItem();
                    }
                }
            }

            if (elapsedTimeBetweenAttacks <= 0)
            {
                // attack using items of only certain types
                if (Input.GetMouseButtonDown(0))
                {
                    string hotbarItem = playerInventory.itemNames[playerInventory.selectedHotbarSlot+30];
                    string itemIndexInDataType = playerInventory.GetItemInData(hotbarItem).type;
                    int itemIndexInDataAmount = playerInventory.GetItemInData(hotbarItem).amount;

                    if (new List<string>() {"sword", "bow"}.Contains(itemIndexInDataType))
                    {
                        elapsedTimeBetweenAttacks = timeBetweenAttacks;
                        attackDamage = itemIndexInDataAmount;
                        Attack();
                    }
                }
            }
            else
            {
                elapsedTimeBetweenAttacks -= Time.deltaTime;
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!pauseMenu.activeSelf && !playerInventory.inventoryEnabled)
            {
                pauseMenu.SetActive(true);
                gamePaused = true;
            }
            else if (pauseMenu.activeSelf)
            {
                pauseMenu.SetActive(false);
                gamePaused = false;
            }
        }

        if (currentHealth <= 0)
        {
            gameOverMenu.SetActive(true);
            gamePaused = true;
        }

        string currentSprite = sprite.sprite.name;

        // adjust the boxCollider2D size depending on the player's direction
        if (currentSprite.Contains("left") | currentSprite.Contains("right"))
        {
            playerCollider.size = new Vector2 (0.55f, playerCollider.size.y);
        }
        else
        {
            playerCollider.size = new Vector2 (0.8f, playerCollider.size.y);
        }
    }

    void FixedUpdate()
    {
        if (!gamePaused)
        {
            Move();
        }
    }

    void OnCollisionStay2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Damage"))
        {
            // check if collider is on Enemies layer (layer 9)
            if (col.gameObject.layer == 9)
            {
                // deal damage to the player depending on the enemy damage
                ChangeHealth(-col.gameObject.GetComponent<EnemyData>().damage);
            }
            else
            {
                ChangeHealth(-1);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        // collect collectibles
        if (col.gameObject.layer == 13)
        {
            if (col is BoxCollider2D)
            {
                Collectible collectible = col.gameObject.GetComponent<Collectible>();
                string collectibleType = collectible.GetCollectibleType();

                collectible.CollectCollectible();
                playerInventory.AddToInventory(collectibleType, 1);
            }
        }
    }

    void OnTriggerStay2D(Collider2D col)
    {
        if (col.gameObject.layer == 13)
        {
            if (col is CircleCollider2D)
            {
                Vector3 direction = (transform.position - col.transform.position);
                direction = new Vector3 (Mathf.Sign(direction.x), Mathf.Sign(direction.y), Mathf.Sign(direction.z)) * 0.1f;

                col.transform.position += direction;
            }
        }
    }

    // change the cursor depending on what is being hovered on
    void CursorHandler()
    {
        Vector2 worldMousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        Collider2D[] nearNpcs = Physics2D.OverlapCircleAll(worldMousePosition, detectRange, npcLayers);
        foreach (Collider2D npc in nearNpcs)
        {
            if (npc.gameObject.CompareTag("Trading Npc"))
            {
                Cursor.SetCursor(tradeCursor, Vector2.zero, CursorMode.Auto);
                return;
            }

            if (npc.gameObject.CompareTag("Dialog Npc"))
            {
                Cursor.SetCursor(dialogCursor, Vector2.zero, CursorMode.Auto);
                return;
            }
        }

        Collider2D[] nearObstacles = Physics2D.OverlapCircleAll(worldMousePosition, detectRange, obstacleLayers);
        foreach (Collider2D obstacle in nearObstacles)
        {
            if (obstacle.gameObject.CompareTag("Storage"))
            {
                Cursor.SetCursor(storageCursor, Vector2.zero, CursorMode.Auto);
                return;
            }
        }

        Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
    }

    void ProcessInputs()
    {
        float moveX = 0;
        float moveY = 0;

        string currentSprite = sprite.sprite.name;
        if (!currentSprite.Contains("attack"))
        {
            // get inputs using GetAxisRaw (WASD and arrow keys both work)
            moveX = Input.GetAxisRaw("Horizontal");
            moveY = Input.GetAxisRaw("Vertical");
        }

        moveDirection = new Vector2 (moveX, moveY).normalized;
    }

    void Move()
    {
        // change playerRigidbody velocity depending on the move direction
        playerRigidbody.velocity = moveDirection * moveSpeed;
    }

    IEnumerator Dash()
    {
        // increase the move speed for the dash
        moveSpeed *= 2;
        GetComponent<PlayerFadeTrail>().enabled = true;

        for (int dashInt=0; dashInt<10; dashInt++)
        {
            if (!gamePaused)
            {
                // incrementally move the player
                playerRigidbody.velocity = moveDirection * moveSpeed;
                yield return new WaitForSeconds(0.01f);
            }
        }

        // reset the move speed
        moveSpeed /= 2;
        GetComponent<PlayerFadeTrail>().enabled = false;
    }

    // deal attacks with hotbar items
    void Attack()
    {
        Vector3 attackPosition = playerRigidbody.position;

        // use a seperate Vector2 than lastingMoveDirection for the possibility that it is diagonal
        Vector2 nonDiagonalMoveDirection = new Vector2 (0, 0);

        // get the current hotbar item's type and texture
        string hotbarItemType = playerInventory.GetItemInData(playerInventory.itemNames[playerInventory.selectedHotbarSlot+30]).type;
        string hotbarItemTexture = playerInventory.GetItemInData(playerInventory.itemNames[playerInventory.selectedHotbarSlot + 30]).texture;

        float attackSkew = 0.5f;

        if (new List<string>{"bow"}.Contains(hotbarItemType))
        {
            // change the attack skew to negate collision with projectile
            attackSkew = 0.7f;
        }

        // make sure the attack is not diagonal
        if (Mathf.Abs(lastingMoveDirection.x) != Mathf.Abs(lastingMoveDirection.y))
        {
            nonDiagonalMoveDirection = lastingMoveDirection;
        }
        else
        {
            nonDiagonalMoveDirection = new Vector2 (0, Mathf.Sign(lastingMoveDirection.y));
        }

        attackPosition.x += nonDiagonalMoveDirection.x * attackSkew;
        attackPosition.y += nonDiagonalMoveDirection.y * attackSkew;

        swordAnimator.SetFloat("Horizontal", nonDiagonalMoveDirection.x);
        swordAnimator.SetFloat("Vertical", nonDiagonalMoveDirection.y);

        attackPoint.position = attackPosition;

        if (hotbarItemType == "sword")
        {
            // display the sword as the sword being used
            swordGFX.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>($"Items/{hotbarItemTexture}");
            swordGFX.SetActive(true);

            animator.SetTrigger("Sword Attack");
            swordAnimator.SetTrigger("Swing");

            Collider2D [] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.transform.position, attackRange, enemyLayers);

            foreach (Collider2D enemy in hitEnemies)
            {
                enemy.gameObject.GetComponent<EnemyAI>().TakeDamage(attackDamage);

                // scale knockback depending on enemy speed
                Vector2 knockbackPosition = (Vector2) nonDiagonalMoveDirection;
                knockbackPosition *= enemy.gameObject.GetComponent<EnemyData>().speed / 5;

                // deal knockback over a short period of time
                StartCoroutine(MoveInDirection(enemy.gameObject, knockbackPosition));
            }
        }

        if (hotbarItemType == "bow")
        {
            Vector2 projectileVelocity;

            projectileVelocity = new Vector2 (nonDiagonalMoveDirection.x * 7, nonDiagonalMoveDirection.y * 7);

            // display the bow being used
            bowGFX.GetComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>($"Items/{hotbarItemTexture}");
            BowGFX bowScript = bowGFX.GetComponent<BowGFX>();
            StartCoroutine(bowScript.UseBow(nonDiagonalMoveDirection));

            animator.SetTrigger("Bow Attack");

            // make a gameObject with the projectile script
            GameObject projectile = Instantiate(projectilePrefab);
            projectile.AddComponent<Projectile>();

            // initialize the projectile using the function
            projectile.GetComponent<Projectile>().Initialize(
                projectileSprite,
                attackDamage,
                attackPoint.position,
                projectileVelocity
            );
        }
    }

    public static IEnumerator MoveInDirection(GameObject gameObj, Vector2 direction, int speed = 5)
    {
        for (int moveReps=0; moveReps<speed; moveReps++)
        {
            if (gameObj != null)
            {
                gameObj.transform.position += (Vector3) direction / speed;
                yield return new WaitForSeconds(0.01f);
            }
        }
    }

    // check for npc using Physics2D
    bool CheckForNpc()
    {
        Vector2 worldMousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        float distanceFromMouse = Vector2.Distance(transform.position, worldMousePosition);

        // return if too far from the mouse cursor
        if (distanceFromMouse > 1.5f) return false;

        Collider2D[] nearNpcs = Physics2D.OverlapCircleAll(worldMousePosition, detectRange, npcLayers);
        foreach (Collider2D npc in nearNpcs)
        {
            // check what type of npc the collider is
            if (npc.gameObject.CompareTag("Trading Npc"))
            {
                tradingNpcs.ShowTradingMenu(npc);
                return true;
            }

            if (npc.gameObject.CompareTag("Dialog Npc"))
            {
                dialogNpcs.ShowDialog(npc);
                return true;
            }
        }

        return false;
    }

    // check for storage using Physics2D
    bool CheckForStorage()
    {
        Vector2 worldMousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        float distanceFromMouse = Vector2.Distance(transform.position, worldMousePosition);

        // return if too far from the mouse cursor
        if (distanceFromMouse > 1.5f) return false;

        Collider2D [] nearObstacles = Physics2D.OverlapCircleAll(worldMousePosition, detectRange, obstacleLayers);
        foreach (Collider2D obstacle in nearObstacles)
        {
            if (obstacle.CompareTag("Storage"))
            {
                playerInventory.currentStorage = obstacle.GetComponent<Storage>();
                playerInventory.ReloadStorageInventory();

                // reset the inventory messages, etc
                playerInventory.descriptionText.text = "Storage";
                playerInventory.traderDialog.SetActive(false);
                playerInventory.storagePanel.SetActive(true);

                playerInventory.inventoryEnabled = true;
                playerInventory.inventory.SetActive(true);
                gamePaused = true;

                return true;
            }
        }

        return false;
    }

    public void SetPausedState(bool paused)
    {
        gamePaused = paused;
    }

    public void GoToMainMenu()
    {
        // get the next scene in the queue
        SceneManager.LoadScene("Menu");
    }

    public void Respawn()
    {
        // reset player
        playerRigidbody.position = Vector2.zero;
        currentHealth = maxHealth;
        healthBar.SetHealth(currentHealth);
    }

    public void ChangeHealth(int delta)
    {
        // only deal damage if not invincible
        if (isInvincible && delta < 0) return;

        currentHealth += delta;

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        healthBar.SetHealth(currentHealth);

        if (delta < 0)
        {
            StartCoroutine(StartInvincibility());
        }
    }

    private IEnumerator StartInvincibility()
    {
        isInvincible = true;
        bool hasFlashed = false;

        sprite.material = flashMaterial;
        yield return new WaitForSeconds(0.0625f);
        sprite.material = mainMaterial;

        for (float i = 0; i < invicibilityDuration; i += invicibilityDuration / 5)
        {
            // change the sprite to show invincibility
            if (hasFlashed)
            {
                sprite.transform.localScale = Vector2.one;
                hasFlashed = false;
            }
            else
            {
                sprite.transform.localScale = Vector2.zero;
                hasFlashed = true;
            }

            yield return new WaitForSeconds(invicibilityDuration / 5);
        }

        sprite.transform.localScale = Vector2.one;
        isInvincible = false;
    }
}
