using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthBar : MonoBehaviour
{
    public Image fill;
    public Image fillDelayed;
    private int maxValue;

    public void SetMaxHealth(int health)
    {
        maxValue = health;

        fill.fillAmount = 1;
    }

    public void SetHealth(int health)
    {
        StartCoroutine(SetDelayedHealth(health));
    }

    IEnumerator SetDelayedHealth(int health)
    {
        fill.fillAmount = (float) health / (float) maxValue;

        yield return new WaitForSeconds(0.5f);

        fillDelayed.fillAmount = (float) health / (float) maxValue;
    }
}
