using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Minimap : MonoBehaviour
{
    public Camera minimapCamera;

    // decrease the orthographic size, making the fov smaller for the minimap
    public void IncreaseMagnification()
    {
        if (minimapCamera.orthographicSize > 5)
        {
            minimapCamera.orthographicSize -= 1;
        }
    }

    // increase the orthographic size, making the fov larger for the minimap
    public void DecreaseMagnification()
    {
        if (minimapCamera.orthographicSize < 17)
        {
            minimapCamera.orthographicSize += 1;
        }
    }
}
