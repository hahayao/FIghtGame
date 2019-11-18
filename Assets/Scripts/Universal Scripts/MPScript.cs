using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MPScript : MonoBehaviour
{
    public float MP = 100f;
    private CharacterAnimation animationScript;
    private EnemyMovement enemyMovement;

    private bool characterDied;

    public bool is_Player;
    private MPUI MP_UI;

    private void Awake()
    {
        animationScript = GetComponentInChildren<CharacterAnimation>();
        if(is_Player)
        {
            MP_UI = GetComponent<MPUI>();
        }
        
    }

    public void ApplyDamage(float damage, bool knockDown)
    {
        //if (characterDied)
        //    return;

        MP -= damage;

        //display MP UI
        if(is_Player)
        {
            MP_UI.DisplayMP(MP);
        }

        //TODO: Implement the mechanics for a super move
        if (MP <= 0f)
        {
            //animationScript.Death();
            //characterDied = true;

            //if it is the player deactivate enemy script
            //if (is_Player)
            //{
            //    GameObject.FindWithTag(Tags.ENEMY_TAG).GetComponent<EnemyMovement>().enabled = false;
            //}
            //return;
        }

    }

}
