using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ComboStateAR
{
    NONE,
    PUNCH_1,
    PUNCH_2,
    PUNCH_3,
    KICK_1,
    KICK_2
}

public class ARAttack : MonoBehaviour
{
    private CharacterAnimation player_Anim;

    private bool activateTimerToReset;

    private float default_Combo_Timer = 0.4f;
    private float current_Combo_Timer;

    public GameObject kickBt;
    public GameObject punchBt;
    protected Button buttonKick;
    protected Button buttonPunch;

    public float APPunch = 10.0f;
    public float APKick = 11.0f;

    private ComboStateAR current_Combo_state;

    private void Start()
    {
        current_Combo_Timer = default_Combo_Timer;
        current_Combo_state = ComboStateAR.NONE;
        buttonKick = kickBt.GetComponent<Button>();
        buttonPunch = punchBt.GetComponent<Button>();
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

    public void ComboAttacks()
    {
        if (buttonPunch.Pressed)
        {
            //avoid state overflow to kick action when click keyboard too fast
            if (current_Combo_state == ComboStateAR.PUNCH_3 ||
                current_Combo_state == ComboStateAR.KICK_1 ||
                current_Combo_state == ComboStateAR.KICK_2)
            {
                return;
            }
            buttonPunch.Pressed = false;
            current_Combo_state++;
            activateTimerToReset = true;
            current_Combo_Timer = default_Combo_Timer;

            if (current_Combo_state == ComboStateAR.PUNCH_1)
            {
                player_Anim.Punch_1();
            }
            
            if (current_Combo_state == ComboStateAR.PUNCH_2)
            {
                player_Anim.Punch_2();

            }

            if (current_Combo_state == ComboStateAR.PUNCH_3)
            {
                player_Anim.Punch_3();
            }
            //decrease AP
            GetComponent<APScript>().UseAP(APPunch);
            //increase MP
            GetComponent<MPScript>().GetMP(1);
        }
        if (buttonKick.Pressed)
        {
            buttonKick.Pressed = false;
            //if cur is punch3 or kick2 => return => have no combo to perform
            if (current_Combo_state == ComboStateAR.KICK_2 ||
                current_Combo_state == ComboStateAR.PUNCH_3)
            {
                return;
            }
            //set current combo state to kick1 to chain the combo
            if (current_Combo_state == ComboStateAR.NONE ||
                current_Combo_state == ComboStateAR.PUNCH_1 ||
                current_Combo_state == ComboStateAR.PUNCH_2)
            {
                current_Combo_state = ComboStateAR.KICK_1;
            }
            //move to kick2
            else if (current_Combo_state == ComboStateAR.KICK_1)
            {
                current_Combo_state++;
            }

            activateTimerToReset = true;
            current_Combo_Timer = default_Combo_Timer;

            if (current_Combo_state == ComboStateAR.KICK_1)
            {
                player_Anim.Kick_1();
            }
            if (current_Combo_state == ComboStateAR.KICK_2)
            {
                player_Anim.Kick_2();
            }
            GetComponent<APScript>().UseAP(APKick);
            //increase MP
            GetComponent<MPScript>().GetMP(1);
        }
    }

    void ResetComboState()
    {
        if (activateTimerToReset)
        {
            current_Combo_Timer -= Time.deltaTime;
            if (current_Combo_Timer <= 0f)
            {
                current_Combo_state = ComboStateAR.NONE;
                activateTimerToReset = false;
                current_Combo_Timer = default_Combo_Timer;
            }
        }
    }
}
