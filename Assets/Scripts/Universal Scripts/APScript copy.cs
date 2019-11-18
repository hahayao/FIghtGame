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

    private void Awake()
    {
        animationScript = GetComponentInChildren<CharacterAnimation>();
        if(is_Player)
        {
            AP_UI = GetComponent<APUI>();
        }
        
    }

    public void ApplyDamage(float damage, bool knockDown)
    {
        if (characterDied)
            return;

        AP -= damage;

        //display AP UI
        if(is_Player)
        {
            AP_UI.DisplayAP(AP);
        }
        
        if (AP <= 0f)
        {
            animationScript.Death();
            characterDied = true;

            //if it is the player deactivate enemy script
            if (is_Player)
            {
                GameObject.FindWithTag(Tags.ENEMY_TAG).GetComponent<EnemyMovement>().enabled = false;
            }
            return;
        }
        if (!is_Player)
        {
            if (knockDown)
            {
                //50%
                if (Random.Range(0, 2) > 0)
                {
                    animationScript.KnockDown();
                }
            }
            else
            {
                //33%
                if (Random.Range(0, 3) > 1)
                {
                    animationScript.Hit();
                }
            }

        }

    }

}
