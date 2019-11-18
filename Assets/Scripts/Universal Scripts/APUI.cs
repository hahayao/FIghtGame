using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class APUI : MonoBehaviour
{
    private Image AP_UI;

    private void Awake()
    {
        AP_UI = GameObject.FindWithTag(Tags.AP_UI).GetComponent<Image>();
    }

    public void DisplayAP(float value)
    {
        value /= 100f;
        if (value < 0f)
            value = 0f;
        AP_UI.fillAmount = value;
    }
}
