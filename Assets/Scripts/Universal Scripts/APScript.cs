using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class APScript : MonoBehaviour
{
    public float AP = 100f;
    private CharacterAnimation animationScript;
    private EnemyMovement enemyMovement;

    private bool characterDied;

    public bool is_Player;
    private APUI AP_UI;
    // value for regenerate Rate
    public float regenRate = 100f;

    private void Awake()
    {
        animationScript = GetComponentInChildren<CharacterAnimation>();
        if(is_Player)
        {
            AP_UI = GetComponent<APUI>();
        }
        
    }

    private void Update()
    {
        
        AP += regenRate * Time.deltaTime;
        AP_UI.DisplayAP(AP);
        if (AP > 100)
        {
            AP = 100;
        }
        
    }

    public void UseAP(float amount)
    {
        if (characterDied)
            return;

        AP -= amount;

        if (AP < 0)
        {
            
        }

        //display AP UI

        AP_UI.DisplayAP(AP);

        
  /*      if (AP <= 0f)
        {
            //penalize player for using all AP
        }*/


    }

}
