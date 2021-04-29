using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstanceBall : MonoBehaviour
{
    [Tooltip("Used to determine if the ball master can interact with this ball")]
    public bool Selectable = true;

    public void Reset()
    {
        Selectable = true;
    }

    public void Update()
    {
        Debug.DrawLine(gameObject.transform.position, gameObject.transform.position + gameObject.transform.rotation.eulerAngles, Color.green);
        Debug.DrawLine(gameObject.transform.position, gameObject.transform.position + gameObject.transform.localRotation.eulerAngles, Color.red);
    }
}
