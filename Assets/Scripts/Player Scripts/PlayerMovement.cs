using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    private CharacterAnimation player_Anim;
    private Rigidbody mybody;

    public float walk_Speed = 2f;
    public float z_Speed = 1.5f;

    //make two players face to face
    private float rotation_Y = 90f;
    //rotate player speed
    private float rotation_Speed = 15f;

    void Awake()
    {
        mybody = GetComponent<Rigidbody>();
        player_Anim = GetComponentInChildren<CharacterAnimation>();
    }

    // get keyboard input
    void Update()
    {
        RotatePlayer();
        AnimatePlayerWalk();
    }

    void FixedUpdate()
    {
        DetectMovement();
    }

    void DetectMovement()
    {
        mybody.velocity = new Vector3(
            Input.GetAxisRaw(Axis.HORIZONTAL_AXIS) * (-walk_Speed),
            mybody.velocity.y,
            Input.GetAxisRaw(Axis.VERTICAL_AXIS) * (-z_Speed));
    }

    void RotatePlayer()
    {
        //go to right side, see right side
        if(Input.GetAxisRaw(Axis.HORIZONTAL_AXIS) > 0)
        {
            transform.rotation = Quaternion.Euler(0f, -Mathf.Abs(rotation_Y), 0f);
        }
        //go to left side, see left side
        else if(Input.GetAxisRaw(Axis.HORIZONTAL_AXIS) < 0)
        {
            transform.rotation = Quaternion.Euler(0f, Mathf.Abs(rotation_Y), 0f);
        }
    }

    void AnimatePlayerWalk()
    {
        if(Input.GetAxisRaw(Axis.HORIZONTAL_AXIS) != 0 ||
            Input.GetAxisRaw(Axis.VERTICAL_AXIS) != 0)
            {
            //call walk animation, also edit animation exit time, if it has no condition
            //better to set exit time to make the animation full display
            player_Anim.Walk(true);
        }
        else
        {
            player_Anim.Walk(false);
        }
    }
}
