using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogNpcs : MonoBehaviour
{
    public RectTransform player;

    public TextMeshProUGUI dialogMessage;
    public GameObject dialogContinue;
    public RectTransform dialogCanvas;

    private bool dialogOn = false;

    [System.Serializable]
    public class DialogNpc
    {
        public Collider2D collider;
        public List<string> dialog;
        public int dialogPlace;
    }
    public List<DialogNpc> dialogNpcs = new List<DialogNpc>();

    void Update()
    {
        // disable dialog when more than 3 units away from player
        if (dialogCanvas.gameObject.activeSelf)
        {
            if (Vector2.Distance(player.transform.position, dialogCanvas.transform.position) >= 2)
            {
                dialogCanvas.gameObject.SetActive(false);
            }
        }
    }

    // get a dialogNpc class from a collider
    DialogNpc FindNpc(Collider2D npc)
    {
        foreach (DialogNpc currentNpc in dialogNpcs)
        {
            if (currentNpc.collider == npc)
            {
                return currentNpc;
            }
        }

        return new DialogNpc();
    }

    public void ShowDialog(Collider2D npc)
    {
        if (dialogOn)
        {
            return;
        }

        DialogNpc currentNpc = FindNpc(npc);

        int currentDialogPlace = currentNpc.dialogPlace;
        string currentDialog = currentNpc.dialog[currentDialogPlace];
        Vector2 npcPosition = currentNpc.collider.transform.position;

        dialogCanvas.gameObject.SetActive(true);
        dialogCanvas.position = new Vector2 (npcPosition.x, npcPosition.y + 1.2f);

        dialogContinue.gameObject.SetActive(false);

        if (!dialogOn)
        {
            StartCoroutine(TypewriterEffect(currentDialog, currentNpc, currentDialogPlace));
        }
    }

    public IEnumerator TypewriterEffect(string text, DialogNpc currentNpc, int currentDialogPlace)
    {
        dialogMessage.text = "";
        dialogOn = true;
        bool dialogCanceled = false;

        for (int charInt=0; charInt<text.Length; charInt++)
        {
            if (!dialogCanvas.gameObject.activeSelf)
            {
                dialogCanceled = true;
                break;
            }

            char character = text[charInt];
            dialogMessage.text += character;

            // get text speed from slider in mainMenu
            yield return new WaitForSeconds(0.225f / MainMenu.textSpeed);
        }

        if (!dialogCanceled)
        {
            // loop if final message is reached
            if (currentDialogPlace < currentNpc.dialog.Count - 1)
            {
                currentNpc.dialogPlace += 1;
            }
            else
            {
                currentNpc.dialogPlace = 0;
            }

            bool isFinalMessage = false;

            if (currentDialogPlace + 1 > currentNpc.dialog.Count - 1)
            {
                isFinalMessage = true;
            }

            // automatically disable if final message
            if (isFinalMessage)
            {
                yield return new WaitForSeconds(1.5f);
                dialogCanvas.gameObject.SetActive(false);
            }
            else
            {
                dialogContinue.SetActive(true);
            }
        }

        dialogOn = false;
    }
}
