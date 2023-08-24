using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTarget : MonoBehaviour
{
    public GameObject target;
    private float startZPosition;

    void Start()
    {
        startZPosition = transform.position.z;
    }

    void Update()
    {
        transform.position = target.transform.position;
        transform.position = new Vector3 (transform.position.x, transform.position.y, startZPosition);
    }
}
