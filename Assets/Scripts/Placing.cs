using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Placing : MonoBehaviour
{
    public GameObject player;
    public Camera mainCamera;
    public PlayerInventory playerInventory;

    public GameObject selectionBox;
    public LayerMask obstacleLayers;

    [System.Serializable]
    public class Placeable
    {
        public string name;
        public GameObject prefab;
        public Transform parent;
        public Sprite sprite;
    }
    public List<Placeable> placeables;

    void Update()
    {
        string hotbarItemType = playerInventory.GetItemInData(playerInventory.itemNames[playerInventory.selectedHotbarSlot + 30]).type;
        if (!Player.gamePaused && hotbarItemType == "placeable")
        {
            ShowPlaceablePosition();
        }
        else
        {
            selectionBox.SetActive(false);
        }
    }

    // get a placeable object from a placeable's name
    Placeable GetPlaceable(string name)
    {
        foreach (Placeable placeable in placeables)
        {
            if (placeable.name == name)
            {
                return placeable;
            }
        }

        return new Placeable();
    }

    // show a ghost placeable where the placeable will be placed
    void ShowPlaceablePosition()
    {
        Vector2 worldMousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        SpriteRenderer selectionBoxSpriteRenderer = selectionBox.GetComponent<SpriteRenderer>();

        string hotbarItem = playerInventory.itemNames[playerInventory.selectedHotbarSlot + 30];
        selectionBoxSpriteRenderer.sprite = GetPlaceable(hotbarItem).sprite;

        // make sure the selection position is on a tile
        Vector3 selectionPosition = new Vector3 (Mathf.Floor(worldMousePosition.x) + 0.5f, Mathf.Floor(worldMousePosition.y) + 0.5f, -9);
        selectionBox.transform.position = selectionPosition;

        float distanceFromSelection = Vector2.Distance(player.transform.position, selectionPosition);

        float colorAlpha = 0.75f;

        if (distanceFromSelection > 2)
        {
            colorAlpha = 0.25f;
        }

        // make the color red if there is obstacle where the placeable is trying to be placed
        if (Physics2D.OverlapBoxAll(selectionPosition, new Vector2 (0.9f, 0.9f), 0, obstacleLayers).Count() == 0)
        {
            selectionBoxSpriteRenderer.color = new Color (1, 1, 1, colorAlpha);
        }
        else
        {
            selectionBoxSpriteRenderer.color = new Color (1, 0.5f, 0.5f, colorAlpha);
        }

        selectionBox.SetActive(true);
    }

    public bool PlaceDownPlaceable(string name)
    {
        Vector2 worldMousePosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        // make sure the selection position is on a tile
        Vector2 placeablePosition = new Vector2 (Mathf.Floor(worldMousePosition.x) + 0.5f, Mathf.Floor(worldMousePosition.y) + 0.5f);
        float distanceFromPlaceable = Vector2.Distance(player.transform.position, placeablePosition);

        // return if too far from the placeable position
        if (distanceFromPlaceable > 2) return false;

        // only place down the placeable if there is no obstacle where the placeable is trying to be placed
        if (Physics2D.OverlapBoxAll(placeablePosition, new Vector2 (0.9f, 0.9f), 0, obstacleLayers).Count() == 0)
        {
            GameObject placeablePrefab = GetPlaceable(name).prefab;
            Transform placeableParent = GetPlaceable(name).parent;

            Instantiate(placeablePrefab, (Vector3) placeablePosition, Quaternion.identity, placeableParent);
            return true;
        }
        else
        {
            return false;
        }
    }
}
