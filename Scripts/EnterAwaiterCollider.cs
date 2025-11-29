using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EnterAwaiterCollider : MonoBehaviour
{
    public Action<Collider> OnEnter;
    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("On triegger eneter");
        OnEnter?.Invoke(other);
    }
}
