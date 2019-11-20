using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MPScript : MonoBehaviour
{
    public float MP = 0f;
    //private CharacterAnimation animationScript;
    private EnemyMovement enemyMovement;

    private bool characterDied;

    public bool is_Player;
    private MPUI MP_UI;
    float regen = 0.5f; 

    private void Awake()
    {
        //animationScript = GetComponentInChildren<CharacterAnimation>();
        if(is_Player)
        {
            MP_UI = GetComponent<MPUI>();
        }
    }

    private void Update()
    {
        //
        MP += regen * Time.deltaTime;
        MP_UI.DisplayMP(MP);
        if (MP > 100)
        {
            MP = 100;
            //do some super move here
        }
    }

    public void GetMP(float MPPoint)
    {
        //if (characterDied)
        //    return;

        MP += MPPoint;

        //display MP UI

        MP_UI.DisplayMP(MP);

        //TODO: Implement the mechanics for a super move

    }

}
