using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MPUI : MonoBehaviour
{
    private Image MP_UI;

    private void Awake()
    {
        MP_UI = GameObject.FindWithTag(Tags.MP_UI).GetComponent<Image>();
    }

    public void DisplayMP(float value)
    {
        value /= 100f;
        if (value < 0f)
            value = 0f;
        MP_UI.fillAmount = value;
    }
}
