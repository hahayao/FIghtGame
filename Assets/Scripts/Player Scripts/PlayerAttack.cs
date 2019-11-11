using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ComboState
{
    NONE,
    PUNCH_1,
    PUNCH_2,
    PUNCH_3,
    KICK_1,
    KICK_2
}

public class PlayerAttack : MonoBehaviour
{
    private CharacterAnimation player_Anim;

    private bool activateTimerToReset;

    private float default_Combo_Timer = 0.4f;
    private float current_Combo_Timer;

    private ComboState current_Combo_state;

    private void Start()
    {
        current_Combo_Timer = default_Combo_Timer;
        current_Combo_state = ComboState.NONE;
    }

    private void Awake()
    {
        player_Anim = GetComponentInChildren<CharacterAnimation>();
    }

    // Update is called once per frame
    void Update()
    {
        ComboAttacks();
        ResetComboState();
    }

    void ComboAttacks()
    {
        if(Input.GetKeyDown(KeyCode.Z))
        {
            //avoid state overflow to kick action when click keyboard too fast
            if(current_Combo_state == ComboState.PUNCH_3||
                current_Combo_state == ComboState.KICK_1||
                current_Combo_state == ComboState.KICK_2)
            {
                return;
            }

            current_Combo_state++;
            activateTimerToReset = true;
            current_Combo_Timer = default_Combo_Timer;

            if(current_Combo_state == ComboState.PUNCH_1)
            {
                player_Anim.Punch_1();
            }

            if (current_Combo_state == ComboState.PUNCH_2)
            {
                player_Anim.Punch_2();
            }

            if (current_Combo_state == ComboState.PUNCH_3)
            {
                player_Anim.Punch_3();
            }
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            //if cur is punch3 or kick2 => return => have no combo to perform
            if(current_Combo_state == ComboState.KICK_2 ||
                current_Combo_state == ComboState.PUNCH_3)
            {
                return;
            }
            //set current combo state to kick1 to chain the combo
            if(current_Combo_state == ComboState.NONE ||
                current_Combo_state == ComboState.PUNCH_1 ||
                current_Combo_state == ComboState.PUNCH_2)
            {
                current_Combo_state = ComboState.KICK_1;
            }
            //move to kick2
            else if(current_Combo_state == ComboState.KICK_1)
            {
                current_Combo_state++;
            }

            activateTimerToReset = true;
            current_Combo_Timer = default_Combo_Timer;

            if(current_Combo_state == ComboState.KICK_1)
            {
                player_Anim.Kick_1();
            }
            if(current_Combo_state == ComboState.KICK_2)
            {
                player_Anim.Kick_2();
            }
            
        }
    }

    void ResetComboState()
    {
        if(activateTimerToReset)
        {
            current_Combo_Timer -= Time.deltaTime;
            if(current_Combo_Timer <= 0f)
            {
                current_Combo_state = ComboState.NONE;
                activateTimerToReset = false;
                current_Combo_Timer = default_Combo_Timer;
            }
        }
    }
}
