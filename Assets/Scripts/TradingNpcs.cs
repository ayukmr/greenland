using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TradingNpcs : MonoBehaviour
{
    public PlayerInventory playerInventory;
    public GameObject player;

    public GameObject traderDialog;
    public Image traderDialogImage;
    public TextMeshProUGUI traderDialogText;

    public GameObject tradeCanvas;
    public GameObject tradePanel;
    private bool tradeOn = false;

    [System.Serializable]
    public class TradingNpc
    {
        public Collider2D collider;
        public List<string> trades;
        public List<string> dialogs;
        public int tradePlace;
    }
    public List<TradingNpc> tradingNpcs = new List<TradingNpc>();

    void Update()
    {
        // disable trade when more than 3 units away from player
        if (tradeCanvas.gameObject.activeSelf)
        {
            if (Vector2.Distance(player.transform.position, tradeCanvas.transform.position) >= 2)
            {
                tradeCanvas.gameObject.SetActive(false);
            }
        }
    }

    // get a tradingNpc class from a collider
    TradingNpc FindNpc(Collider2D npc)
    {
        foreach (TradingNpc currentNpc in tradingNpcs)
        {
            if (currentNpc.collider == npc)
            {
                return currentNpc;
            }
        }

        return new TradingNpc();
    }

    public void ShowTradingMenuOld(Collider2D npc)
    {
        TradingNpc currentNpc = FindNpc(npc);

        traderDialogImage.sprite = npc.gameObject.GetComponent<SpriteRenderer>().sprite;
        traderDialogText.text = currentNpc.dialogs[Random.Range(0, currentNpc.dialogs.Count)];
        traderDialog.SetActive(true);

        playerInventory.descriptionText.text = "Trades";
        playerInventory.MakeRecipeButtons(currentNpc.trades);

        playerInventory.inventoryEnabled = true;
        playerInventory.inventory.SetActive(true);

        Player.gamePaused = true;
    }

    public void ShowTradingMenu(Collider2D npc)
    {
        if (tradeOn)
        {
            return;
        }

        TradingNpc currentNpc = FindNpc(npc);

        int currentTradePlace = currentNpc.tradePlace;
        string currentTrade = currentNpc.trades[currentTradePlace];
        Vector2 npcPosition = currentNpc.collider.transform.position;

        LoadTradePanel(currentNpc, currentTrade);

        tradeCanvas.gameObject.SetActive(true);
        tradeCanvas.transform.position = new Vector2(npcPosition.x, npcPosition.y + 1.2f);
    }

    private void LoadTradePanel(TradingNpc npc, string trade)
    {
        List<string> tradeList = trade.Split(';').ToList();

        string outcome = tradeList[0];
        string outcomeAmount = tradeList[1];

        string ingredient0 = tradeList[2];
        string ingredient0Amount = tradeList[3];
        string ingredient1 = "";
        string ingredient1Amount = "";

        if (tradeList.Count > 4)
        {
            ingredient1 = tradeList[4];
            ingredient1Amount = tradeList[5];
        }

        GameObject outcomeImage = tradePanel.transform.Find("Outcome").gameObject;
        GameObject outcomeAmountText = outcomeImage.transform.Find("Outcome Amount").gameObject;
        outcomeImage.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Items/{playerInventory.GetItemInData(outcome).texture}");
        outcomeAmountText.GetComponent<TextMeshProUGUI>().text = outcomeAmount;

        GameObject ingredient0Image = tradePanel.transform.Find("Ingredient 0").gameObject;
        GameObject ingredient0AmountText = ingredient0Image.transform.Find("Ingredient 0 Amount").gameObject;
        ingredient0Image.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Items/{playerInventory.GetItemInData(ingredient0).texture}");
        ingredient0AmountText.GetComponent<TextMeshProUGUI>().text = ingredient0Amount;

        GameObject ingredient1Image = tradePanel.transform.Find("Ingredient 1").gameObject;
        GameObject ingredient1AmountText = ingredient1Image.transform.Find("Ingredient 1 Amount").gameObject;
        if (ingredient1 != "")
        {
            ingredient1Image.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Items/{playerInventory.GetItemInData(ingredient1).texture}");
            ingredient1AmountText.GetComponent<TextMeshProUGUI>().text = ingredient1Amount;
        }
        else
        {
            ingredient1Image.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/empty");
            ingredient1AmountText.GetComponent<TextMeshProUGUI>().text = "";
        }

        Button tradeButton = tradePanel.transform.Find("Trade Button").gameObject.GetComponent<Button>();
        tradeButton.onClick.RemoveAllListeners();
        tradeButton.onClick.AddListener(() => playerInventory.CraftItem(trade));

        Button arrowButton = tradePanel.transform.Find("Arrow Button").gameObject.GetComponent<Button>();
        arrowButton.onClick.RemoveAllListeners();
        arrowButton.onClick.AddListener(() => GoToNextTrade(npc));
    }

    public void GoToNextTrade(TradingNpc npc)
    {
        // loop if final trade is reached
        if (npc.tradePlace < npc.trades.Count - 1)
        {
            npc.tradePlace += 1;
        }
        else
        {
            npc.tradePlace = 0;
        }

        LoadTradePanel(npc, npc.trades[npc.tradePlace]);
    }
}
