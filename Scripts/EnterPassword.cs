using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Assets.Scripts;
using System;

public class EnterPassword : MonoBehaviour
{
    [SerializeField] private List<GameObject> inputs = new List<GameObject>();
    private List<InputPanel> panels;

    public Action<string> OnEntered;

    private void Awake()
    {
        panels = new List<InputPanel>(inputs.Count);

        for (int i = 0; i < inputs.Count; i++)
        {
            if (inputs[i]!=null)
            {
                panels.Add(inputs[i].GetComponent<InputPanel>());
            }
        }
    }

    public void EnterPressed()
    {
        string value = "";
        for (int i = 0; i < panels.Count; i++)
        {
            if (panels[i]!=null)
            {
                value += panels[i].GetValue().ToString();
            }
        }

        //Debug.Log("Entere password is " + value);

        OnEntered?.Invoke(value);
    }
}
