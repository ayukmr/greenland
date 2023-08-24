using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    public string type;

    public string GetCollectibleType()
    {
        return type;
    }

    public void CollectCollectible()
    {
        Destroy(gameObject);
    }
}
