using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class PlayerInventory : MonoBehaviour
{
    [Header("Inventory")]
    public GameObject inventory;
    public bool inventoryEnabled = false;

    [Header("General")]
    public Player player;
    public Placing placingScript;
    public LayerMask uiLayers;
    public TextAsset itemJson;
    public TextAsset recipesJson;

    [Header("Slots")]
    public GameObject slotOverlays;
    public GameObject slotAmounts;
    private Vector2 amountOffset;

    [Header("Remove Slot")]
    public Image removeSlot;
    public Sprite closedRemoveSlot;
    public Sprite openRemoveSlot;

    [Header("Hotbar")]
    public GameObject hotbarOverlays;
    public GameObject hotbarAmounts;
    public GameObject hotbarSlotSelectors;

    [Header("Divider")]
    public GameObject inventoryButtonPrefab;
    public Transform dividerContent;
    public TextMeshProUGUI descriptionText;
    [HideInInspector] public Recipes recipeData;
    public GameObject traderDialog;

    [Header("Storage")]
    public GameObject storagePanel;
    public GameObject storageSlots;
    public GameObject storageSlotOverlays;
    public GameObject storageSlotAmounts;
    [HideInInspector] public Storage currentStorage = null;

    [HideInInspector] public List<string> itemNames = new List<string>();
    [HideInInspector] public List<int> itemAmounts = new List<int>();
    [HideInInspector] public static ItemContainer itemData;

    private int tetherSlot = -1;
    private Vector2 tetherSlotOrigin = new Vector2();

    [Header("Hotbar Selector")]
    public int selectedHotbarSlot = 0;
    private List<KeyCode> hotbarNumbers = new List<KeyCode>()
    {
      KeyCode.Alpha1,
      KeyCode.Alpha2,
      KeyCode.Alpha3,
      KeyCode.Alpha4,
      KeyCode.Alpha5,
      KeyCode.Alpha6,
      KeyCode.Alpha7,
      KeyCode.Alpha8,
      KeyCode.Alpha9,
      KeyCode.Alpha0,
    };

    [Header("Hover Box")]
    public RectTransform hoverBox;
    public TextMeshProUGUI hoverTitle;
    public TextMeshProUGUI hoverDescription;
    public TextMeshProUGUI hoverType;
    public Vector2 hoverBoxOffset;

    private float hoverTime = 0.1f;
    private float elapsedHoverTime;

    private Dictionary<string, string> displayTypes = new Dictionary<string, string>()
    {
        {"collectible", "Collectible"},
        {"sword", "Broadsword"},
        {"bow", "Bow"},
        {"healing", "Healing"},
        {"placeable", "Placeable"},
        {"blueprint", "Blueprint"}
    };

    [System.Serializable]
    public class Item
    {
        public string name;
        public string displayName;
        public string texture;
        public string type;
        public int amount;
        public int stackAmount;
        public string description;
    }

    [System.Serializable]
    public class ItemContainer
    {
        public Item[] items;

        public static ItemContainer CreateFromJson(string jsonText)
        {
            return JsonUtility.FromJson<ItemContainer>(jsonText);
        }
    }

    [System.Serializable]
    public class Recipes
    {
        public List<string> recipes;

        public static Recipes CreateFromJson(string jsonText)
        {
            return JsonUtility.FromJson<Recipes>(jsonText);
        }
    }

    void Start()
    {
        int amountOfSlots = 40;

        for (int addInt=0; addInt<amountOfSlots; addInt++)
        {
            itemNames.Add("");
            itemAmounts.Add(0);
        }

        GameObject selectedSlot = hotbarSlotSelectors.transform.Find($"Slot Selector {selectedHotbarSlot}").gameObject;
        selectedSlot.SetActive(true);

        // make sure amount offset is consistent between screen resolutions (not having it hard-set)
        Vector2 slotOverlay0 = slotOverlays.transform.Find("Slot Overlay 0").position;
        Vector2 slotAmount0 = slotAmounts.transform.Find("Slot Amount 0").position;
        amountOffset = new Vector2(slotAmount0.x - slotOverlay0.x, slotAmount0.y - slotOverlay0.y);

        // pulling in data from json files
        itemData = ItemContainer.CreateFromJson(itemJson.text);
        recipeData = Recipes.CreateFromJson(recipesJson.text);

        ReloadInventory();
    }

    void Update()
    {
        // toggling inventory
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!inventoryEnabled && !Player.gamePaused)
            {
                // make the recipe buttons from the recipeData json file
                MakeRecipeButtons(recipeData.recipes);

                // reset the inventory messages, etc
                descriptionText.text = "Recipes";
                traderDialog.SetActive(false);
                storagePanel.SetActive(false);
                currentStorage = null;

                inventoryEnabled = true;
                inventory.SetActive(true);
                Player.gamePaused = true;
            }
            else if (inventoryEnabled)
            {
                // reset the inventory messages, etc
                descriptionText.text = "Recipes";
                traderDialog.SetActive(false);
                storagePanel.SetActive(false);
                currentStorage = null;

                inventoryEnabled = false;
                inventory.SetActive(false);
                Player.gamePaused = false;
            }
        }

        if (!inventoryEnabled)
        {
            ChangeSelectedSlot();
        }

        // keep items on mouse when moving items
        if (tetherSlot != -1)
        {
            GameObject tetheredSlot = slotOverlays.transform.Find($"Slot Overlay {tetherSlot}").gameObject;
            GameObject tetheredSlotAmount = slotAmounts.transform.Find($"Slot Amount {tetherSlot}").gameObject;

            tetheredSlot.transform.position = Input.mousePosition;

            // make sure the amount text is down-right (relative to the item)
            tetheredSlotAmount.transform.position = (Vector2) Input.mousePosition + amountOffset;
        }

        HoverAction();

        if (inventoryEnabled)
        {
            if (Input.GetMouseButtonDown(0))
            {
                DragItem();
            }

            // make sure that there is an active storage
            if (Input.GetMouseButtonDown(1) && currentStorage != null)
            {
                PushToOtherInventory();
            }
        }
        // return item to original slot if the inventory is closed
        else if (tetherSlot != -1)
        {
            GameObject tetheredSlot = slotOverlays.transform.Find($"Slot Overlay {tetherSlot}").gameObject;
            GameObject tetheredSlotAmount = slotAmounts.transform.Find($"Slot Amount {tetherSlot}").gameObject;

            tetheredSlot.transform.position = tetherSlotOrigin;

            // make sure the amount text is down-right (relative to the item)
            tetheredSlotAmount.transform.position = tetherSlotOrigin + amountOffset;

            tetheredSlot.layer = LayerMask.NameToLayer("UI");

            tetherSlot = -1;
        }
    }

    // change the selected slot in the hotbar
    public void ChangeSelectedSlot()
    {
        for (int keycodeNumber=0; keycodeNumber<hotbarNumbers.Count; keycodeNumber++)
        {
            if (Input.GetKeyDown(hotbarNumbers[keycodeNumber]))
            {
                GameObject oldSelectedSlot = hotbarSlotSelectors.transform.Find($"Slot Selector {selectedHotbarSlot}").gameObject;
                oldSelectedSlot.SetActive(false);

                selectedHotbarSlot = keycodeNumber;

                GameObject selectedSlot = hotbarSlotSelectors.transform.Find($"Slot Selector {selectedHotbarSlot}").gameObject;
                selectedSlot.SetActive(true);
            }
        }
    }

    // use item in the hotbar
    public void UseHotbarItem()
    {
        string hotbarItem = itemNames[selectedHotbarSlot+30];
        Item hotbarItemInItemData = GetItemInData(hotbarItem);
        string hotbarItemType = hotbarItemInItemData.type;
        bool usedItem = false;

        if (hotbarItemType == "healing")
        {
            player.ChangeHealth(hotbarItemInItemData.amount);
            usedItem = true;
        }

        if (hotbarItemType == "blueprint")
        {
            string cleanRecipeString = hotbarItemInItemData.name.Replace(".", ";");

            recipeData.recipes.Add(cleanRecipeString);
            usedItem = true;
        }

        if (hotbarItemType == "placeable")
        {
            if (placingScript.PlaceDownPlaceable(hotbarItemInItemData.name))
            {
                usedItem = true;
            }
        }

        if (usedItem)
        {
            DeleteFromInventory(hotbarItem, 1);
        }
    }

    // get an item in itemData
    public Item GetItemInData(string itemName)
    {
        foreach (Item item in itemData.items)
        {
            if (item.name == itemName)
            {
                return item;
            }
        }

        return new Item();
    }

    public static Item StaticGetItemInData(string itemName)
    {
        foreach (Item item in itemData.items)
        {
            if (item.name == itemName)
            {
                return item;
            }
        }

        return new Item();
    }

    // do actions when hovering over a slot or item
    public void HoverAction()
    {
        bool hoveringOverSlot = false;

        // get the closest slot to the mouse
        Collider2D [] nearUISlots = Physics2D.OverlapCircleAll(Input.mousePosition, 0.001f, uiLayers);
        string uiSlotItem = "";

        if (nearUISlots.Count() == 0)
        {
            removeSlot.sprite = closedRemoveSlot;
        }

        foreach (Collider2D uiSlot in nearUISlots)
        {
            if (uiSlot.gameObject.name.Contains("Slot Overlay"))
            {
                hoveringOverSlot = true;
                int uiSlotInt = int.Parse(uiSlot.gameObject.name.Replace("Slot Overlay ", ""));

                // set a uiSlotItem for use in hover box
                if (!uiSlot.gameObject.CompareTag("Storage Slot"))
                {
                    uiSlotItem = itemNames[uiSlotInt];
                }
                else if (currentStorage != null)
                {
                    if (uiSlotInt < currentStorage.itemNames.Count)
                    {
                        uiSlotItem = currentStorage.itemNames[uiSlotInt];
                    }
                }
            }

            // make the remove slot look like it is opening on hover
            if (uiSlot.gameObject.name == "Remove Slot")
            {
                removeSlot.sprite = openRemoveSlot;
            }
        }

        // check if not currently moving item and the slot is not empty
        if (tetherSlot == -1 && hoveringOverSlot && uiSlotItem != "")
        {
            elapsedHoverTime += Time.deltaTime;

            // show a pop-up box
            if (elapsedHoverTime >= hoverTime)
            {
                //hoverBox.transform.position = new Vector2 (Input.mousePosition.x + 80 + 10, Input.mousePosition.y - 87.5f - 12);
                float canvasScale = GetComponent<RectTransform>().localScale.x;
                Rect hoverBoxRect = hoverBox.GetComponent<RectTransform>().rect;
                Vector2 hoverBoxSize = new Vector2 (hoverBoxRect.width * canvasScale, hoverBoxRect.height * canvasScale);

                hoverBox.transform.position = new Vector2 (Input.mousePosition.x + (hoverBoxSize.x / 2), Input.mousePosition.y - 15 - (hoverBoxSize.y / 2));
                hoverBox.gameObject.SetActive(true);

                Item uiSlotItemInData = GetItemInData(uiSlotItem);

                hoverTitle.text = uiSlotItemInData.displayName;
                hoverDescription.text = uiSlotItemInData.description;

                // substitute the type of item for a cleaner version of it
                hoverType.text = displayTypes[uiSlotItemInData.type];
            }
        }
        else
        {
            hoveringOverSlot = false;
        }

        // disabled pop-up box if not hovering over item
        if (!hoveringOverSlot)
        {
            elapsedHoverTime = 0;
            hoverBox.gameObject.SetActive(false);
        }
    }

    // get the next slot avaliable
    int GetNextAvaliableSlot()
    {
        for (int itemInt=0; itemInt<itemNames.Count; itemInt++)
        {
            string itemName = itemNames[itemInt];

            if (itemName == "")
            {
                return itemInt;
            }
        }

        return -1;
    }

    // get the indexes of an item in the itemNames list
    List<int> GetItemIndexes(string element)
    {
        List<int> returnedList = new List<int>();

        for (int itemInt=0; itemInt<itemNames.Count; itemInt++)
        {
            if (itemNames[itemInt] == element)
            {
                returnedList.Add(itemInt);
            }
        }

        if (returnedList.Count > 0)
        {
            return returnedList;
        }
        else
        {
            return new List<int>();
        }
    }

    // get the amount of an item in the whole inventory
    int TallyItemAmounts(string item)
    {
        int tally = 0;

        for (int itemInt=0; itemInt<itemNames.Count; itemInt++)
        {
            if (itemNames[itemInt] == item)
            {
                tally += itemAmounts[itemInt];
            }
        }

        return tally;
    }

    // add an item to the inventory
    public void AddToInventory(string item, int amount)
    {
        bool reachedQuota = false;
        int targetIndex = 0;

        List<int> elementList = GetItemIndexes(item);

        // logic which handles overflowing of items in a single slot (going above stack amount)
        while (!reachedQuota)
        {
            bool reachedIndex = false;
            int itemIndex = 0;

            if (targetIndex < elementList.Count)
            {
                itemIndex = elementList[targetIndex];
                reachedIndex = true;
            }

            // check if selected item slot can take the remainder items
            if (reachedIndex && itemAmounts[itemIndex] + amount <= GetItemInData(item).stackAmount)
            {
                itemAmounts[itemIndex] += amount;
                reachedQuota = true;
            }
            else if (reachedIndex)
            {
                amount -= GetItemInData(item).stackAmount - itemAmounts[itemIndex];
                itemAmounts[itemIndex] = GetItemInData(item).stackAmount;
                targetIndex += 1;
            }
            else
            {
                if (itemNames.Contains(""))
                {
                    int nextAvaliableSlot = GetNextAvaliableSlot();
                    itemNames[nextAvaliableSlot] = item;
                    itemAmounts[nextAvaliableSlot] = amount;
                }

                reachedQuota = true;
            }
        }

        ReloadInventory();
    }

    public void DeleteFromInventory(string item, int amount)
    {
        if (!itemNames.Contains(item)) return;

        bool reachedQuota = false;
        int targetIndex = 0;

        List<int> elementList = GetItemIndexes(item);

        // logic which handles overflowing of items in a single slot (going above stack amount)
        while (!reachedQuota)
        {
            bool reachedIndex = false;
            int itemIndex = 0;

            if (targetIndex < elementList.Count)
            {
                itemIndex = elementList[targetIndex];
                reachedIndex = true;
            }

            if (reachedIndex && itemAmounts[itemIndex] >= amount)
            {
                itemAmounts[itemIndex] -= amount;
                reachedQuota = true;
            }
            else if (reachedIndex)
            {
                amount -= itemAmounts[itemIndex];
                itemAmounts[itemIndex] = 0;
                targetIndex += 1;
            }
            else
            {
                reachedQuota = true;
            }
        }

        // deleting the item if there is none of it
        for (int itemDeleteInt=0; itemDeleteInt<itemNames.Count; itemDeleteInt++)
        {
            if (itemNames[itemDeleteInt] == item && itemAmounts[itemDeleteInt] <= 0)
            {
                itemNames[itemDeleteInt] = "";
                itemAmounts[itemDeleteInt] = 0;
            }
        }

        ReloadInventory();
    }

    // reloads inventory
    public void ReloadInventory()
    {
        // use the items and itemNames to draw back the inventory
        for (var itemInt=0; itemInt<itemNames.Count; itemInt++)
        {
            string item = itemNames[itemInt];

            // set default to an empty slot
            string itemTexture = "Sprites/empty";
            string itemAmount = "";

            Item itemInItemData = GetItemInData(item);

            if (item != "")
            {
                // set the actual texture and amount if there is an item
                itemTexture = $"Items/{itemInItemData.texture}";
                itemAmount = itemAmounts[itemInt].ToString();
            }

            GameObject itemSlot = slotOverlays.transform.Find($"Slot Overlay {itemInt}").gameObject;
            GameObject itemSlotAmount = slotAmounts.transform.Find($"Slot Amount {itemInt}").gameObject;

            itemSlot.GetComponent<Image>().sprite = Resources.Load<Sprite>(itemTexture);
            itemSlotAmount.GetComponent<TextMeshProUGUI>().text = itemAmount;

            if (itemInt >= 30)
            {
                GameObject hotbarSlot = hotbarOverlays.transform.Find($"Slot Overlay {itemInt - 30}").gameObject;
                GameObject hotbarSlotAmount = hotbarAmounts.transform.Find($"Slot Amount {itemInt - 30}").gameObject;

                hotbarSlot.GetComponent<Image>().sprite = Resources.Load<Sprite>(itemTexture);
                hotbarSlotAmount.GetComponent<TextMeshProUGUI>().text = itemAmount;
            }
        }
    }

    // craft an item using the format outcome;outcomeAmount;ingredient0;ingredientAmount0;ingredient1;ingredientAmount1
    public void CraftItem(string recipeDataString)
    {
        List<string> recipeData = recipeDataString.Split(';').ToList();

        string outcome = recipeData[0];
        int outcomeAmount = int.Parse(recipeData[1]);

        recipeData.RemoveAt(0);
        recipeData.RemoveAt(0);

        List<string> ingredientNames = new List<string>();
        List<int> ingredientAmounts = new List<int>();

        // split the recipeData into two lists for easier organization
        for (int recipeDataCount=0; recipeDataCount<recipeData.Count; recipeDataCount++)
        {
            object ingredient = recipeData[recipeDataCount];

            if ((recipeDataCount + 1) % 2 == 0)
            {
                ingredientAmounts.Add(int.Parse((string) ingredient) - 1);
            }
            else
            {
                ingredientNames.Add((string) ingredient);
            }
        }

        // check if inventory contains the required items
        for (int ingredientNameCount=0; ingredientNameCount<ingredientNames.Count; ingredientNameCount++)
        {
            string ingredientName = ingredientNames[ingredientNameCount];
            int ingredientAmount = ingredientAmounts[ingredientNameCount];

            if (!itemNames.Contains(ingredientName))
            {
                return;
            }

            if (TallyItemAmounts(ingredientName) <= ingredientAmount)
            {
                return;
            }
        }

        for (int ingredientNameAmount=0; ingredientNameAmount<ingredientAmounts.Count; ingredientNameAmount++)
        {
            string ingredientName = ingredientNames[ingredientNameAmount];
            int ingredientAmount = ingredientAmounts[ingredientNameAmount];

            DeleteFromInventory(ingredientName, ingredientAmount + 1);
        }

        AddToInventory(outcome, outcomeAmount);
    }

    public void DragItem()
    {
        // get the slot the mouse is on
        Collider2D [] nearUIElements = Physics2D.OverlapCircleAll(Input.mousePosition, 0.001f, uiLayers);
        foreach (Collider2D uiElement in nearUIElements)
        {
            if (uiElement.gameObject.CompareTag("Storage Slot")) return;

            string uiElementName = uiElement.gameObject.name;

            // check if the element is a slot
            if (uiElementName.Contains("Slot Overlay"))
            {
                int uiElementSlot = int.Parse(uiElementName.Replace("Slot Overlay ", ""));

                // tether a slot if there is not a slot tethered already
                if (tetherSlot == -1 && itemNames[uiElementSlot] != "")
                {
                    tetherSlot = uiElementSlot;
                    tetherSlotOrigin = uiElement.transform.position;

                    GameObject tetheredSlot = slotOverlays.transform.Find($"Slot Overlay {tetherSlot}").gameObject;
                    tetheredSlot.layer = LayerMask.NameToLayer("Default");
                }
                // place down an item in a slot
                else if (tetherSlot != -1)
                {
                        GameObject tetheredSlot = slotOverlays.transform.Find($"Slot Overlay {tetherSlot}").gameObject;
                        GameObject tetheredSlotAmount = slotAmounts.transform.Find($"Slot Amount {tetherSlot}").gameObject;

                        tetheredSlot.transform.position = tetherSlotOrigin;

                        // make sure the amount text is down-right (relative to the item)
                        tetheredSlotAmount.transform.position = tetherSlotOrigin + amountOffset;

                        tetheredSlot.layer = LayerMask.NameToLayer("UI");

                        // replacing old item with new one
                        string slotReplacedOverlay = itemNames[uiElementSlot];
                        int slotReplacedInt = itemAmounts[uiElementSlot];

                        itemNames[uiElementSlot] = itemNames[tetherSlot];
                        itemAmounts[uiElementSlot] = itemAmounts[tetherSlot];

                        itemNames[tetherSlot] = slotReplacedOverlay;
                        itemAmounts[tetherSlot] = slotReplacedInt;

                        tetherSlot = -1;
                        ReloadInventory();
                }
            }
            // throw away an item if clicked on the trash can slot
            else if (uiElementName.Contains("Remove Slot") && tetherSlot != -1)
            {
                GameObject tetheredSlot = slotOverlays.transform.Find($"Slot Overlay {tetherSlot}").gameObject;
                GameObject tetheredSlotAmount = slotAmounts.transform.Find($"Slot Amount {tetherSlot}").gameObject;

                tetheredSlot.transform.position = tetherSlotOrigin;

                // make sure the amount text is down-right (relative to the item)
                tetheredSlotAmount.transform.position = tetherSlotOrigin + amountOffset;

                tetheredSlot.layer = LayerMask.NameToLayer("UI");

                itemNames[tetherSlot] = "";
                itemAmounts[tetherSlot] = 0;

                ReloadInventory();

                tetherSlot = -1;
            }
        }

        // return slot to original position if clicking out of bounds
        if (nearUIElements.Count() == 0 && tetherSlot != -1)
        {
            GameObject tetheredSlot = slotOverlays.transform.Find($"Slot Overlay {tetherSlot}").gameObject;
            GameObject tetheredSlotAmount = slotAmounts.transform.Find($"Slot Amount {tetherSlot}").gameObject;

            tetheredSlot.transform.position = tetherSlotOrigin;

            // make sure the amount text is down-right (relative to the item)
            tetheredSlotAmount.transform.position = tetherSlotOrigin + amountOffset;

            tetheredSlot.layer = LayerMask.NameToLayer("UI");

            tetherSlot = -1;
        }
    }

    // make recipe buttons using a list of the format outcome;outcomeAmount;ingredient0;ingredientAmount0;ingredient1;ingredientAmount1
    public void MakeRecipeButtons(List<string> buttonDataStringList)
    {
        for (int contentInt=0; contentInt<dividerContent.childCount; contentInt++)
        {
            Transform currentContentTransform = dividerContent.GetChild(contentInt);
            Destroy(currentContentTransform.gameObject);
        }

        // change divider content to fit buttons
        RectTransform dividerContentTransform = dividerContent.GetComponent<RectTransform>();
        float newContentHeight = (buttonDataStringList.Count * 72) - 270;

        //dividerContentTransform.sizeDelta = new Vector2(dividerContentTransform.sizeDelta.x, newContentHeight);
        dividerContentTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, newContentHeight);

        for (int buttonDataStringInt=0; buttonDataStringInt<buttonDataStringList.Count; buttonDataStringInt++)
        {
            // get the current button info from the list
            string buttonDataString = buttonDataStringList[buttonDataStringInt];

            Vector2 buttonPosition = new Vector2 (dividerContent.position.x, dividerContent.position.y + buttonDataStringInt * 72);

            GameObject newButton = Instantiate(inventoryButtonPrefab, buttonPosition, Quaternion.identity, dividerContent);
            newButton.name = buttonDataString;

            List<string> buttonDataStringSplit = buttonDataString.Split(';').ToList();

            string outcomeFromString = buttonDataStringSplit[0];
            string outcomeFromStringAmount = buttonDataStringSplit[1];

            // remove the outcome and the outcomeAmount from the list
            buttonDataStringSplit.RemoveAt(0);
            buttonDataStringSplit.RemoveAt(0);

            List<string> ingredientDataAmounts = new List<string>();
            List<string> ingredientDataNames = new List<string>();

            for (int ingredientDataCount=0; ingredientDataCount<buttonDataStringSplit.Count; ingredientDataCount++)
            {
                string buttonData = buttonDataStringSplit[ingredientDataCount];

                if ((ingredientDataCount + 1) % 2 == 0)
                {
                    ingredientDataAmounts.Add(buttonData);
                }
                else
                {
                    ingredientDataNames.Add(buttonData);
                }
            }

            // edit the recipe button to show the valid information
            Transform outcome = newButton.transform.Find("Outcome");
            outcome.gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Items/{GetItemInData(outcomeFromString).texture}");
            Transform outcomeAmount = outcome.Find("Outcome Amount");
            outcomeAmount.gameObject.GetComponent<TextMeshProUGUI>().text = outcomeFromStringAmount;

            Transform ingredient0 = newButton.transform.Find("Ingredient 0");
            ingredient0.gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Items/{GetItemInData(ingredientDataNames[0]).texture}");
            Transform ingredient0Amount = ingredient0.Find("Ingredient Amount 0");
            ingredient0Amount.gameObject.GetComponent<TextMeshProUGUI>().text = ingredientDataAmounts[0];

            if (ingredientDataNames.Count > 1)
            {
                Transform ingredient1 = newButton.transform.Find("Ingredient 1");
                ingredient1.gameObject.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Items/{GetItemInData(ingredientDataNames[1]).texture}");
                Transform ingredient1Amount = ingredient1.Find("Ingredient Amount 1");
                ingredient1Amount.gameObject.GetComponent<TextMeshProUGUI>().text = ingredientDataAmounts[1];
            }

            // craft an item on button click
            newButton.GetComponent<Button>().onClick.AddListener(() => CraftItem(buttonDataString));
        }
    }

    // move items from inventory to storage
    public void TransferFromInventoryToStorage(int itemIndex)
    {
        string movedItem = itemNames[itemIndex];
        int movedItemAmount = itemAmounts[itemIndex];

        List<int> elementList = new List<int>();

        for (int itemInt=0; itemInt<currentStorage.itemNames.Count; itemInt++)
        {
            if (currentStorage.itemNames[itemInt] == movedItem)
            {
                elementList.Add(itemInt);
            }
        }

        bool reachedQuota = false;
        int targetIndex = 0;

        // logic which handles overflowing of items in a single slot (going above stack amount)
        while (!reachedQuota)
        {
            bool reachedIndex = false;
            int quotaItemIndex = 0;

            if (targetIndex < elementList.Count)
            {
                quotaItemIndex = elementList[targetIndex];
                reachedIndex = true;
            }

            // check if selected item slot can take the remainder items
            if (reachedIndex && currentStorage.itemAmounts[quotaItemIndex] + movedItemAmount <= GetItemInData(movedItem).stackAmount)
            {
                currentStorage.itemAmounts[quotaItemIndex] += movedItemAmount;
                reachedQuota = true;
            }
            else if (reachedIndex)
            {
                movedItemAmount -= GetItemInData(movedItem).stackAmount - currentStorage.itemAmounts[quotaItemIndex];
                currentStorage.itemAmounts[quotaItemIndex] = GetItemInData(movedItem).stackAmount;
                targetIndex += 1;
            }
            else
            {
                if (currentStorage.itemNames.Count - 1 < currentStorage.amountOfSlots)
                {
                    currentStorage.itemNames.Add(movedItem);
                    currentStorage.itemAmounts.Add(movedItemAmount);
                }
                else return;

                reachedQuota = true;
            }
        }

        itemNames[itemIndex] = "";
        itemAmounts[itemIndex] = 0;

        ReloadInventory();
        ReloadStorageInventory();
    }

    // move items from storage to inventory
    public void TransferFromStorageToInventory(int itemIndex)
    {
        string movedItem = currentStorage.itemNames[itemIndex];
        int movedItemAmount = currentStorage.itemAmounts[itemIndex];

        currentStorage.itemNames.RemoveAt(itemIndex);
        currentStorage.itemAmounts.RemoveAt(itemIndex);

        AddToInventory(movedItem, movedItemAmount);

        ReloadInventory();
        ReloadStorageInventory();
    }

    // reloads storage inventory in divider
    public void ReloadStorageInventory()
    {
        int amountOfStorageSlots = 30;

        // use the items and itemNames to draw back the inventory
        for (var itemInt=0; itemInt<amountOfStorageSlots; itemInt++)
        {
            // set the default to the slot being unactive
            bool slotActive = false;

            // set default to an empty slot
            string itemTexture = "Sprites/empty";
            string itemAmount = "";

            if (itemInt < currentStorage.itemNames.Count)
            {
                string item = currentStorage.itemNames[itemInt];
                Item itemInItemData = GetItemInData(item);

                // set the actual texture and amount if there is an item
                itemTexture = $"Items/{itemInItemData.texture}";
                itemAmount = currentStorage.itemAmounts[itemInt].ToString();
            }

            if (itemInt < currentStorage.amountOfSlots)
            {
                slotActive = true;
            }

            GameObject itemSlotOverlay = storageSlotOverlays.transform.Find($"Slot Overlay {itemInt}").gameObject;
            GameObject itemSlotAmount = storageSlotAmounts.transform.Find($"Slot Amount {itemInt}").gameObject;

            itemSlotOverlay.GetComponent<Image>().sprite = Resources.Load<Sprite>(itemTexture);
            itemSlotAmount.GetComponent<TextMeshProUGUI>().text = itemAmount;

            GameObject itemSlot = storageSlots.transform.Find($"Slot {itemInt}").gameObject;

            itemSlot.SetActive(slotActive);
            itemSlotOverlay.SetActive(slotActive);
        }
    }

    // try to move item from inventory to storage or other way around
    public void PushToOtherInventory()
    {
        // get the slot the mouse is on
        Collider2D [] nearUIElements = Physics2D.OverlapCircleAll(Input.mousePosition, 0.001f, uiLayers);
        foreach (Collider2D uiElement in nearUIElements)
        {
            string uiElementName = uiElement.gameObject.name;

            if (uiElementName.Contains("Slot Overlay"))
            {
                int uiElementSlot = int.Parse(uiElementName.Replace("Slot Overlay ", ""));

                if (uiElement.gameObject.CompareTag("Storage Slot"))
                {
                    if (uiElementSlot < currentStorage.itemNames.Count)
                    {
                        TransferFromStorageToInventory(uiElementSlot);
                    }
                }
                else
                {
                    if (itemNames[uiElementSlot] != "")
                    {
                        TransferFromInventoryToStorage(uiElementSlot);
                    }
                }
            }
        }
    }
}
