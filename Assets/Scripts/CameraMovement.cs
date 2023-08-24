using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform target;
    public float speed;

    void LateUpdate()
    {
        transform.position = Vector2.Lerp((Vector2) transform.position, (Vector2) target.transform.position, speed * Time.deltaTime);
        transform.position = new Vector3 (transform.position.x, transform.position.y, -10);
    }
}
