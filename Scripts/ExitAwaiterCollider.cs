using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ExitAwaiterCollider : MonoBehaviour
{
    public Action<Collider> OnExit;
    private void OnTriggerExit(Collider other)
    {
        //Debug.Log("On triegger exit");
        OnExit?.Invoke(other);
    }
}
