using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public static string nickname = "Nickname";
    public static int textSpeed = 3;

    public void PlayGame()
    {
        // get the next scene in the queue
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        // quit the game
        Application.Quit();
    }

    public void SetNickname(string nick)
    {
        if (nick == "") {
            nick = "Nickname";
        }

        nickname = nick;
    }

    // set text speed for npcs
    public void SetTextSpeed(System.Single speed)
    {
        textSpeed = (int) speed;
    }
}
