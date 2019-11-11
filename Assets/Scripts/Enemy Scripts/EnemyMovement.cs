using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    private CharacterAnimation enemyAnim;

    private Rigidbody myBody;
    public float speed = 5f;

    private Transform playerTarget;

    public float attack_Distance = 1f;
    public float chase_Player_After_Attack = 1f;

    private float current_Attack_Time;
    private float default_Attack_Time = 2f;

    private bool followPlayer;
    private bool attackPlayer;

    private void Awake()
    {
        enemyAnim = GetComponentInChildren<CharacterAnimation>();
        myBody = GetComponent<Rigidbody>();
        playerTarget = GameObject.FindWithTag(Tags.PLAYER_TAG).transform;
    }

    void Start()
    {
        //when start, follow it & attack it immediately
        followPlayer = true;
        current_Attack_Time = default_Attack_Time;
    }

    // Update is called once per frame
    void Update()
    {
        Attack();
    }

    private void FixedUpdate()
    {
        FollowTarget();
    }

    void FollowTarget()
    {
        //if we are not supposed to follow the player
        if (!followPlayer)
            return;
        //distance between two players
        if (Vector3.Distance(transform.position, playerTarget.position) > attack_Distance)
        {
            transform.LookAt(playerTarget);
            myBody.velocity = transform.forward * speed;

            //check if it is move
            if (myBody.velocity.sqrMagnitude != 0)
            {
                enemyAnim.Walk(true);
            }
        }
        else if (Vector3.Distance(transform.position, playerTarget.position) <= attack_Distance)
        {
            myBody.velocity = Vector3.zero;
            enemyAnim.Walk(false);

            followPlayer = false;
            attackPlayer = true;
        }
    }

    void Attack()
    {
        if (!attackPlayer)
            return;

        current_Attack_Time += Time.deltaTime;

        if(current_Attack_Time > default_Attack_Time)
        {
            //return 0,1,2
            enemyAnim.EnemyAttack(Random.Range(0, 3));
            //reset time, wait and attack
            current_Attack_Time = 0f;
        }
        //add chase distance, if beyond this value, cannot attck anymore, need to do follow
        if(Vector3.Distance(transform.position, playerTarget.position) >
            attack_Distance + chase_Player_After_Attack)
            {
            attackPlayer = false;
            followPlayer = true;
        }
    }
}
