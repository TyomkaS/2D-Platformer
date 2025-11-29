using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputPanel : MonoBehaviour
{
    //[SerializeField] GameObject entartPasswordPanel;
    [SerializeField] TextMeshProUGUI valueText;
    [SerializeField] Button plus;
    [SerializeField] Button minus;

    private int value;

    private void Awake()
    {
        System.Random random = new System.Random();
        value = random.Next(0, 10);
        valueText.text = value.ToString();
    }
    public int GetValue()
    {
        return value;
    }

    public void PlusPressed()
    {
        if (value != 9)
        {
            value++;
        }
        else
        {
            value = 0;
        }
        valueText.text = value.ToString();
    }

    public void MinusPressed()
    {
        if (value != 0)
        {
            value--;
        }
        else
        {
            value = 9;
        }
        valueText.text = value.ToString();
    }

}
